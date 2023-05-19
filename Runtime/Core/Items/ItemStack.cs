using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class ItemStack : ReadOnlyItemStack, ICloneable
{
    [SerializeField] private Item _itemType;
    [SerializeField] [Min(0)] private int _quantity = 1;
    [SerializeField] private PropertyBag<ItemInstanceProperty> _instanceProperties;

    public ItemStack() : this(null, 0) { }

    public ItemStack(Item itemType) : this(itemType, 1) { }

    public ItemStack(Item itemType, int quantity)
    {
        this._itemType = itemType;
        this._quantity = quantity;
        _instanceProperties = new PropertyBag<ItemInstanceProperty>();
    }

    public static bool CanMerge(ItemStack src, ItemStack dst)
    {
        return src.itemType == dst.itemType //Item types must match
            && new HashSet<ItemInstanceProperty>(src._instanceProperties).SetEquals(dst._instanceProperties); //Ensure we have the same properties. FIXME GC
    }

    public static bool TryMerge(ItemStack src, ItemStack dst)
    {
        if (!CanMerge(src, dst)) return false;

        int totalAmt = src._quantity + dst._quantity;

        dst._quantity = totalAmt;
        //TODO Could do better decoupling
        if (dst._itemType.properties.TryGet(out MaxStackSize s)) dst._quantity = Mathf.Min(dst._quantity, s.size);
        src._quantity = totalAmt-dst._quantity;

        return true;
    }

    #region Property IO

    public void AddProperty<T>(T prop) where T : ItemInstanceProperty
    {
        _instanceProperties.Add(prop);
    }

    public T GetProperty<T>() where T : ItemInstanceProperty
    {
        return _instanceProperties.Get<T>();
    }

    public void RemoveProperty<T>() where T : ItemInstanceProperty
    {
        _instanceProperties.Remove<T>();
    }

    #endregion

    #region Interface compatability

    public ItemStack Clone()
    {
        ItemStack @out = new ItemStack(_itemType, _quantity);
        @out._instanceProperties = (PropertyBag<ItemInstanceProperty>) _instanceProperties.Clone();
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

    public IEnumerable<ItemInstanceProperty> instanceProperties
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

    public IEnumerable<ItemInstanceProperty> instanceProperties
    {
        get;
    }

    public T GetProperty<T>() where T : ItemInstanceProperty;
}