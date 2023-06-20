using System;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

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

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
    }

}