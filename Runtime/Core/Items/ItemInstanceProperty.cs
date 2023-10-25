using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public abstract class ItemInstanceProperty : ICloneable
    {
        public virtual ItemInstanceProperty Clone() => (ItemInstanceProperty) MemberwiseClone();
        object ICloneable.Clone() => Clone();

        public override bool Equals(object obj)
        {
            return obj.GetType() == this.GetType() &&
                this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).All(i => { //Memberwise equality test. Not efficient, but easy to implement.
                    object a = i.GetValue(obj);
                    object b = i.GetValue(this);
                    return a == b || a.Equals(b);
                });
        }

        public override int GetHashCode()
        {
            int hash = base.GetHashCode();
            hash ^= this.GetType().GetHashCode();
            foreach (FieldInfo fi in this.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)) hash ^= fi.GetValue(this)?.GetHashCode() ?? 0;
            return hash;
        }

        public virtual void Tick(InventorySlot slot, Inventory inventory, Component @object) { }
    }

}