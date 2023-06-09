using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents an item (or multiple items) that may be owned, used, traded, etc.
/// </summary>
/// <remarks>
/// For example, consider a car: there may be many of a given model. This would represent any specific car, complete with wear and tear, paint, and contents.
/// </remarks>
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

    public ItemStack(Item itemType, int quantity, IEnumerable<ItemInstanceProperty> instanceProperties) : this(itemType, quantity)
    {
        foreach (ItemInstanceProperty i in instanceProperties) _instanceProperties.Add(i.Clone());
    }

    public override string ToString()
    {
        string name = _itemType!=null ? _itemType.displayName : "[null]";
        return $"({name} x{_quantity}, {_instanceProperties.Count} instance properties)";
    }

    /// <summary>
    /// Can two ItemStacks be merged?
    /// </summary>
    /// <param name="src">Stack that will be merged from, and will be consumed in the process</param>
    /// <param name="dst">Stack that will be merged into</param>
    /// <returns>Whether the merge would be allowed</returns>
    public static bool CanMerge(ItemStack src, ItemStack dst)
    {
        return src.itemType == dst.itemType //Item types must match
            && src._instanceProperties.SetEquals(dst._instanceProperties); //Ensure we have the same properties. FIXME GC
    }

    /// <summary>
    /// Attempt to merge two ItemStacks
    /// </summary>
    /// <param name="src">Stack that will be merged from, and will be consumed in the process</param>
    /// <param name="dst">Stack that will be merged into</param>
    /// <returns>Whether the merge was performed. If false, no changes to either stack were made.</returns>
    public static bool TryMerge(ItemStack src, ItemStack dst)
    {
        if (!CanMerge(src, dst)) return false;

        int totalAmt = src._quantity + dst._quantity;

        dst._quantity = totalAmt;
        //TODO Could do better decoupling
        if (dst._itemType.Properties.TryGet(out MaxStackSize s)) dst._quantity = Mathf.Min(dst._quantity, s.size);
        src._quantity = totalAmt-dst._quantity;

        return true;
    }
    
    #region Interface compatability

    /// <summary>
    /// Make a copy of this ItemStack, including instance properties
    /// </summary>
    public ItemStack Clone()
    {
        ItemStack @out = (ItemStack) MemberwiseClone();
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

    public PropertyBag<ItemInstanceProperty> instanceProperties
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

    public PropertyBag<ItemInstanceProperty> instanceProperties
    {
        get;
    }
}