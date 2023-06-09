using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An inventory with a fixed number of slots. Attempting to add more items when full will fail. Slots may be null if empty.
/// </summary>
[Serializable]
public sealed class FixedSlotInventory : Inventory
{
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    public FixedSlotInventory() { }
    public FixedSlotInventory(int size)
    {
        for (int i = 0; i < size; ++i) contents.Add(null);
    }

    /// <summary>
    /// Add an item using an ItemStack
    /// </summary>
    /// <param name="newStack">Stack to add</param>
    public override void AddItem(ItemStack newStack)
    {
        if (newStack == null || newStack.itemType == null) throw new ArgumentException("Cannot add nothing!");

        //Prevent covariants
        newStack = newStack.Clone();

        //Try to merge with existing stacks
        foreach (ItemStack existing in contents)
        {
            if (existing != null) ItemStack.TryMerge(newStack, existing);
            if (newStack.quantity == 0) return;
        }

        //Awful fix, but it plays nice with stacking rules
        while (newStack.quantity > 0)
        {
            ItemStack s = newStack.Clone();
            s.quantity = 0;
            contents[contents.FindIndex(i => i == null || i.itemType == null || i.quantity == 0)] = s;
            ItemStack.TryMerge(newStack, s);
        }
    }

    /// <summary>
    /// Check how many items are present of the given type
    /// </summary>
    public override int Count(Item itemType)
    {
        return contents.Where(i => i != null && i.itemType == itemType).Sum(i => i.quantity);
    }

    /// <summary>
    /// Dump the contents of this inventory. Note that these the original instances.
    /// </summary>
    public override IEnumerable<ReadOnlyItemStack> GetContents()
    {
        return contents.Where(i => i != null && i.itemType != null);
    }

    /// <summary>
    /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
    /// </summary>
    public override List<ItemStack> CloneContents()
    {
        return contents.Select(i => i?.Clone()).ToList();
    }

    /// <summary>
    /// Remove all items of the given type
    /// </summary>
    /// <returns>How many items were removed</returns>
    public override int RemoveAll(Item typeToRemove)
    {
        int nRemoved = 0;
        for (int i = 0; i < contents.Count; ++i)
        {
            if (contents[i] != null && contents[i].itemType == typeToRemove)
            {
                nRemoved += contents[i].quantity;
                contents[i] = null;
            }
        }
        return nRemoved;
    }

    /// <summary>
    /// Attempt to remove items. If not enough are available, no changes will be made.
    /// </summary>
    /// <param name="typeToRemove">Item type to be removed</param>
    /// <param name="totalToRemove">How many to be removed</param>
    /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
    public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove)
    {
        List<ItemStack> @out = new List<ItemStack>();

        //Make sure we have enough
        if (Count(typeToRemove) < totalToRemove) return @out;

        for (int i = 0; i < contents.Count; ++i)
        {
            if (contents[i] != null && contents[i].itemType == typeToRemove)
            {
                if (contents[i].quantity >= totalToRemove)
                {
                    //This stack is enough to complete requirements. Stop consuming.
                    ItemStack tmp = contents[i].Clone();
                    tmp.quantity = totalToRemove;
                    @out.Add(tmp);
                    contents[i].quantity -= totalToRemove;
                    return @out;
                }
                else
                {
                    //This stack is not enough to complete requirements. Continue consuming.
                    totalToRemove -= contents[i].quantity;
                    @out.Add(contents[i]);
                    contents[i] = null;
                }
            }
        }

        throw new InvalidOperationException("Counted sufficient items, but somehow didn't have enough. This should never happen!");
    }
    
    /// <summary>
    /// Find the first item of the given type
    /// </summary>
    /// <returns>The matching ItemStack if a match was present, else null</returns>
    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i != null && i.itemType == type);
    }
}
