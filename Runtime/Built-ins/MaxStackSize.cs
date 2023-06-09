using System;
using UnityEngine;

/// <summary>
/// Limit item stacking to the given amount. If not present, items will stack infinitely.
/// </summary>
public class MaxStackSize : ItemProperty
{
    [Min(1)] public int size = 10;
}
