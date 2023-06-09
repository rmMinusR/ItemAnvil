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

    public override IEnumerable<ReadOnlyItemStack> GetContents()
    {
        return contents.Where(i => i != null && i.itemType != null);
    }

    #region TryRemove family

    public override IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove)
    {
        //Make sure we have enough
        if (Count(filter) < totalToRemove) return new List<ItemStack>();

        return TryRemove_Impl(filter.Matches, totalToRemove);
    }

    public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove)
    {
        //Make sure we have enough
        if (Count(typeToRemove) < totalToRemove) return new List<ItemStack>();

        return TryRemove_Impl(s => s.itemType == typeToRemove, totalToRemove);
    }

    private IEnumerable<ItemStack> TryRemove_Impl(Func<ItemStack, bool> filter, int totalToRemove)
    {
        List<ItemStack> @out = new List<ItemStack>();

        for (int i = 0; i < contents.Count; ++i)
        {
            if (contents[i] != null && filter(contents[i]))
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

    #endregion

    #region RemoveAll family

    public override int RemoveAll(ItemFilter filter) => RemoveAll_Impl(filter.Matches);
    public override int RemoveAll(Item typeToRemove) => RemoveAll_Impl(s => s.itemType == typeToRemove);
    private int RemoveAll_Impl(Func<ItemStack, bool> filter)
    {
        int nRemoved = 0;
        for (int i = 0; i < contents.Count; ++i)
        {
            if (contents[i] != null && filter(contents[i]))
            {
                nRemoved += contents[i].quantity;
                contents[i] = null;
            }
        }
        return nRemoved;
    }

    #endregion

    public override int Count(ItemFilter filter) => contents.Where(i => i != null).Where(filter.Matches).Sum(i => i.quantity);
    public override int Count(Item itemType) => contents.Where(i => i != null).Where(i => i.itemType == itemType).Sum(i => i.quantity);

    public override ItemStack FindFirst(ItemFilter filter) => FindAll(filter).FirstOrDefault();
    public override ItemStack FindFirst(Item type) => FindAll(type).FirstOrDefault();

    public override IEnumerable<ItemStack> FindAll(ItemFilter filter) => contents.Where(i => i != null).Where(filter.Matches);
    public override IEnumerable<ItemStack> FindAll(Item type) => contents.Where(i => i != null).Where(i => i.itemType == type);

    public override void Sort(IComparer<ReadOnlyItemStack> comparer)
    {
        contents.Sort(comparer);
    }
}
