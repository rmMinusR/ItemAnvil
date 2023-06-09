using System;
using System.Collections.Generic;
using System.Linq;

[Serializable]
public abstract class Inventory
{
    /// <summary>
    /// Add an item
    /// </summary>
    /// <param name="itemType">Type of the item to add</param>
    /// <param name="quantity">How many to add</param>
    public virtual void AddItem(Item itemType, int quantity) => AddItem(new ItemStack(itemType, quantity));

    /// <summary>
    /// Add an item using an ItemStack
    /// </summary>
    /// <param name="newStack">Stack to add</param>
    public abstract void AddItem(ItemStack newStack);

    /// <summary>
    /// Attempt to remove items. If not enough are available, no changes will be made.
    /// </summary>
    /// <param name="filter">Filter specifying what to remove</param>
    /// <param name="totalToRemove">How many to be removed</param>
    /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
    public abstract IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove);

    /// <summary>
    /// Attempt to remove items. If not enough are available, no changes will be made.
    /// </summary>
    /// <param name="typeToRemove">Item type to be removed</param>
    /// <param name="totalToRemove">How many to be removed</param>
    /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
    public abstract IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove);
    
    /// <summary>
    /// Remove all items that match the given filter
    /// </summary>
    /// <returns>How many items were removed</returns>
    public abstract int RemoveAll(ItemFilter filter);

    /// <summary>
    /// Remove all items of the given type
    /// </summary>
    /// <returns>How many items were removed</returns>
    public abstract int RemoveAll(Item typeToRemove);

    /// <summary>
    /// Check how many items match the given filter
    /// </summary>
    public abstract int Count(ItemFilter filter);

    /// <summary>
    /// Check how many items are present of the given type
    /// </summary>
    public abstract int Count(Item itemType);

    /// <summary>
    /// Find the first ItemStack of the given type.
    /// </summary>
    /// <remarks>
    /// Note that these are the original instances, and changes made will reflect in the inventory.
    /// </remarks>
    /// <returns>The matching ItemStack if a match was present, else null</returns>
    public virtual ItemStack FindFirst(ItemFilter filter) => FindAll(filter).FirstOrDefault();

    /// <summary>
    /// Find the first item of the given type.
    /// </summary>
    /// <remarks>
    /// Note that these are the original instances, and changes made will reflect in the inventory.
    /// </remarks>
    public virtual ItemStack FindFirst(Item type) => FindAll(type).FirstOrDefault();

    /// <summary>
    /// Find all ItemStacks that match the filter.
    /// </summary>
    /// <remarks>
    /// Note that these are the original instances, and changes made will reflect in the inventory.
    /// </remarks>
    public abstract IEnumerable<ItemStack> FindAll(ItemFilter filter);

    /// <summary>
    /// Find all ItemStacks with the given type.
    /// </summary>
    /// <remarks>
    /// Note that these are the original instances, and changes made will reflect in the inventory.
    /// </remarks>
    public abstract IEnumerable<ItemStack> FindAll(Item type);

    /// <summary>
    /// Dump the contents of this inventory.
    /// </summary>
    /// <remarks>
    /// Note that these are the original instances, and changes made will reflect in the inventory.
    /// </remarks>
    public abstract IEnumerable<ReadOnlyItemStack> GetContents();

    /// <summary>
    /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
    /// </summary>
    public abstract List<ItemStack> CloneContents();
    
    public virtual void Tick()
    {
        foreach (ReadOnlyItemStack i in GetContents())
        {
            if (i != null) foreach (ItemInstanceProperty p in i.instanceProperties)
            {
                if (p.ShouldTick) p.Tick(); //FIXME this breaks read-only contract
            }
        }
    }

    //NOTE: Comparers and heuristics must be null-safe!
    public abstract void Sort(IComparer<ReadOnlyItemStack> comparer);
    public virtual void Sort(Func<ReadOnlyItemStack, float> heuristic) => Sort(new HeuristicComparer(heuristic));
    private class HeuristicComparer : IComparer<ReadOnlyItemStack>
    {
        Func<ReadOnlyItemStack, float> heuristic;
        public HeuristicComparer(Func<ReadOnlyItemStack, float> heuristic) => this.heuristic = heuristic;
        public int Compare(ReadOnlyItemStack x, ReadOnlyItemStack y) => Comparer<float>.Default.Compare(heuristic(x), heuristic(y));
    }
}
