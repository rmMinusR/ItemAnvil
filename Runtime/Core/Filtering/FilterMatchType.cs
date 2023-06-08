using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchType : ItemFilter
{
    [SerializeField] private Item match;

    public override bool Matches(ItemStack itemStack)
    {
        return match == itemStack.itemType;
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }

    public FilterMatchType() { }
    public FilterMatchType(Item type) { match = type; }
}
