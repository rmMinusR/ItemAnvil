using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Hooks.Inventory
{

    /// <summary>
    /// Workaround for a quirk of the Unity serializer. By holding events for an
    /// Inventory implementation, it allows hooks to persist across serialization.
    /// </summary>
    /// <remarks>
    /// When Unity deserializes a field marked SerializeReference, it creates a new
    /// instance before copying values into it. InventoryHolder uses SerializeReference
    /// for Inventories, so it would break everything unserializable (including hooks).
    /// ScriptableObjects are serialized differently: First, they are stored separately,
    /// and second, data is directly written onto them without making a separate copy.
    /// 
    /// <code>
    /// [SerializeField, HideInInspector] private InventoryHooksImplDetail hooksImpl;
    /// </code>
    /// 
    /// Serialization happens almost constantly when editing (or even sometimes viewing)
    /// objects via Inspector.
    /// </remarks>
    public class InventoryHooksImplDetail : ScriptableObject
    {
        public HookPoint<CanAddItemHook   > canAddItem    = new HookPoint<CanAddItemHook   >();
        public HookPoint<CanSlotAcceptHook> canSlotAccept = new HookPoint<CanSlotAcceptHook>();
        public HookPoint<PostAddItemHook  > postAddItem   = new HookPoint<PostAddItemHook  >();
        public HookPoint<TryRemoveItemHook> tryRemoveItem = new HookPoint<TryRemoveItemHook>();
        public HookPoint<PostRemoveHook   > postRemove    = new HookPoint<PostRemoveHook   >();
        public HookPoint<TrySortSlotHook  > trySortSlot   = new HookPoint<TrySortSlotHook  >();
        public HookPoint<PostSortHook     > postSort      = new HookPoint<PostSortHook     >();
        public HookPoint<SwapSlotsHook    > swapSlots     = new HookPoint<SwapSlotsHook    >();

        /*
         * Hooks execute in ascending priority. For events that only listen for a final result without modifying behavior (such as UI), register for priority = int.MaxValue.
         */
        
        public QueryEventResult ExecuteCanAddItem   (ItemStack final, ReadOnlyItemStack original                                     , object cause) => canAddItem   .Process(hook => hook(final, original,                cause));
        public QueryEventResult ExecuteCanSlotAccept(ReadOnlyInventorySlot slot, ReadOnlyItemStack stack                             , object cause) => canSlotAccept.Process(hook => hook(slot, stack,                    cause));
        public PostEventResult  ExecutePostAddItem  (ItemStack stack                                                                 , object cause) => postAddItem  .Process(hook => hook(stack,                          cause));
        public QueryEventResult ExecuteTryRemoveItem(ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => tryRemoveItem.Process(hook => hook(slot, removed, originalRemoved, cause));
        public void             ExecutePostRemove   (                                                                                  object cause) => postRemove   .Process(hook => hook(                                cause));
        public QueryEventResult ExecuteTrySort      (ReadOnlyInventorySlot slot                                                      , object cause) => trySortSlot  .Process(hook => hook(slot,                           cause));
        public PostEventResult  ExecutePostSort     (                                                                                  object cause) => postSort     .Process(hook => hook(                                cause));
    }

}
