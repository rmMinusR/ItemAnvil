using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class FixedSlotInventory : Inventory
{
    [SerializeField] [Min(1)] private int slotCount = 1;
    [SerializeField] private List<ItemStack> contents;

    public override void AddItem(Item itemType, int quantity)
    {
        throw new System.NotImplementedException();
    }

    public override void AddItem(ItemStack newStack)
    {
        throw new System.NotImplementedException();
    }

    public override List<ItemStack> CloneContents()
    {
        throw new System.NotImplementedException();
    }

    public override int Count(Item itemType)
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerable<ReadOnlyItemStack> GetContents()
    {
        throw new System.NotImplementedException();
    }

    public override int RemoveAll(Item typeToRemove)
    {
        throw new System.NotImplementedException();
    }

    public override bool TryRemove(Item typeToRemove, int totalToRemove)
    {
        throw new System.NotImplementedException();
    }
}
