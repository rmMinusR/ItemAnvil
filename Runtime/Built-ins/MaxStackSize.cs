using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Limit item stacking to the given amount. If not present, items will stack infinitely.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: "ItemAnvil", sourceClassName: "MaxStackSize")]
    public class MaxStackSize : ItemProperty
    {
        [Min(1)] public int size = 10;

        private CanSlotAcceptHook MakeHook(ReadOnlyInventorySlot slot)
        {
            int owningSlotID = slot.ID;
            return (ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original, object cause) =>
            {
                //Only execute on our current slot
                if (owningSlotID != slot.ID) return QueryEventResult.Allow;
                
                //Do work
                int currentQuantity = slot.IsEmpty ? 0 : slot.Contents.quantity;
                finalToAccept.quantity = Mathf.Min(size-currentQuantity, finalToAccept.quantity);
                return QueryEventResult.Allow;
            };
        }

        protected internal override void InstallHooks(InventorySlot context)
        {
            context.inventory.Hooks.CanSlotAccept.InsertHook(MakeHook(context), 0);
        }

        protected internal override void UninstallHooks(InventorySlot context)
        {
            context.inventory.Hooks.CanSlotAccept.RemoveHook(MakeHook(context));
        }
    }

}