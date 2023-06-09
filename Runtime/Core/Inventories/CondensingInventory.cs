using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// An inventory that automatically expands and shrinks to fit its contents. All slots are guaranteed valid.
/// </summary>
[Serializable]
public sealed class CondensingInventory : Inventory
{
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    /// <summary>
    /// Add an item using an ItemStack
    /// </summary>
    /// <param name="newStack">Stack to add</param>
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

    /// <summary>
    /// Attempt to remove items. If not enough are available, no changes will be made.
    /// </summary>
    /// <param name="typeToRemove">Item type to be removed</param>
    /// <param name="totalToRemove">How many to be removed</param>
    /// <returns>If enough items were present, an IEnumerable of those items. Otherwise it will be empty, and no changes were made.</returns>
    public override IEnumerable<ItemStack> TryRemove(Item typeToRemove, int totalToRemove)
    {
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

    /// <summary>
    /// Remove all items of the given type
    /// </summary>
    /// <returns>How many items were removed</returns>
    public override int RemoveAll(Item typeToRemove)
    {
        bool matches(ItemStack stack) => stack.itemType == typeToRemove;
        int nRemoved = contents.Where(matches).Sum(i => i.quantity);
        contents.RemoveAll(matches);
        return nRemoved;
    }

    /// <summary>
    /// Check how many items are present of the given type
    /// </summary>
    public override int Count(Item itemType)
    {
        return contents.Where(stack => stack.itemType == itemType).Sum(stack => stack.quantity);
    }

    /// <summary>
    /// Dump the contents of this inventory. Note that these the original instances.
    /// </summary>
    public override IEnumerable<ReadOnlyItemStack> GetContents() => contents;

    /// <summary>
    /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
    /// </summary>
    public override List<ItemStack> CloneContents()
    {
        List<ItemStack> list = new List<ItemStack>();
        foreach (ItemStack s in contents) list.Add(s.Clone());
        return list;
    }

    /// <summary>
    /// Find the first item of the given type
    /// </summary>
    /// <returns>The matching ItemStack if a match was present, else null</returns>
    public override ItemStack Find(Item type)
    {
        return contents.FirstOrDefault(i => i.itemType == type);
    }
}
