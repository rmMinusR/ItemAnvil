using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FixedSlotInventory : Inventory
{
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    public FixedSlotInventory() { }
    public FixedSlotInventory(int size)
    {
        for (int i = 0; i < size; ++i) contents.Add(null);
    }

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

    public override List<ItemStack> CloneContents()
    {
        return contents.Select(i => i?.Clone()).ToList();
    }

    public override int Count(Item itemType)
    {
        return contents.Where(i => i != null && i.itemType == itemType).Sum(i => i.quantity);
    }

    public override IEnumerable<ReadOnlyItemStack> GetContents()
    {
        return contents.Where(i => i != null && i.itemType != null);
    }

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
    
    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i != null && i.itemType == type);
    }
}
