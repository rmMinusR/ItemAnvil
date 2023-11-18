using rmMinusR.ItemAnvil.Hooks;
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
            slot.inventory.Hooks.CanSlotAccept.InsertHook(_DoFilter, 0);
            slot.inventory.Hooks.TrySortSlot  .InsertHook(_PreventSort, 0);
        }

        protected override void UninstallHooks(InventorySlot slot)
        {
            slot.inventory.Hooks.CanSlotAccept.RemoveHook(_DoFilter);
            slot.inventory.Hooks.TrySortSlot  .RemoveHook(_PreventSort);
        }

        private QueryEventResult _DoFilter(ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original, object cause)
        {
            if (slot.ID != slotID || allowedItems == null) return QueryEventResult.Allow;
            return allowedItems.Matches(original) ? QueryEventResult.Allow : QueryEventResult.Deny;
        }

        private QueryEventResult _PreventSort(ReadOnlyInventorySlot slot, object cause) => QueryEventResult.Deny;
    }

}