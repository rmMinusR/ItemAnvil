using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
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


        #region Hook interface

        /*
         * Hooks execute in ascending priority. For events that only listen without modifying behavior (such as UI), register for priority = int.MaxValue.
         */

        public abstract void Hook(AddItemHook       listener, int priority);
        public abstract void Hook(CanSlotAcceptHook listener, int priority);
        public abstract void Hook(PostAddItemHook   listener, int priority);
        public abstract void Hook(RemoveItemHook    listener, int priority);
        public abstract void Hook(TrySortSlotHook   listener, int priority);
        public abstract void Hook(PostSortHook      listener, int priority);
        public abstract void Unhook(AddItemHook       listener);
        public abstract void Unhook(CanSlotAcceptHook listener);
        public abstract void Unhook(PostAddItemHook   listener);
        public abstract void Unhook(RemoveItemHook    listener);
        public abstract void Unhook(TrySortSlotHook   listener);
        public abstract void Unhook(PostSortHook      listener);

        /*
        
        STANDARD IMPLEMENTATION:

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

         */

        #endregion
    }

}