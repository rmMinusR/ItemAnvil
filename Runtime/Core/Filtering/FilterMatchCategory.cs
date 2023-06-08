using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchCategory : ItemFilter
{
    [SerializeField] private ItemCategory category;

    public override bool Matches(ItemStack itemStack)
    {
        return itemStack.itemType.categories.Contains(category);
    }

    public override ItemFilter Clone()
    {
        return (ItemFilter) MemberwiseClone();
    }
}
