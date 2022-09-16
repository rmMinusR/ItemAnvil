using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class FixedSlotInventory : MonoBehaviour, Inventory
{
    [SerializeField] [Min(1)] private int slotCount = 1;
    [SerializeField] private List<ItemStack> contents;

    public void AddItem(Item itemType, int quantity)
    {
        throw new System.NotImplementedException();
    }

    public void AddItem(ItemStack newStack)
    {
        throw new System.NotImplementedException();
    }

    public List<ItemStack> CloneContents()
    {
        throw new System.NotImplementedException();
    }

    public int Count(Item itemType)
    {
        throw new System.NotImplementedException();
    }

    public IEnumerator<ReadOnlyItemStack> GetContents()
    {
        throw new System.NotImplementedException();
    }

    public int RemoveAll(Item typeToRemove)
    {
        throw new System.NotImplementedException();
    }

    public bool TryRemove(Item typeToRemove, int totalToRemove)
    {
        throw new System.NotImplementedException();
    }
}
