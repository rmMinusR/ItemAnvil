using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data container for how items should look and act. One of these must exist per item type.
/// MUST be created through Unity and passed in by Inspector, can not by created or retrieved through code.
/// </summary>
/// <seealso cref="ItemStack"/>
/// <author>Robert Christensen</author>
public abstract class Item : ScriptableObject
{
    [Header("Display settings")]
    public string displayName = "Item";
    public Sprite displayIcon;
    [TextArea] public string displayTooltip;
    public bool showInMainInventory = true;

    [Space]
    [Min(0)] public int buyPrice;
    [Min(0)] public int sellPrice;
}
