using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchCategory : ItemStackFilter
{
    [SerializeField] private ItemCategory category;

    public override bool Matches(ItemStack itemStack)
    {
        return itemStack.itemType.categories.Contains(category);
    }
}
