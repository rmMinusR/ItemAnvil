namespace rmMinusR.ItemAnvil.Hooks
{
    /// <summary>
    /// Query-type events are used to check the extent if something is eligible, or modify it to satisfy some eligibility constraint.
    /// </summary>
    public enum QueryEventResult
    {
        Allow,
        Deny
    }

    /// <summary>
    /// Post-algorithm events are used to respond to changes made by an algorithm. Some can request that it be re-run once the response resolves.
    /// </summary>
    public enum PostEventResult
    {
        Continue,
        Retry
    }
}

namespace rmMinusR.ItemAnvil.Hooks.Inventory
{
    //Hooks for Inventory.AddItem family
    public delegate QueryEventResult CanAddItemHook   (ItemStack final, ReadOnlyItemStack original,                                      object cause);
    public delegate QueryEventResult TryAddToSlotHook(ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original,  object cause);
    public delegate PostEventResult  PostAddItemHook  (ItemStack stack,                                                                  object cause); //Overflow handling etc

    //Hooks for Inventory.TryRemove and Inventory.RemoveAll
    public delegate QueryEventResult TryRemoveItemHook(ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause);
    public delegate void             PostRemoveHook   (                                                                                  object cause);
    
    //Hooks for Inventory.Sort
    public delegate QueryEventResult TrySortSlotHook  (ReadOnlyInventorySlot slot,                                                       object cause);
    public delegate PostEventResult  PostSortHook     (                                                                                  object cause);

    //If the swap goes between inventories, will be called on both inventories.
    //If only within the same inventory, will only be called once.
    public delegate QueryEventResult TrySwapSlotsHook (InventorySlot slotA, InventorySlot slotB, object cause);
    public delegate void             PostSwapSlotsHook(InventorySlot slotA, InventorySlot slotB, object cause);
}
