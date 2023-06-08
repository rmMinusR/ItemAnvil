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
            DoExchange(inventoryA, inventoryB);
            return true;
        }

        return false;
    }

    public bool IsValid(Inventory inventoryA, Inventory inventoryB)
    {
        return itemsAToB.All(i => inventoryA.Count(i.itemType) >= i.quantity)
            && itemsBToA.All(i => inventoryB.Count(i.itemType) >= i.quantity);
    }

    private void DoExchange(Inventory inventoryA, Inventory inventoryB)
    {
#if UNITY_EDITOR
        Debug.Assert(IsValid(inventoryA, inventoryB));
#endif
        List<ItemStack> itemsAToB_instanced = new List<ItemStack>();
        List<ItemStack> itemsBToA_instanced = new List<ItemStack>();

        try
        {
            //Capture items to be moved, and remove from original inventories
            for (int i = 0; i < itemsAToB.Length; ++i) itemsAToB_instanced.AddRange(inventoryA.TryRemove(itemsAToB[i].itemType, itemsAToB[i].quantity));
            for (int i = 0; i < itemsBToA.Length; ++i) itemsBToA_instanced.AddRange(inventoryB.TryRemove(itemsBToA[i].itemType, itemsBToA[i].quantity));
        }
        catch(Exception e)
        {
            //Something went wrong! Refund items.
            foreach (ItemStack i in itemsAToB_instanced) inventoryA.AddItem(i);
            foreach (ItemStack i in itemsBToA_instanced) inventoryB.AddItem(i);

            throw new Exception("Something went wrong while processing Transaction!", e);
        }

        //Add items to destination inventories
        foreach (ItemStack i in itemsAToB_instanced) inventoryB.AddItem(i);
        foreach (ItemStack i in itemsBToA_instanced) inventoryA.AddItem(i);
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