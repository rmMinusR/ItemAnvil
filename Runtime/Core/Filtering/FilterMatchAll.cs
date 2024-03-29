﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public sealed class FilterMatchAll : ItemFilter
    {
        [TypeSwitcher]
        [SerializeReference] public List<ItemFilter> criteria;

        public override bool Matches(ReadOnlyItemStack itemStack)
        {
            return criteria.All(c => c.Matches(itemStack));
        }

        public override ItemFilter Clone()
        {
            return new FilterMatchAll()
            {
                criteria = criteria.Select(i => i.Clone()).ToList()
            };
        }
    }

}