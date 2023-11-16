using System;

namespace rmMinusR.ItemAnvil
{
    public abstract class SlotProperty : ICloneable
    {
        public virtual object Clone()
        {
            return MemberwiseClone();
        }

        protected abstract void InstallHooks(InventorySlot inventorySlot);
        protected abstract void UninstallHooks(InventorySlot inventorySlot);



        private bool installed = false;

        internal void _InstallHooks(InventorySlot inventorySlot)
        {
            if (!installed)
            {
                installed = true;
                InstallHooks(inventorySlot);
            }
        }
        
        internal void _UninstallHooks(InventorySlot inventorySlot)
        {
            if (installed)
            {
                installed = false;
                UninstallHooks(inventorySlot);
            }
        }
    }
}