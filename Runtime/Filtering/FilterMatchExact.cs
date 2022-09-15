using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchExact : ItemFilter
{
    [SerializeField] private Item itemType;

    public override bool Matches(ItemStack itemStack)
    {
        return itemStack.itemType == itemType;
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }
}
