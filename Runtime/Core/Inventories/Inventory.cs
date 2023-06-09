using System;
using System.Collections.Generic;

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
    /// <param name="typeToRemove">Item type to be removed</param>
    /// <param name="totalToRemove">How many to be removed</param>
    /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
    public abstract IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove);

    /// <summary>
    /// Remove all items of the given type
    /// </summary>
    /// <returns>How many items were removed</returns>
    public abstract int RemoveAll(Item typeToRemove);
    
    /// <summary>
    /// Check how many items are present of the given type
    /// </summary>
    public abstract int Count(Item itemType);

    /// <summary>
    /// Find the first item of the given type
    /// </summary>
    /// <returns>The matching ItemStack if a match was present, else null</returns>
    public abstract ItemStack Find(Item type);

    /// <summary>
    /// Dump the contents of this inventory. Note that these the original instances.
    /// </summary>
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
}
