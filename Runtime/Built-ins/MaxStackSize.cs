using System;
using UnityEngine;

public class MaxStackSize : ItemProperty
{
    [Min(1)] public int size = 10;
}
