using System;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Describes an active property of an Inventory, such as expanding on overflow, or restricting item types.
    /// </summary>

    [Serializable]
    public abstract class InventoryProperty : ICloneable
    {
        [SerializeReference] private Inventory _inventory;
        protected Inventory inventory => _inventory;
        
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        internal void _InstallHooks(Inventory _inventory)
        {
            if (this._inventory == null)
            {
                this._inventory = _inventory;
                InstallHooks();
            }
        }

        protected abstract void InstallHooks();
    }

}