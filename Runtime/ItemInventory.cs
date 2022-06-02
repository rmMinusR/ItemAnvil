using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ItemInventory : MonoBehaviour
{
    [SerializeField] private List<ItemStack> contents;

    public void AddItem(ItemStack newStack)
    {
        ItemStack existingStack = contents.Find(stack => stack.CanMerge(newStack));
        if(existingStack != null) ItemStack.Merge(newStack, existingStack);
    }

    public bool TryRemove(Item typeToRemove, int totalToRemove)
    {
        List<ItemStack> matches = contents.Where(stack => stack.itemType == typeToRemove).ToList();
        
        //NOTE: Not threadsafe

        //Make sure we have enough
        int itemsAvailable = matches.Sum(stack => stack.quantity);
        if (itemsAvailable < totalToRemove) return false;
        else
        {
            //Removal routine
            while (totalToRemove > 0)
            {
                if(totalToRemove < matches[0].quantity)
                {
                    //If we would be able to take enough from the current stack, finish routine
                    matches[0].quantity -= totalToRemove;
                    totalToRemove = 0;
                }
                else
                {
                    //If we wouldn't be able to take enough from the current stack, take what we can and continue
                    //FIXME breaks for keep-if-zero
                    totalToRemove -= matches[0].quantity;
                    matches[0].quantity = 0; //Just to be safe...
                    contents.Remove(matches[0]);
                    matches.RemoveAt(0);
                }
            }
            return true;
        }
    }

    public int RemoveAll(Item typeToRemove)
    {
        return contents.RemoveAll(stack => stack.itemType == typeToRemove);
    }

    public int Count(Item itemType)
    {
        return contents.Where(stack => stack.itemType == itemType).Sum(stack => stack.quantity);
    }


    //Technically breaks the encapsulation contract, but clearly defines which is mutable and which isn't.

    internal IEnumerator<ItemStack> GetContents() { foreach (ItemStack s in contents) yield return s.Clone(); }

    internal IReadOnlyList<ItemStack> GetMutableContents() => contents;
}
