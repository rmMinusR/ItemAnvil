using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class Transaction : ICloneable
{
#if USING_INSPECTORSUGAR
    [InspectorReadOnly]
#endif
    [SerializeField] private Inventory inventoryA;
    [SerializeField] private ItemStack[] itemsAToB;

#if USING_INSPECTORSUGAR
    [InspectorReadOnly]
#endif
    [SerializeField] private Inventory inventoryB;
    [SerializeField] private ItemStack[] itemsBToA;

    [SerializeField] [InspectorReadOnly] private bool hasValidated;

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public Transaction(Inventory inventoryA, IEnumerable<ItemStack> itemsAtoB, Inventory inventoryB, IEnumerable<ItemStack> itemsBtoA)
    {
        this.inventoryA = inventoryA;
        this.itemsAToB = itemsAtoB.Select(s => s.Clone()).ToArray();
        this.inventoryB = inventoryB;
        this.itemsBToA = itemsBtoA.Select(s => s.Clone()).ToArray();
        hasValidated = false;
    }

    public bool IsValid()
    {
        return hasValidated = (   itemsAToB.All(i => inventoryA.Count(i.itemType) >= i.quantity)
                               && itemsBToA.All(i => inventoryB.Count(i.itemType) >= i.quantity));
    }

    public void DoExchange()
    {
#if UNITY_EDITOR
        Debug.Assert(hasValidated || IsValid());
#endif

        //FIXME: If itemsAtoB has duplicate type, or itemsBtoA has duplicate type, breaks rollback-on-fail contract (exception safety level 2) because items will still be removed

        //Remove items
        bool stillValid = true;
        foreach (ItemStack i in itemsAToB) stillValid &= inventoryA.TryRemove(i.itemType, i.quantity);
        foreach (ItemStack i in itemsBToA) stillValid &= inventoryB.TryRemove(i.itemType, i.quantity);

        Debug.Assert(stillValid);

        //Add items provided transaction was valid
        if (stillValid)
        {
            foreach (ItemStack i in itemsAToB) inventoryB.AddItem(i);
            foreach (ItemStack i in itemsBToA) inventoryA.AddItem(i);
        }

        //Clear data to make sure this can't be executed twice
        itemsAToB = new ItemStack[] { };
        itemsBToA = new ItemStack[] { };
    }

    public object Clone()
    {
        return new Transaction(inventoryA, itemsAToB, inventoryB, itemsBToA);
    }

    public void MultiplyInPlace(int scale)
    {
        foreach (ItemStack s in itemsAToB) s.quantity *= scale;
        foreach (ItemStack s in itemsBToA) s.quantity *= scale;
    }
}