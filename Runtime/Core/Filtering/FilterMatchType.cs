using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchType : ItemFilter
{
    public Item match;

    public override bool Matches(ReadOnlyItemStack itemStack)
    {
        return match == itemStack.itemType;
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }
}
