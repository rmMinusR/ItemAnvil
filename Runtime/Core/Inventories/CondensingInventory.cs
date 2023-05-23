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

    public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove)
    {
        Debug.Log(this + " removing " + typeToRemove + " x" + totalToRemove);

        List<ItemStack> matches = contents.Where(stack => stack.itemType == typeToRemove).ToList();

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
                if(totalToRemove < matches[0].quantity)
                {
                    //If we would be able to take enough from the current stack, finish routine
                    ItemStack tmp = matches[0].Clone();
                    tmp.quantity = totalToRemove;
                    @out.Add(tmp);
                    matches[0].quantity -= totalToRemove;
                    totalToRemove = 0;
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
            return @out;
        }
    }

    public override int RemoveAll(Item typeToRemove)
    {
        bool matches(ItemStack stack) => stack.itemType == typeToRemove;
        int nRemoved = contents.Where(matches).Sum(i => i.quantity);
        contents.RemoveAll(matches);
        return nRemoved;
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

    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i.itemType == type);
    }
}
