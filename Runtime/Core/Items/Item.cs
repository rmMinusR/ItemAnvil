using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Represents the concept of an item, or its type. Not to be confused with an ItemStack, which is an owned item.
/// </summary>
/// <remarks>
/// For example, consider a car: there may be many of a given model, but this represents the model, not any specific car.
/// In scripting, these should be created through Unity and passed in by Inspector, not created or retrieved through code.
/// </remarks>
/// <seealso cref="ItemStack"/>
/// <seealso cref="ItemProperty"/>

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public sealed class Item : ScriptableObject
{
    [Header("Display settings")]
    public string displayName = "Item";
    public Sprite displayIcon;
    [TextArea] public string displayTooltip;
    public bool showInMainInventory = true;

    [field: SerializeField] public PropertyBag<ItemProperty> Properties { get; private set; } = new PropertyBag<ItemProperty>();
}
