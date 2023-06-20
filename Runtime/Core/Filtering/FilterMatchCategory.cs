using System;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public sealed class FilterMatchCategory : ItemFilter
    {
        public ItemCategory category;

        public override bool Matches(ReadOnlyItemStack itemStack)
        {
            return itemStack.itemType.categories.Contains(category);
        }

        public override ItemFilter Clone()
        {
            return (ItemFilter) MemberwiseClone();
        }
    }

}