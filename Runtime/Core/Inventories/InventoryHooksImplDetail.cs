using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Hooks.Inventory
{

    /// <summary>
    /// Internal implementation detail to workaround a quirk of the Unity serializer.
    /// By holding events for an Inventory implementation, it allows hooks to persist
    /// across serialization.
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
    internal class InventoryHooksImplDetail : ScriptableObject
    {
        #region Helper details

        private struct HookContainer<T> where T : class
        {
            public T hook;
            public int priority;
        }

        private static void InsertHook<T>(List<HookContainer<T>> hooks, T hook, int priority) where T : class
        {
            HookContainer<T> container = new HookContainer<T>()
            {
                hook = hook,
                priority = priority
            };

            if (hooks.Count == 0) hooks.Add(container);
            else
            {
                int insertIndex = hooks.FindIndex(x => x.priority >= priority);
                if (insertIndex != -1) hooks.Insert(insertIndex, container); //There exist hooks that should execute after this one. Insert before them.
                else hooks.Add(container); //This hooks should execute after all others. Append.
            }
        }

        private static void RemoveHook<T>(List<HookContainer<T>> hooks, T hook) where T : class
        {
            hooks.RemoveAll(i => i.hook == hook);
        }

        #endregion

        private List<HookContainer<AddItemHook    >> _addItem;
        private List<HookContainer<ConsumeItemHook>> _consumeItem;
        private List<HookContainer<SwapSlotsHook  >> _swapSlots;


        /*
         * Hooks execute in ascending priority. For events that only listen without modifying behavior (such as UI), register for priority = int.MaxValue.
         */

        public void HookAddItem   (AddItemHook     listener, int priority) => InsertHook(_addItem    , listener, priority);
        public void HookRemoveItem(ConsumeItemHook listener, int priority) => InsertHook(_consumeItem, listener, priority);
        public void HookSwapSlots (SwapSlotsHook   listener, int priority) => InsertHook(_swapSlots  , listener, priority);

        public void UnhookAddItem   (AddItemHook     listener) => RemoveHook(_addItem    , listener);
        public void UnhookRemoveItem(ConsumeItemHook listener) => RemoveHook(_consumeItem, listener);
        public void UnhookSwapSlots (SwapSlotsHook   listener) => RemoveHook(_swapSlots  , listener);

        //TODO: Horrible practice. Is there a better way that's still performant?
        public EventResult ExecuteHookAddItem   (ItemStack stack, ref InventorySlot destinationSlot, object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<AddItemHook    > i in _addItem    ) { res = i.hook(stack, ref destinationSlot, cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecuteHookRemoveItem(InventorySlot slot, ref int amountConsumed,         object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<ConsumeItemHook> i in _consumeItem) { res = i.hook(slot, ref amountConsumed,   cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecuteHookSwapSlots (InventorySlot slotA, InventorySlot slotB,           object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<SwapSlotsHook  > i in _swapSlots  ) { res = i.hook(slotA, slotB,               cause); if (res != EventResult.Allow) break; } return res; }
    }

}
