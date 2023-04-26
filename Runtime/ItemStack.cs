﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class ItemStack : ReadOnlyItemStack, ICloneable
{
    [SerializeField] private Item _itemType;
    [SerializeField] [Min(0)] private int _quantity = 1;
    [SerializeField] private List<ItemInstanceProperty> _instanceProperties;
    
    public ItemStack() : this(null, 1) { }

    public ItemStack(Item itemType) : this(itemType, 1) { }

    public ItemStack(Item itemType, int quantity)
    {
        this._itemType = itemType;
        this._quantity = quantity;
        _instanceProperties = new List<ItemInstanceProperty>();
    }

    public static bool CanMerge(ItemStack src, ItemStack dst)
    {
        return src.itemType == dst.itemType //Item types must match
            && Enumerable.SequenceEqual(src.instanceProperties, dst.instanceProperties); //Instance properties must match exactly
    }

    public static bool TryMerge(ItemStack src, ItemStack dst)
    {
        if (!CanMerge(src, dst)) return false;

        int totalAmt = src._quantity + dst._quantity;

        dst._quantity = totalAmt;
        //TODO Could do better decoupling
        if (dst._itemType.TryGetProperty(out MaxStackSize s)) dst._quantity = Mathf.Min(dst._quantity, s.size);
        src._quantity = totalAmt-dst._quantity;

        return true;
    }

    #region Interface compatability

    public ItemStack Clone()
    {
        ItemStack @out = new ItemStack(_itemType, _quantity);
        foreach (ItemInstanceProperty p in _instanceProperties) @out._instanceProperties.Add(p.Clone());
        return @out;
    }
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

    public IReadOnlyList<ItemInstanceProperty> instanceProperties
    {
        get => _instanceProperties;
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

    public IReadOnlyList<ItemInstanceProperty> instanceProperties
    {
        get;
    }
}