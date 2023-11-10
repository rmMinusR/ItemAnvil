using System;
using System.Collections.Generic;
using System.Linq;
using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public abstract class Inventory
    {
        /// <summary>
        /// Attempt to add one of an item
        /// </summary>
        /// <param name="itemType">Type of the item to add</param>
        public virtual void AddItem(Item itemType, object cause) => AddItem(itemType, 1, cause);

        /// <summary>
        /// Attempt to add an item
        /// </summary>
        /// <param name="itemType">Type of the item to add</param>
        /// <param name="quantity">How many to add</param>
        public virtual void AddItem(Item itemType, int quantity, object cause) => AddItem(new ItemStack(itemType, quantity), cause);

        /// <summary>
        /// Attempt to add an ItemStack. If the stack can't be fully transferred, the ItemStack will be changed to reflect that.
        /// </summary>
        /// <param name="newStack">Stack to add</param>
        public abstract void AddItem(ItemStack newStack, object cause);

        /// <summary>
        /// Attempt to remove items. If not enough are available, no changes will be made.
        /// </summary>
        /// <param name="filter">Filter specifying what to remove</param>
        /// <param name="totalToRemove">How many to be removed</param>
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise null, and no changes were made.</returns>
        public abstract IEnumerable<ItemStack> TryRemove(Predicate<ItemStack> filter, int totalToRemove, object cause);
        public virtual IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove, object cause) => TryRemove(filter.Matches, totalToRemove, cause);
        public virtual IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove, object cause) => TryRemove(s => s.itemType == typeToRemove, totalToRemove, cause);

        /// <summary>
        /// Remove all items that match the given filter
        /// </summary>
        /// <returns>How many items were removed</returns>
        public abstract int RemoveAll(Predicate<ItemStack> filter, object cause);
        public virtual int RemoveAll(ItemFilter filter, object cause) => RemoveAll(filter.Matches, cause);
        public virtual int RemoveAll(Item typeToRemove, object cause) => RemoveAll(s => s.itemType == typeToRemove, cause);

        /// <summary>
        /// Check how many items match the given filter
        /// </summary>
        public abstract int Count(Predicate<ItemStack> filter);
        public virtual int Count(ItemFilter filter) => Count(filter.Matches);
        public virtual int Count(Item typeToCount)  => Count(s => s.itemType == typeToCount);

        /// <summary>
        /// Find the first ItemStack of the given type.
        /// </summary>
        /// <remarks>
        /// Note that these are the original instances, and changes made will reflect in the inventory.
        /// Note also that changes made will NOT trigger events.
        /// </remarks>
        /// <returns>The matching ItemStack if a match was present, else null</returns>
        public virtual ItemStack FindFirst(Predicate<ItemStack> filter) => FindAll(filter).FirstOrDefault();
        public virtual ItemStack FindFirst(ItemFilter filter) => FindFirst(filter.Matches);
        public virtual ItemStack FindFirst(Item typeToCount)  => FindFirst(s => s.itemType == typeToCount);

        /// <summary>
        /// Find all ItemStacks that match the filter.
        /// </summary>
        /// <remarks>
        /// Note that these are the original instances, and changes made will reflect in the inventory.
        /// Note also that changes made will NOT trigger events.
        /// </remarks>
        public abstract IEnumerable<ItemStack> FindAll(Predicate<ItemStack> filter);
        public virtual IEnumerable<ItemStack> FindAll(ItemFilter filter) => FindAll(filter.Matches);
        public virtual IEnumerable<ItemStack> FindAll(Item typeToCount)  => FindAll(s => s.itemType == typeToCount);
        
        /// <summary>
        /// Get a given slot, which can then be freely manipulated. Errors if outside the range [0, SlotCount)
        /// </summary>
        public abstract InventorySlot GetSlot(int id);

        /// <summary>
        /// How many slots are present, regardless of whether they are filled or not?
        /// </summary>
        public abstract int SlotCount { get; }

        public abstract IEnumerable<InventorySlot> Slots { get; }

        /// <summary>
        /// Dump the contents of this inventory.
        /// </summary>
        /// <remarks>
        /// Note that these are the original instances. If downcast, changes made will reflect in the inventory.
        /// Note also that changes made will NOT trigger events.
        /// </remarks>
        public abstract IEnumerable<ReadOnlyItemStack> GetContents();

        /// <summary>
        /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory's copy.
        /// </summary>
        public abstract List<ItemStack> CloneContents();
    
        public virtual void Tick(Component owningObject)
        {
            for (int i = 0; i < SlotCount; ++i)
            {
                InventorySlot slot = GetSlot(i);
                if (!slot.IsEmpty) foreach (ItemInstanceProperty p in slot.Contents.instanceProperties)
                {
                    p.Tick(slot, this, owningObject);
                }
            }
        }

        //NOTE: Comparers and heuristics must be null-safe!
        public abstract void Sort(IComparer<ReadOnlyItemStack> comparer, object cause);
        public virtual void Sort(Func<ReadOnlyItemStack, float> heuristic, object cause) => Sort(new HeuristicComparer(heuristic), cause);
        protected class HeuristicComparer : IComparer<ReadOnlyItemStack>
        {
            Func<ReadOnlyItemStack, float> heuristic;
            public HeuristicComparer(Func<ReadOnlyItemStack, float> heuristic) => this.heuristic = heuristic;
            public int Compare(ReadOnlyItemStack x, ReadOnlyItemStack y) => Comparer<float>.Default.Compare(heuristic(x), heuristic(y));
        }
        protected class ItemStackToSlotComparer : IComparer<ReadOnlyInventorySlot> //Proxies an IComparer<ReadOnlyItemStack> so slots can be sortable
        {
            IComparer<ReadOnlyItemStack> wrapped;
            public ItemStackToSlotComparer(IComparer<ReadOnlyItemStack> wrapped) => this.wrapped = wrapped;
            public int Compare(ReadOnlyInventorySlot x, ReadOnlyInventorySlot y) => wrapped.Compare(x.Contents, y.Contents);
        }

        /// <summary>
        /// Ensure state is valid (such as slot IDs). Also handles API upgrades.
        /// </summary>
        public abstract void Validate();

        /// <summary>
        /// Should be called exactly once when the inventory is created. Handles stuff like installing hooks.
        /// </summary>
        public abstract void DoSetup();

        #region Hook interface

        /*
         * Hooks execute in ascending priority. For events that only listen without modifying behavior (such as UI), register for priority = int.MaxValue.
         * See StandardInventory for a reusable implementation using InventoryHooksImplDetail
         */

        public abstract IInventoryHooks Hooks { get; }

        #endregion
    }

}