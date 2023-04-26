using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FixedSlotInventory : Inventory
{
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    public override void AddItem(ItemStack newStack)
    {
        //Try to merge with existing stacks
        foreach (ItemStack existing in contents)
        {
            if (existing != null) ItemStack.TryMerge(newStack, existing);
            if (newStack.quantity == 0) return;
        }

        //Awful fix, but it plays nice with stacking rules
        while (newStack.quantity > 0)
        {
            ItemStack s = new ItemStack(newStack.itemType, 0);
            contents[contents.FindIndex(i => i == null || i.itemType == null || i.quantity == 0)] = s;
            ItemStack.TryMerge(newStack, s);
        }

        //Prevent covariants
        newStack.quantity = 0;
    }

    public override List<ItemStack> CloneContents()
    {
        return contents.Select(i => i.Clone()).ToList();
    }

    public override int Count(Item itemType)
    {
        return contents.Where(i => i != null && i.itemType == itemType).Sum(i => i.quantity);
    }

    public override IEnumerable<ReadOnlyItemStack> GetContents()
    {
        return contents;
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

    public override bool TryRemove(Item typeToRemove, int totalToRemove)
    {
        if (Count(typeToRemove) < totalToRemove) return false;

        for (int i = 0; i < contents.Count; ++i)
        {
            if (contents[i] != null && contents[i].itemType == typeToRemove)
            {
                if (totalToRemove >= contents[i].quantity)
                {
                    //This stack is not enough to complete requirements. Continue consuming.
                    totalToRemove -= contents[i].quantity;
                    contents[i] = null;
                }
                else
                {
                    //This stack is enough to complete requirements. Stop consuming.
                    contents[i].quantity -= totalToRemove;
                    return true;
                }
            }
        }

        throw new InvalidOperationException("Counted sufficient items, but somehow didn't have enough. This should never happen!");
    }
}
