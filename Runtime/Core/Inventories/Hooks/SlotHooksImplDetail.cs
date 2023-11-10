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
    /// [SerializeField, HideInInspector] private SlotHooksImplDetail hooksImpl;
    /// </code>
    /// 
    /// Serialization happens almost constantly when editing (or even sometimes viewing)
    /// objects via Inspector.
    /// </remarks>
    public class SlotHooksImplDetail : ScriptableObject
    {
        public HookPoint<TrySwapSlotsHook > trySwapSlots  = new HookPoint<TrySwapSlotsHook >();
        public HookPoint<PostSwapSlotsHook> postSwapSlots = new HookPoint<PostSwapSlotsHook>();

        /*
         * Hooks execute in ascending priority. For events that only listen for a final result without modifying behavior (such as UI), register for priority = int.MaxValue.
         */
        
        public QueryEventResult ExecuteTrySwapSlots (InventorySlot slotA, InventorySlot slotB, object cause) => trySwapSlots .Process(hook => hook(slotA, slotB,                   cause));
        public void             ExecutePostSwapSlots(InventorySlot slotA, InventorySlot slotB, object cause) => postSwapSlots.Process(hook => hook(slotA, slotB,                   cause));
    }

}
