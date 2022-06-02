using System;
using UnityEngine;

[Serializable]
public sealed class ItemStack : ReadOnlyItemStack, ICloneable
{
    public Item itemType;
    [Min(0)] public int quantity = 1;
    //If we had metadata, it would go here

    public ItemStack() : this(null, 1) { }

    public ItemStack(Item itemType) : this(itemType, 1) { }

    public ItemStack(Item itemType, int quantity)
    {
        this.itemType = itemType;
        this.quantity = quantity;
    }

    public bool CanMerge(ItemStack other)
    {
        //Note: Must be reflective, symmetric, and transitive

        return this.itemType == other.itemType;
        //If we had metadata, we'd also need to check against it here
    }

    public static void Merge(ItemStack src, ItemStack dst)
    {
        Debug.Assert(src.CanMerge(dst));

        dst.quantity += src.quantity;
        src.quantity = 0;
    }

    #region Interface compatability

    public ItemStack Clone() => new ItemStack(itemType, quantity);
    object ICloneable.Clone() => Clone();

    public Item GetItemType() => itemType;
    public int GetQuantity() => quantity;

    #endregion
}

public interface ReadOnlyItemStack
{
    public Item GetItemType();
    public int GetQuantity();
}