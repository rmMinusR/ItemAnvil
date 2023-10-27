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
    public delegate EventResult AddItemHook      (ItemStack final, ReadOnlyItemStack original,                                      object cause);
    public delegate EventResult CanSlotAcceptHook(ReadOnlyInventorySlot slot, ReadOnlyItemStack stack,                              object cause);
    public delegate EventResult PostAddItemHook  (ItemStack stack,                                                                  object cause); //Overflow handling etc
    public delegate EventResult RemoveItemHook   (ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause);
    public delegate EventResult TrySortSlotHook  (ReadOnlyInventorySlot slot,                                                       object cause);
    public delegate EventResult PostSortHook     (                                                                                  object cause);

    //If the swap goes between inventories, will be called on both
    //TODO IMPLEMENT
    public delegate EventResult SwapSlotsHook(InventorySlot slotA, InventorySlot slotB, object cause);
}
