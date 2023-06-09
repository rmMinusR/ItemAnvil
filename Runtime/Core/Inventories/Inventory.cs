using System;
using System.Collections.Generic;

[Serializable]
public abstract class Inventory
{
    public virtual void AddItem(Item itemType, int quantity) => AddItem(new ItemStack(itemType, quantity));
    public abstract void AddItem(ItemStack newStack);

    public abstract IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove);
    public abstract IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove);
    public abstract int RemoveAll(ItemFilter filter);
    public abstract int RemoveAll(Item typeToRemove);

    public abstract int Count(ItemFilter filter);
    public abstract int Count(Item itemType);

    public abstract IEnumerable<ReadOnlyItemStack> GetContents();
    public abstract ItemStack FindFirst(ItemFilter filter);
    public abstract ItemStack FindFirst(Item type);
    public abstract IEnumerable<ItemStack> FindAll(ItemFilter filter);
    public abstract IEnumerable<ItemStack> FindAll(Item type);

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
