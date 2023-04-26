using System;
using UnityEngine;

public class Marketable : ItemProperty
{
    public Item currency;
    public bool isBuyable;
    [Min(0)] public int buyPrice;
    public bool isSellable;
    [Min(0)] public int sellPrice;

    protected internal override TooltipEntry GetTooltipEntry()
    {
        throw new NotImplementedException();
    }
}
