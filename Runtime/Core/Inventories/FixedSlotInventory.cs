using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// An inventory with a fixed number of slots. Attempting to add more items when full will fail. Slots may be null if empty.
    /// </summary>
    [Serializable]
    public sealed class FixedSlotInventory : Inventory
    {
        [SerializeField] private List<InventorySlot> slots = new List<InventorySlot>();
        public override IEnumerable<InventorySlot> Slots => slots;
        public override InventorySlot GetSlot(int id) => slots[id];
        public override int SlotCount => slots.Count;

        public FixedSlotInventory() { }
        public FixedSlotInventory(int size)
        {
            for (int i = 0; i < size; ++i) slots.Add(new InventorySlot(i));
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

            //Then try to merge into empty slots
            foreach (InventorySlot slot in slots)
            {
                if (slot.IsEmpty && slot.CanAccept(newStack)) slot.TryAccept(newStack);
                if (newStack.quantity == 0) return;
            }

            //We couldn't accept the full stack
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

        public override IEnumerable<ItemStack> TryRemove(Predicate<ItemStack> filter, int totalToRemove)
        {
            List<ItemStack> @out = new List<ItemStack>();

            for (int i = 0; i < slots.Count; ++i)
            {
                if (!slots[i].IsEmpty && filter(slots[i].Contents))
                {
                    if (slots[i].Contents.quantity >= totalToRemove)
                    {
                        //This stack is enough to complete requirements. Stop consuming.
                        ItemStack tmp = slots[i].Contents.Clone();
                        tmp.quantity = totalToRemove;
                        @out.Add(tmp);
                        slots[i].Contents.quantity -= totalToRemove;
                        return @out;
                    }
                    else
                    {
                        //This stack is not enough to complete requirements. Continue consuming.
                        totalToRemove -= slots[i].Contents.quantity;
                        @out.Add(slots[i].Contents);
                        slots[i].Contents = null;
                    }
                }
            }

            throw new InvalidOperationException("Counted sufficient items, but somehow didn't have enough. This should never happen!");
        }

        public override int RemoveAll(Predicate<ItemStack> filter)
        {
            bool wrappedFilter(InventorySlot i) => !i.IsEmpty && filter(i.Contents);
            int nRemoved = slots.Where(wrappedFilter).Sum(i => i.Contents.quantity);
            foreach (InventorySlot slot in slots.Where(wrappedFilter)) slot.Contents = null;
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