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

        public struct HookContainer<T> where T : class
        {
            public T hook;
            public int priority;
        }

        #endregion


        public List<HookContainer<AddItemHook      >> addItem;
        public List<HookContainer<CanSlotAcceptHook>> canSlotAccept;
        public List<HookContainer<PostAddItemHook  >> postAddItem;
        public List<HookContainer<RemoveItemHook   >> removeItem;
        public List<HookContainer<TrySortSlotHook  >> trySortSlot;
        public List<HookContainer<PostSortHook     >> postSort;
        public List<HookContainer<SwapSlotsHook    >> swapSlots;


        /*
         * Hooks execute in ascending priority. For events that only listen without modifying behavior (such as UI), register for priority = int.MaxValue.
         */
        
        //TODO: Horrible practice. Is there a better way that's still performant?
        public EventResult ExecuteAddItem          (ItemStack final, ReadOnlyItemStack original                                     , object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<AddItemHook      > i in addItem      ) { res = i.hook(final, original,                cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecuteCanSlotAccept    (ReadOnlyInventorySlot slot, ReadOnlyItemStack stack                             , object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<CanSlotAcceptHook> i in canSlotAccept) { res = i.hook(slot, stack,                    cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecutePostAddItem      (ItemStack stack                                                                 , object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<PostAddItemHook  > i in postAddItem  ) { res = i.hook(stack,                          cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecuteRemoveItems      (ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<RemoveItemHook   > i in removeItem   ) { res = i.hook(slot, removed, originalRemoved, cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecuteTrySort          (ReadOnlyInventorySlot slot                                                      , object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<TrySortSlotHook  > i in trySortSlot  ) { res = i.hook(slot,                           cause); if (res != EventResult.Allow) break; } return res; }
        public EventResult ExecutePostSort         (                                                                                  object cause) { EventResult res = EventResult.Allow; foreach(HookContainer<PostSortHook     > i in postSort     ) { res = i.hook(                                cause); if (res != EventResult.Allow) break; } return res; }

        /*
        ONELINER:
        public EventResult ExecuteMyHook(...) { EventResult res = EventResult.Allow; foreach(HookContainer<...> i in ...) { res = i.hook(...); if (res != EventResult.Allow) break; } return res; }

        TEMPLATE:
        EventResult res = EventResult.Allow;
        foreach(HookContainer<...> i in ...)
        {
            res = i.hook(...);
            if (res != EventResult.Allow) break;
        }
        return res;
        */
    }

    internal static class InventoryHooksImplDetailHelperFuncs
    {
        public static void InsertHook<T>(this List<InventoryHooksImplDetail.HookContainer<T>> hooks, T hook, int priority) where T : class
        {
            InventoryHooksImplDetail.HookContainer<T> container = new InventoryHooksImplDetail.HookContainer<T>()
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

        public static void RemoveHook<T>(this List<InventoryHooksImplDetail.HookContainer<T>> hooks, T hook) where T : class
        {
            hooks.RemoveAll(i => i.hook == hook);
        }
    }
}
