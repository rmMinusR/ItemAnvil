using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Hooks.Inventory
{
    public interface IInventoryHooks
    {
        public HookPoint<CanAddItemHook   > CanAddItem    { get; }
        public HookPoint<CanSlotAcceptHook> CanSlotAccept { get; }
        public HookPoint<PostAddItemHook  > PostAddItem   { get; }
        public HookPoint<TryRemoveItemHook> TryRemoveItem { get; }
        public HookPoint<PostRemoveHook   > PostRemove    { get; }
        public HookPoint<TrySortSlotHook  > TrySortSlot   { get; }
        public HookPoint<PostSortHook     > PostSort      { get; }
    }

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
    public class InventoryHooksImplDetail : ScriptableObject, IInventoryHooks
    {
        public HookPoint<CanAddItemHook   > CanAddItem    { get; } = new HookPoint<CanAddItemHook   >();
        public HookPoint<CanSlotAcceptHook> CanSlotAccept { get; } = new HookPoint<CanSlotAcceptHook>();
        public HookPoint<PostAddItemHook  > PostAddItem   { get; } = new HookPoint<PostAddItemHook  >();
        public HookPoint<TryRemoveItemHook> TryRemoveItem { get; } = new HookPoint<TryRemoveItemHook>();
        public HookPoint<PostRemoveHook   > PostRemove    { get; } = new HookPoint<PostRemoveHook   >();
        public HookPoint<TrySortSlotHook  > TrySortSlot   { get; } = new HookPoint<TrySortSlotHook  >();
        public HookPoint<PostSortHook     > PostSort      { get; } = new HookPoint<PostSortHook     >();

        /*
         * Hooks execute in ascending priority. For events that only listen for a final result without modifying behavior (such as UI), register for priority = int.MaxValue.
         */
        
        public QueryEventResult ExecuteCanAddItem   (ItemStack final, ReadOnlyItemStack original                                     , object cause) => CanAddItem   .Process(hook => hook(final, original,                cause));
        public QueryEventResult ExecuteCanSlotAccept(ReadOnlyInventorySlot slot, ItemStack finalToAccept, ReadOnlyItemStack original , object cause) => CanSlotAccept.Process(hook => hook(slot, finalToAccept, original,  cause));
        public PostEventResult  ExecutePostAddItem  (ItemStack stack                                                                 , object cause) => PostAddItem  .Process(hook => hook(stack,                          cause));
        public QueryEventResult ExecuteTryRemoveItem(ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => TryRemoveItem.Process(hook => hook(slot, removed, originalRemoved, cause));
        public void             ExecutePostRemove   (                                                                                  object cause) => PostRemove   .Process(hook => hook(                                cause));
        public QueryEventResult ExecuteTrySort      (ReadOnlyInventorySlot slot                                                      , object cause) => TrySortSlot  .Process(hook => hook(slot,                           cause));
        public PostEventResult  ExecutePostSort     (                                                                                  object cause) => PostSort     .Process(hook => hook(                                cause));
    }

}
