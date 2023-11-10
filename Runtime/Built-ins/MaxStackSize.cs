using rmMinusR.ItemAnvil.Hooks;
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

        private QueryEventResult LimitStackSize(ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original, object cause)
        {
            if (!slot.IsEmpty) finalToAccept.quantity = Mathf.Min(size-slot.Contents.quantity, finalToAccept.quantity);
            return QueryEventResult.Allow;
        }

        protected internal override void InstallHooks(InventorySlot context)
        {
            //context.Hooks.CanSlotAccept.InsertHook(LimitStackSize, 0);
        }

        protected internal override void UninstallHooks(InventorySlot context)
        {
            //context.Hooks.CanSlotAccept.RemoveHook(LimitStackSize);
        }
    }

}