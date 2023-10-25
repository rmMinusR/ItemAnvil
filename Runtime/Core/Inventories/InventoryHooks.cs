using System;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Hooks
{
    public enum EventResult
    {
        Allow,
        Deny
    }
}

namespace rmMinusR.ItemAnvil.Hooks.Inventory
{
    public delegate EventResult AddItemHook(ItemStack stack, ref InventorySlot destinationSlot, object cause);

    public delegate EventResult ConsumeItemHook(InventorySlot slot, ref int amountConsumed, object cause);

    //If the swap goes between inventories, will be called on both
    public delegate EventResult SwapSlotsHook(InventorySlot slotA, InventorySlot slotB, object cause);
}
