using System;
using UnityEngine;

[Serializable]
public sealed class ItemStack : ReadOnlyItemStack, ICloneable
{
    [SerializeField] private Item _itemType;
    [SerializeField] [Min(0)] private int _quantity = 1;
    //If we had metadata, it would go here

    public ItemStack() : this(null, 1) { }

    public ItemStack(Item itemType) : this(itemType, 1) { }

    public ItemStack(Item itemType, int quantity)
    {
        this._itemType = itemType;
        this._quantity = quantity;
    }

    public static bool TryMerge(ItemStack src, ItemStack dst)
    {
        if (src._itemType != dst._itemType) return false;

        int totalAmt = src._quantity + dst._quantity;

        dst._quantity = totalAmt;
        //TODO Could do better decoupling
        if (dst._itemType.TryGetProperty(out MaxStackSize s)) dst._quantity = Mathf.Min(dst._quantity, s.size);
        src._quantity = totalAmt-dst._quantity;

        return true;
    }

    #region Interface compatability

    public ItemStack Clone() => new ItemStack(_itemType, _quantity);
    object ICloneable.Clone() => Clone();

    public Item itemType
    {
        get => _itemType;
        set => _itemType = value;
    }

    public int quantity
    {
        get => _quantity;
        set => _quantity = value;
    }

    #endregion
}

public interface ReadOnlyItemStack
{
    public Item itemType
    {
        get;
    }

    public int quantity
    {
        get;
    }
}