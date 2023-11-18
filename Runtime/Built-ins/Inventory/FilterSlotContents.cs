using rmMinusR.ItemAnvil.Hooks;
using System;
using UnityEditor.Graphs;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Limit the items that may go in a slot. Also prevents the slot from being sorted.
    /// </summary>
    public sealed class FilterSlotContents : SlotProperty
    {
        [SerializeReference] public ItemFilter allowedItems;
        int slotID;

        protected override void InstallHooks(InventorySlot slot)
        {
            slotID = slot.ID;
            slot.inventory.Hooks.TryAddToSlot.InsertHook(_DoFilterOnAdd, 0);
            slot.inventory.Hooks.TrySwapSlots.InsertHook(_DoFilterOnSwap, 0);
            slot.inventory.Hooks.TrySortSlot .InsertHook(_PreventSort, 0);
        }

        protected override void UninstallHooks(InventorySlot slot)
        {
            slot.inventory.Hooks.TryAddToSlot.RemoveHook(_DoFilterOnAdd);
            slot.inventory.Hooks.TrySwapSlots.RemoveHook(_DoFilterOnSwap);
            slot.inventory.Hooks.TrySortSlot .RemoveHook(_PreventSort);
        }

        private QueryEventResult _DoFilterOnAdd(ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original, object cause)
        {
            if (slot.ID != slotID || allowedItems == null) return QueryEventResult.Allow;
            return allowedItems.Matches(original) ? QueryEventResult.Allow : QueryEventResult.Deny;
        }

        private QueryEventResult _DoFilterOnSwap(InventorySlot slotA, InventorySlot slotB, object cause)
        {
            if (slotA.ID != slotID && slotB.ID != slotID) return QueryEventResult.Allow;
            if (allowedItems == null) return QueryEventResult.Allow;

            bool good = true;
            good &= slotA.IsEmpty || allowedItems.Matches(slotA.Contents);
            good &= slotB.IsEmpty || allowedItems.Matches(slotB.Contents);
            return good ? QueryEventResult.Allow : QueryEventResult.Deny;
        }

        private QueryEventResult _PreventSort(ReadOnlyInventorySlot slot, object cause) => QueryEventResult.Deny;
    }

}