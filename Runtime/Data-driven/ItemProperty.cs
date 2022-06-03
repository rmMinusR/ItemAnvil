using System;
using UnityEngine;

[Serializable]
public abstract class ItemProperty
{
    protected internal struct TooltipEntry
    {
        public GameObject prefab;
        public int order;
    }

#if USING_TOOLTIPS
    protected internal abstract TooltipEntry GetTooltipEntry();
#else
    protected internal virtual TooltipEntry GetTooltipEntry() => default; //Give something to override, but don't require
#endif
}
