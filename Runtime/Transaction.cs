using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Transaction : ICloneable
{
    [SerializeField] private Inventory inventoryA;
    [SerializeField] private ItemStack[] itemsAtoB;

    [SerializeField] private Inventory inventoryB;
    [SerializeField] private ItemStack[] itemsBtoA;

    [SerializeField] [InspectorReadOnly] private bool hasValidated;

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public Transaction(Inventory inventoryA, IEnumerable<ItemStack> itemsAtoB, Inventory inventoryB, IEnumerable<ItemStack> itemsBtoA)
    {
        this.inventoryA = inventoryA;
        this.itemsAtoB = itemsAtoB.Select(s => s.Clone()).ToArray();
        this.inventoryB = inventoryB;
        this.itemsBtoA = itemsBtoA.Select(s => s.Clone()).ToArray();
        hasValidated = false;
    }

    public bool IsValid()
    {
        return hasValidated = (   itemsAtoB.All(i => inventoryA.Count(i.itemType) >= i.quantity)
                               && itemsBtoA.All(i => inventoryB.Count(i.itemType) >= i.quantity));
    }

    public void DoExchange()
    {
#if UNITY_EDITOR
        Debug.Assert(hasValidated || IsValid());
#endif

        //FIXME: If itemsAtoB has duplicate type, or itemsBtoA has duplicate type, breaks rollback-on-fail contract (exception safety level 2) because items will still be removed

        //Remove items
        bool stillValid = true;
        foreach (ItemStack i in itemsAtoB) stillValid &= inventoryA.TryRemove(i.itemType, i.quantity);
        foreach (ItemStack i in itemsBtoA) stillValid &= inventoryB.TryRemove(i.itemType, i.quantity);

        Debug.Assert(stillValid);

        //Add items provided transaction was valid
        if (stillValid)
        {
            foreach (ItemStack i in itemsAtoB) inventoryB.AddItem(i);
            foreach (ItemStack i in itemsBtoA) inventoryA.AddItem(i);
        }

        //Clear data to make sure this can't be executed twice
        itemsAtoB = new ItemStack[] { };
        itemsBtoA = new ItemStack[] { };
    }

    public object Clone()
    {
        return new Transaction(inventoryA, itemsAtoB, inventoryB, itemsBtoA);
    }

    //Like Clone(), except it also multiplies the trade by a given amount
    public Transaction CloneAndMultiply(int scale)
    {
        return new Transaction(inventoryA, itemsAtoB.Select(i => new ItemStack(i.itemType, i.quantity*scale)),
                               inventoryB, itemsBtoA.Select(i => new ItemStack(i.itemType, i.quantity*scale)));
    }
}