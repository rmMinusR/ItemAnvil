using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class Transaction : ICloneable
{
    [SerializeField] private ItemStack[] itemsAToB;
    [SerializeField] private ItemStack[] itemsBToA;

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public Transaction(IEnumerable<ItemStack> itemsAtoB, IEnumerable<ItemStack> itemsBtoA)
    {
        this.itemsAToB = itemsAtoB.Select(s => s.Clone()).ToArray();
        this.itemsBToA = itemsBtoA.Select(s => s.Clone()).ToArray();
    }

    public bool TryExchange(Inventory inventoryA, Inventory inventoryB)
    {
        if (IsValid(inventoryA, inventoryB))
        {
            return DoExchange(inventoryA, inventoryB);
        }

        return false;
    }

    public bool IsValid(Inventory inventoryA, Inventory inventoryB)
    {
        return itemsAToB.All(i => inventoryA.Count(i.itemType) >= i.quantity)
            && itemsBToA.All(i => inventoryB.Count(i.itemType) >= i.quantity);
    }

    private bool DoExchange(Inventory inventoryA, Inventory inventoryB)
    {
        bool stillValid = true;
#if UNITY_EDITOR
        Debug.Assert(stillValid = IsValid(inventoryA, inventoryB));
#endif
        
        //FIXME: If itemsAtoB has duplicate type, or itemsBtoA has duplicate type, breaks rollback-on-fail contract (exception safety level 2) because items will still be removed

        //Remove items
        foreach (ItemStack i in itemsAToB) stillValid &= inventoryA.TryRemove(i.itemType, i.quantity);
        foreach (ItemStack i in itemsBToA) stillValid &= inventoryB.TryRemove(i.itemType, i.quantity);

        Debug.Assert(stillValid);

        //Add items provided transaction was valid
        if (stillValid)
        {
            foreach (ItemStack i in itemsAToB) inventoryB.AddItem(i);
            foreach (ItemStack i in itemsBToA) inventoryA.AddItem(i);
        }

        return stillValid;
    }

    public object Clone()
    {
        return new Transaction(itemsAToB, itemsBToA);
    }

    public void MultiplyInPlace(int scale)
    {
        foreach (ItemStack s in itemsAToB) s.quantity *= scale;
        foreach (ItemStack s in itemsBToA) s.quantity *= scale;
    }

    public void Log()
    {
        foreach (ItemStack s in itemsAToB) Debug.Log("A=>B "+s.itemType+" x"+s.quantity);
        foreach (ItemStack s in itemsBToA) Debug.Log("B=>A "+s.itemType+" x"+s.quantity);
    }
}