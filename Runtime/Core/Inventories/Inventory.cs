using System;
using System.Collections.Generic;

[Serializable]
public abstract class Inventory
{
    public virtual void AddItem(Item itemType, int quantity) => AddItem(new ItemStack(itemType, quantity));
    public abstract void AddItem(ItemStack newStack);

    public abstract bool TryRemove(Item typeToRemove, int totalToRemove);
    public abstract int RemoveAll(Item typeToRemove);
    
    public abstract int Count(Item itemType);

    public abstract IEnumerable<ReadOnlyItemStack> GetContents();

    public abstract List<ItemStack> CloneContents();
}
