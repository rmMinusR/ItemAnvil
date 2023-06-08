using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchTypes : ItemFilter
{
    [SerializeField] private Item[] matches;

    public override bool Matches(ItemStack itemStack)
    {
        return matches.Contains(itemStack.itemType);
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }
}
