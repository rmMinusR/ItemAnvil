using System;

namespace rmMinusR.ItemAnvil
{
    public abstract class SlotProperty : ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected internal abstract void InstallHooks(InventorySlot inventorySlot);
        protected internal abstract void UninstallHooks(InventorySlot inventorySlot);
    }
}