using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data structure describing how items should look and act. One of these must exist per item type.
/// MUST be created through Unity and passed in by Inspector, can not by created or retrieved through code.
/// </summary>
/// <seealso cref="ItemStack"/>
/// <seealso cref="ItemProperty"/>

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public sealed class Item : ScriptableObject
{
    [Header("Display settings")]
    public string displayName = "Item";
    public Sprite displayIcon;
    [TextArea] public string displayTooltip;
    
    [Space]
    public List<ItemCategory> categories = new List<ItemCategory>();

    [field: Space, SerializeField] public PropertyBag<ItemProperty> Properties { get; private set; } = new PropertyBag<ItemProperty>();
}
