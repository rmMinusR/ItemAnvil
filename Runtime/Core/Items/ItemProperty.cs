using System;
using UnityEngine;

/// <summary>
/// Describes an active property of an Item. These should generally be additive, not subtractive.
/// </summary>

[Serializable]
public abstract class ItemProperty : ICloneable
{
    protected internal struct TooltipEntry
    {
        public GameObject prefab;
        public int order;
    }

#if USING_TOOLTIPS
    protected internal abstract TooltipEntry GetTooltipEntry(); //TODO implement
#else
    protected internal virtual TooltipEntry GetTooltipEntry() => default; //Give something to override, but don't require
#endif

    public virtual object Clone()
    {
        return MemberwiseClone();
    }
}
