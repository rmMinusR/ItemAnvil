using System;
using UnityEngine;

/// <summary>
/// Represents that a commodity can be bought and sold on the market.
/// </summary>
public class Marketable : ItemProperty
{
    public Item currency;

    public bool isBuyable;
    [Min(0)] public int buyPrice;

    public bool isSellable;
    [Min(0)] public int sellPrice;
}
