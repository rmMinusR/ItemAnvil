using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// An inventory that automatically expands and shrinks to fit its contents. All slots are guaranteed valid.
    /// </summary>
    [Serializable]
    public sealed class CondensingInventory : Inventory
    {
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
        public override IEnumerable<InventorySlot> Slots => slots;
        public override InventorySlot GetSlot(int id) => slots[id];
        public override int SlotCount => slots.Count;

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
        /// 
        public override void AddItem(ItemStack newStack)
        {
            if (ItemStack.IsEmpty(newStack)) throw new ArgumentException("Cannot add nothing!");
            
            //First try to merge with existing stacks
            foreach (InventorySlot slot in slots)
            {
                if (!slot.IsEmpty && slot.CanAccept(newStack)) slot.TryAccept(newStack);
                if (newStack.quantity == 0) return;
            }

            //Then try adding slots until we deplete the stack
            //Not a great fix, but it plays nice with stacking rules
            while (newStack.quantity > 0)
            {
                InventorySlot newSlot = AddSlot();
                newSlot.TryAccept(newStack);
            }

            //We couldn't accept the full stack
        }

        public override IEnumerable<ItemStack> TryRemove(Predicate<ItemStack> filter, int totalToRemove)
        {
            List<InventorySlot> matches = slots.Where(i => !i.IsEmpty && filter(i.Contents)).ToList();

            //NOTE: Not threadsafe

            List<ItemStack> @out = new List<ItemStack>();

            //Make sure we have enough
            int itemsAvailable = matches.Sum(stack => stack.Contents.quantity);
            if (itemsAvailable < totalToRemove) return @out;
            else
            {
                //Removal routine
                while (totalToRemove > 0)
                {
                    if(matches[0].Contents.quantity >= totalToRemove)
                    {
                        //If we would be able to take enough from the current stack, finish routine
                        ItemStack tmp = matches[0].Contents.Clone();
                        tmp.quantity = totalToRemove;
                        @out.Add(tmp);
                        matches[0].Contents.quantity -= totalToRemove;
                        if (matches[0].IsEmpty) slots.Remove(matches[0]);
                        totalToRemove = 0;
                        break;
                    }
                    else
                    {
                        //If we wouldn't be able to take enough from the current stack, take what we can and continue
                        //FIXME breaks for keep-if-zero
                        @out.Add(matches[0].Contents.Clone());
                        totalToRemove -= matches[0].Contents.quantity;
                        matches[0].Contents.quantity = 0;
                        slots.Remove(matches[0]);
                        matches.RemoveAt(0);
                    }
                }

                //We likely removed slots. Make sure every slot's ID is up to date.
                ValidateIDs();

                return @out;
            }
        }

        public override int RemoveAll(Predicate<ItemStack> filter)
        {
            bool wrappedFilter(InventorySlot i) => !i.IsEmpty && filter(i.Contents);
            int nRemoved = slots.Where(wrappedFilter).Sum(i => i.Contents.quantity);
            slots.RemoveAll(wrappedFilter);
            if (nRemoved != 0) ValidateIDs();
            return nRemoved;
        }
        
        /// <summary>
        /// Check how many items match the given filter
        /// </summary>
        public override int Count(Predicate<ItemStack> filter) => slots.Where(i => filter(i.Contents)).Sum(stack => stack.Contents.quantity);

        /// <summary>
        /// Dump the contents of this inventory. Note that these the original instances.
        /// </summary>
        public override IEnumerable<ReadOnlyItemStack> GetContents() => slots.Where(i => !i.IsEmpty).Select(i => i.Contents);

        /// <summary>
        /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
        /// </summary>
        public override List<ItemStack> CloneContents()
        {
            List<ItemStack> list = new List<ItemStack>();
            foreach (InventorySlot s in slots) list.Add(s.Contents.Clone());
            return list;
        }

        /// <summary>
        /// Find all ItemStacks that match the filter
        /// </summary>
        public override IEnumerable<ItemStack> FindAll(Predicate<ItemStack> filter) => slots.Where(i => filter(i.Contents)).Select(i => i.Contents);

        public override void Sort(IComparer<ReadOnlyItemStack> comparer)
        {
            slots.Sort(new ItemStackToSlotComparer(comparer));
            ValidateIDs();
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

                for (int i = 0; i < contents.Count; ++i) AddSlot().Contents = contents[i];
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