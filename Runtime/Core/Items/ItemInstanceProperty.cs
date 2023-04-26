using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public abstract class ItemInstanceProperty : ICloneable
{
    public virtual ItemInstanceProperty Clone() => (ItemInstanceProperty) MemberwiseClone();
    object ICloneable.Clone() => Clone();

    public override bool Equals(object obj)
    {
        return obj.GetType() == this.GetType() &&
            this.GetType().GetFields().All(i => i.GetValue(obj) == i.GetValue(this)); //Memberwise equality test. Not efficient, but easy to implement.
    }


    public virtual bool ShouldTick => false;
    public virtual void Tick() { }


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
}
