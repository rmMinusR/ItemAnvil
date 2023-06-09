using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// A safe way to transfer items between two inventories, such purchasing from a shop or trading between players
/// </summary>
[Serializable]
public sealed class Transaction : ICloneable
{
    [SerializeField] private ItemStack[] itemsAToB; //Items that should be taken from A and given to B
    [SerializeField] private ItemStack[] itemsBToA; //Items that should be taken from B and given to A

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public Transaction(IEnumerable<ItemStack> itemsAtoB, IEnumerable<ItemStack> itemsBtoA)
    {
        this.itemsAToB = itemsAtoB.Select(s => s.Clone()).ToArray();
        this.itemsBToA = itemsBtoA.Select(s => s.Clone()).ToArray();
    }

    /// <summary>
    /// Attempt to perform the transaction. Does not modify the original object.
    /// </summary>
    /// <returns>Whether the transaction was successfully performed</returns>
    public bool TryExchange(Inventory inventoryA, Inventory inventoryB)
    {
        if (IsValid(inventoryA, inventoryB))
        {
            DoExchange(inventoryA, inventoryB);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Can the transaction take place between the given inventories?
    /// </summary>
    /// <returns>Whether the transaction would be successfully performed</returns>
    public bool IsValid(Inventory inventoryA, Inventory inventoryB)
    {
        return itemsAToB.All(i => inventoryA.Count(i.itemType) >= i.quantity)
            && itemsBToA.All(i => inventoryB.Count(i.itemType) >= i.quantity);
    }

    /// <summary>
    /// Businss logic function that actually transfers the items
    /// </summary>
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

    /// <summary>
    /// Helper function, preferable to calling TryExchange multiple times
    /// </summary>
    public void MultiplyInPlace(int scale)
    {
        foreach (ItemStack s in itemsAToB) s.quantity *= scale;
        foreach (ItemStack s in itemsBToA) s.quantity *= scale;
    }
}