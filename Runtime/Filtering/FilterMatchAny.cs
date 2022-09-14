using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public sealed class FilterMatchAny : ItemStackFilter
{
#if USING_SUBCLASS_SELECTOR
    [SubclassSelector]
#endif
    [SerializeReference] private List<ItemStackFilter> criteria;

    public override bool Matches(ItemStack itemStack)
    {
        return criteria.Any(c => c.Matches(itemStack));
    }
}
