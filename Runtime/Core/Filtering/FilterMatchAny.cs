using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public sealed class FilterMatchAny : ItemFilter
    {
        [SerializeReference] [TypeSwitcher] public List<ItemFilter> criteria;

        public override bool Matches(ReadOnlyItemStack itemStack)
        {
            return criteria.Any(c => c.Matches(itemStack));
        }

        public override ItemFilter Clone()
        {
            return new FilterMatchAny()
            {
                criteria = criteria.Select(i => i.Clone()).ToList()
            };
        }
    }

}