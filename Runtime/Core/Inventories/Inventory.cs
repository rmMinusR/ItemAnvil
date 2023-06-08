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
    public abstract ItemStack Find(ItemFilter filter);
    public abstract ItemStack Find(Item type);

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
}
