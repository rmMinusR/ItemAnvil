using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class CondensingInventory : Inventory
{
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    public override void AddItem(ItemStack newStack)
    {
        if (newStack == null || newStack.itemType == null) throw new ArgumentException("Cannot add nothing!");

        //Prevent covariants
        newStack = newStack.Clone();

        //Try to merge with an existing stack
        foreach (ItemStack existing in contents)
        {
            ItemStack.TryMerge(newStack, existing);
            if (newStack.quantity == 0) return;
        }

        //Awful fix, but it plays nice with stacking rules
        while (newStack.quantity > 0)
        {
            ItemStack s = newStack.Clone();
            s.quantity = 0;
            contents.Add(s);
            ItemStack.TryMerge(newStack, s);
        }
    }

    #region TryRemove family

    public override IEnumerable<ItemStack> TryRemove(ItemFilter filter, int totalToRemove) => TryRemove_Impl(filter.Matches, totalToRemove);
    public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove) => TryRemove_Impl(stack => stack.itemType == typeToRemove, totalToRemove);
    private IEnumerable<ItemStack> TryRemove_Impl(Func<ItemStack, bool> filter, int totalToRemove)
    {
        List<ItemStack> matches = contents.Where(filter).ToList();

        //NOTE: Not threadsafe

        List<ItemStack> @out = new List<ItemStack>();

        //Make sure we have enough
        int itemsAvailable = matches.Sum(stack => stack.quantity);
        if (itemsAvailable < totalToRemove) return @out;
        else
        {
            //Removal routine
            while (totalToRemove > 0)
            {
                if(matches[0].quantity >= totalToRemove)
                {
                    //If we would be able to take enough from the current stack, finish routine
                    ItemStack tmp = matches[0].Clone();
                    tmp.quantity = totalToRemove;
                    @out.Add(tmp);
                    matches[0].quantity -= totalToRemove;
                    totalToRemove = 0;
                    return @out;
                }
                else
                {
                    //If we wouldn't be able to take enough from the current stack, take what we can and continue
                    //FIXME breaks for keep-if-zero
                    @out.Add(matches[0].Clone());
                    totalToRemove -= matches[0].quantity;
                    matches[0].quantity = 0; //Just to be safe...
                    contents.Remove(matches[0]);
                    matches.RemoveAt(0);
                }
            }

            throw new InvalidOperationException("Counted sufficient items, but somehow didn't have enough. This should never happen!");
        }
    }

    #endregion

    #region RemoveAll family

    public override int RemoveAll(ItemFilter filter) => RemoveAll_Impl(filter.Matches);
    public override int RemoveAll(Item typeToRemove) => RemoveAll_Impl(s => s.itemType == typeToRemove);
    private int RemoveAll_Impl(Func<ItemStack, bool> filter)
    {
        int nRemoved = contents.Where(filter).Sum(i => i.quantity);
        contents.RemoveAll(i => filter(i));
        return nRemoved;
    }

    #endregion

    public override int Count(ItemFilter filter)
    {
        return contents.Where(filter.Matches).Sum(stack => stack.quantity);
    }

    public override int Count(Item itemType)
    {
        return contents.Where(stack => stack.itemType == itemType).Sum(stack => stack.quantity);
    }

    public override IEnumerable<ReadOnlyItemStack> GetContents() => contents;

    public override List<ItemStack> CloneContents()
    {
        List<ItemStack> list = new List<ItemStack>();
        foreach (ItemStack s in contents) list.Add(s.Clone());
        return list;
    }

    public override ItemStack Find(ItemFilter filter)
    {
        return contents.FirstOrDefault(filter.Matches);
    }

    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i.itemType == type);
    }
}
