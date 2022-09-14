using System;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchItemType : ItemStackFilter
{
    [SerializeField] private Item itemType;

    public override bool Matches(ItemStack itemStack)
    {
        return itemStack.itemType == itemType;
    }
}
