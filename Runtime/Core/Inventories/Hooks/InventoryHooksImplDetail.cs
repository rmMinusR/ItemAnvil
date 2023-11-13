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
    }
    
}
