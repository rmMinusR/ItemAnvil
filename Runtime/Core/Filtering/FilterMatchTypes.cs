using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchTypes : ItemFilter
{
    public Item[] matches;

    public override bool Matches(ReadOnlyItemStack itemStack)
    {
        return matches.Contains(itemStack.itemType);
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }
}
