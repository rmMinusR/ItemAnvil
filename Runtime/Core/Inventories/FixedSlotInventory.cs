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

        #region TryRemove family

        /// <summary>
        /// Attempt to remove items. If not enough are available, no changes will be made.
        /// </summary>
        /// <param name="filter">Filter specifying what to remove</param>
        /// <param name="totalToRemove">How many to be removed</param>
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
        public override IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove)
        {
            //Make sure we have enough
            if (Count(filter) < totalToRemove) return new List<ItemStack>();

            return TryRemove_Impl(filter.Matches, totalToRemove);
        }

        /// <summary>
        /// Attempt to remove items. If not enough are available, no changes will be made.
        /// </summary>
        /// <param name="typeToRemove">Item type to be removed</param>
        /// <param name="totalToRemove">How many to be removed</param>
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
        public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove)
        {
            //Make sure we have enough
            if (Count(typeToRemove) < totalToRemove) return new List<ItemStack>();

            return TryRemove_Impl(s => s.itemType == typeToRemove, totalToRemove);
        }

        private IEnumerable<ItemStack> TryRemove_Impl(Func<ItemStack, bool> filter, int totalToRemove)
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

        #endregion

        #region RemoveAll family

        /// <summary>
        /// Remove all items that match the given filter
        /// </summary>
        /// <returns>How many items were removed</returns>
        public override int RemoveAll(ItemFilter filter) => RemoveAll_Impl(filter.Matches);

        /// <summary>
        /// Remove all items of the given type
        /// </summary>
        /// <returns>How many items were removed</returns>
        public override int RemoveAll(Item typeToRemove) => RemoveAll_Impl(s => s.itemType == typeToRemove);
    
        private int RemoveAll_Impl(Func<ItemStack, bool> filter)
        {
            int nRemoved = slots.Where(i => filter(i.Contents)).Sum(i => i.Contents.quantity);
            foreach (InventorySlot slot in slots.Where(i => filter(i.Contents))) slot.Contents = null;
            return nRemoved;
        }

        #endregion

        /// <summary>
        /// Check how many items match the given filter
        /// </summary>
        public override int Count(ItemFilter filter) => slots.Where(i => !i.IsEmpty && filter.Matches(i.Contents)).Sum(i => i.Contents.quantity);

        /// <summary>
        /// Check how many items are present of the given type
        /// </summary>
        public override int Count(Item itemType) => slots.Where(i => !i.IsEmpty && i.Contents.itemType == itemType).Sum(i => i.Contents.quantity);
    
        /// <summary>
        /// Find all ItemStacks that match the filter
        /// </summary>
        public override IEnumerable<ItemStack> FindAll(ItemFilter filter) => slots.Where(i => !i.IsEmpty).Select(i => i.Contents).Where(filter.Matches);

        /// <summary>
        /// Find all ItemStacks with the given type
        /// </summary>
        public override IEnumerable<ItemStack> FindAll(Item type) => slots.Where(i => !i.IsEmpty).Select(i => i.Contents).Where(i => i.itemType == type);

        public override void Sort(IComparer<ReadOnlyItemStack> comparer) => contents.Sort(comparer);


        #region Obsolete functions/variables, and upgrader

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

            //Ensure slots have correct IDs
            for (int i = 0; i < slots.Count; ++i) slots[i].ID = i;
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