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

        //Prevent covariants
        newStack.quantity = 0;
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
                    @out.Add(matches[0]);
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
        return contents.RemoveAll(stack => stack.itemType == typeToRemove);
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

    public override void Remove(ItemStack stackToRemove)
    {
        if (!contents.Remove(stackToRemove)) throw new InvalidOperationException(this+" does not contain "+stackToRemove?.ToString());
    }

    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i.itemType == type);
    }
}
