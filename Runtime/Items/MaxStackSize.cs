using System;
using UnityEngine;

public class MaxStackSize : ItemProperty
{
    [Min(1)] public int size = 10;
    protected internal override TooltipEntry GetTooltipEntry() => default;
}
