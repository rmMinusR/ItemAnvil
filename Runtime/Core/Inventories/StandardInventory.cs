using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// A generic inventory that can be customized via InventoryProperties.
    /// </summary>
    [Serializable]
    public sealed class StandardInventory : Inventory
    {
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
        public override IEnumerable<InventorySlot> Slots => slots;
        public override InventorySlot GetSlot(int id) => slots[id];
        public override int SlotCount => slots.Count;

        public StandardInventory() { }
        public StandardInventory(int size)
        {
            for (int i = 0; i < size; ++i) slots.Add(new InventorySlot(i));
        }

        public InventorySlot AddSlot()
        {
            InventorySlot s = new InventorySlot(slots.Count);
            slots.Add(s);
            return s;
        }

        /// <summary>
        /// Add an item using an ItemStack
        /// </summary>
        /// <param name="newStack">Stack to add</param>
        /// <param name="cause"></param>
        public override void AddItem(ItemStack newStack, object cause)
        {
            if (ItemStack.IsEmpty(newStack)) throw new ArgumentException("Cannot add nothing!");

            //Check hooks to see if we're allowed to add this item
            if (Hooks.ExecuteAddItem(newStack, newStack.Clone(), cause) != EventResult.Allow) return;

            //First try to merge with existing stacks
            foreach (InventorySlot slot in slots)
            {
                //Check hooks to see if slot can accept this stack
                if (!slot.IsEmpty && slot.CanAccept(newStack) && Hooks.ExecuteCanSlotAccept(slot, newStack, cause) == EventResult.Allow) slot.TryAccept(newStack);
                if (newStack.quantity == 0) return;
            }

            //Then try to merge into empty slots
            foreach (InventorySlot slot in slots)
            {
                //Check hooks to see if slot can accept this stack 
                if (slot.IsEmpty && slot.CanAccept(newStack) && Hooks.ExecuteCanSlotAccept(slot, newStack, cause) == EventResult.Allow) slot.TryAccept(newStack);
                if (newStack.quantity == 0) return;
            }

            //We couldn't accept the full stack, run appropriate hook
            Hooks.ExecutePostAddItem(newStack, cause);
        }

        /// <summary>
        /// Dump the contents of this inventory. Note that these the original instances.
        /// </summary>
        public override IEnumerable<ReadOnlyItemStack> GetContents()
        {
            return slots.Where(i => !i.IsEmpty).Select(i => i.Contents);
        }

        /// <summary>
        /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
        /// </summary>
        public override List<ItemStack> CloneContents()
        {
            return slots.Where(i => !i.IsEmpty).Select(i => i.Contents.Clone()).ToList();
        }

        /// <summary>
        /// Attempt to remove items. If not enough are available, no changes will be made.
        /// </summary>
        /// <param name="typeToRemove">Item type to be removed</param>
        /// <param name="totalToRemove">How many to be removed</param>
        /// <param name="cause">User-defined data to provide additional context</param>
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
        public override IEnumerable<ItemStack> TryRemove(Predicate<ItemStack> filter, int totalToRemove, object cause)
        {
            //Item removal routine
            List<(ItemStack, InventorySlot)> everythingRemoved = new List<(ItemStack, InventorySlot)>();
            for (int i = 0; i < slots.Count; ++i)
            {
                if (!slots[i].IsEmpty && filter(slots[i].Contents))
                {
                    //Create dummy removedStack
                    ItemStack removedStack = slots[i].Contents.Clone();
                    removedStack.quantity = Mathf.Min(totalToRemove, slots[i].Contents.quantity);

                    //Check hook to see if we can continue, then check to make sure no hooks are up to funny business
                    if (Hooks.ExecuteRemoveItems(slots[i], removedStack, removedStack.Clone(), cause) == EventResult.Allow && removedStack.quantity > 0)
                    {
                        //Make sure we aren't overcharging and leaving the slot with negative quantities
                        removedStack.quantity = Mathf.Min(removedStack.quantity, slots[i].Contents.quantity);

                        //Log it
                        everythingRemoved.Add((removedStack, slots[i]));

                        //Update slot
                        slots[i].Contents.quantity -= removedStack.quantity;
                        if (slots[i].Contents.quantity <= 0) slots[i].Contents = null;
                        
                        //Update totalToRemove
                        totalToRemove -= removedStack.quantity;
                        if (totalToRemove <= 0) break;
                    }
                }
            }

            //If we still have things to remove, continue
            if (totalToRemove > 0)
            {
                //Prevent covariants
                foreach ((ItemStack stack, InventorySlot slot) in everythingRemoved)
                {
                    //Should not call add hook here, since we're reverting a failed operation
                    slot.TryAccept(stack); //Merge back in (TODO: make more performant?)
                }

                //Complain
                throw new InvalidOperationException("Counted sufficient items, but somehow didn't have enough. This should never happen!");
            }
            else return everythingRemoved.Select(i => i.Item1);
        }

        /// <summary>
        /// Remove all items that match the given filter
        /// </summary>
        /// <returns>How many items were removed</returns>
        public override int RemoveAll(Predicate<ItemStack> filter, object cause)
        {
            int nRemoved = 0;

            for (int i = 0; i < slots.Count; ++i)
            {
                if (!slots[i].IsEmpty && filter(slots[i].Contents))
                {
                    //Create dummy removedStack
                    ItemStack removedStack = slots[i].Contents.Clone();
                    
                    //Check hook to see if we can continue, then check to make sure no hooks are up to funny business
                    if (Hooks.ExecuteRemoveItems(slots[i], removedStack, removedStack.Clone(), cause) == EventResult.Allow && removedStack.quantity > 0)
                    {
                        //Make sure we aren't overcharging and leaving the slot with negative quantities
                        removedStack.quantity = Mathf.Min(removedStack.quantity, slots[i].Contents.quantity);

                        //Log it
                        nRemoved += removedStack.quantity;

                        //Update slot
                        slots[i].Contents.quantity -= removedStack.quantity;
                        if (slots[i].Contents.quantity <= 0) slots[i].Contents = null;
                    }
                }
            }

            return nRemoved;
        }

        /// <summary>
        /// Check how many items match the given filter
        /// </summary>
        public override int Count(Predicate<ItemStack> filter) => slots.Where(i => !i.IsEmpty && filter(i.Contents)).Sum(i => i.Contents.quantity);

        /// <summary>
        /// Find all ItemStacks that match the filter
        /// </summary>
        public override IEnumerable<ItemStack> FindAll(Predicate<ItemStack> filter) => slots.Where(i => !i.IsEmpty && filter(i.Contents)).Select(i => i.Contents);

        public override void Sort(IComparer<ReadOnlyItemStack> comparer, object cause)
        {
            //Find what slots can be sorted, and sort them
            List<InventorySlot> sortables = new List<InventorySlot>(slots);
            sortables.RemoveAll(slot => Hooks.ExecuteTrySort(slot, cause) != EventResult.Allow);
            sortables.Sort(new ItemStackToSlotComparer(comparer));

            //Rearrange the sortable set in the original slots
            //This is all legal since it's just references
            int sortableIndex = 0;
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (sortables.Contains(slots[slotIndex]))
                {
                    slots[slotIndex] = sortables[sortableIndex];
                    sortableIndex++;
                }
            }

            ValidateIDs();

            Hooks.ExecutePostSort(cause);
        }

        #region Hook interface
        [SerializeField, HideInInspector] private InventoryHooksImplDetail _hooks;
        private InventoryHooksImplDetail Hooks => _hooks != null ? _hooks : (_hooks = ScriptableObject.CreateInstance<InventoryHooksImplDetail>());
        public override void Hook(AddItemHook       listener, int priority) => Hooks.addItem      .InsertHook(listener, priority);
        public override void Hook(CanSlotAcceptHook listener, int priority) => Hooks.canSlotAccept.InsertHook(listener, priority);
        public override void Hook(PostAddItemHook   listener, int priority) => Hooks.postAddItem  .InsertHook(listener, priority);
        public override void Hook(RemoveItemHook    listener, int priority) => Hooks.removeItem   .InsertHook(listener, priority);
        public override void Hook(TrySortSlotHook   listener, int priority) => Hooks.trySortSlot  .InsertHook(listener, priority);
        public override void Hook(PostSortHook      listener, int priority) => Hooks.postSort     .InsertHook(listener, priority);
        public override void Unhook(AddItemHook       listener) => Hooks.addItem      .RemoveHook(listener);
        public override void Unhook(CanSlotAcceptHook listener) => Hooks.canSlotAccept.RemoveHook(listener);
        public override void Unhook(PostAddItemHook   listener) => Hooks.postAddItem  .RemoveHook(listener);
        public override void Unhook(RemoveItemHook    listener) => Hooks.removeItem   .RemoveHook(listener);
        public override void Unhook(TrySortSlotHook   listener) => Hooks.trySortSlot  .RemoveHook(listener);
        public override void Unhook(PostSortHook      listener) => Hooks.postSort     .RemoveHook(listener);
        #endregion


        #region Obsolete functions/variables, and upgrader

        private void ValidateIDs()
        {
            //Ensure slots have correct IDs
            for (int i = 0; i < slots.Count; ++i) slots[i].ID = i;
        }

        public override void Validate()
        {
            //Update obsolete members
#pragma warning disable CS0612
            if (contents.Count != 0)
            {
                if (slots.Count != 0) throw new InvalidOperationException("Cannot update storage -- destination 'slots' must be empty");

                for (int i = 0; i < contents.Count; ++i)
                {
                    InventorySlot s = new InventorySlot(i);
                    s.Contents = contents[i];
                    slots.Add(s);
                }
                contents.Clear();

            }
#pragma warning restore CS0612

            ValidateIDs();
        }

        /// <summary>
        /// OUTDATED as of 0.5.1 - use "slots" instead
        /// Validate() should handle the upgrade gracefully
        /// </summary>
        [HideInInspector, Obsolete]
        [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

        #endregion
    }

}