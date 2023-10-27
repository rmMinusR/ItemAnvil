using System;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{
    /// <summary>
    /// Serializable predicate for counting, removing, and restricting items in inventories
    /// </summary>
    [Serializable]
    public abstract class ItemFilter
    {
        public abstract bool Matches(ReadOnlyItemStack itemStack);

        public abstract ItemFilter Clone();
    }

}