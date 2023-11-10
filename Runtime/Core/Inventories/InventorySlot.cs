using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public sealed class InventorySlot : ReadOnlyInventorySlot
    {
        public InventorySlot(int id, Inventory inventory)
        {
            _id = id;
            this.inventory = inventory;
            _slotProperties = new PropertyBag<SlotProperty>();
        }

        public Inventory inventory { get; private set; }
        [SerializeField] private ItemStack _contents;
        [SerializeField] [HideInInspector] private int _id;
        [SerializeField] private PropertyBag<SlotProperty> _slotProperties;

        public bool IsEmpty => ItemStack.IsEmpty(_contents);

        //Boilerplate R/W properties

        /// <summary>
        /// What items are present?
        /// </summary>
        public ItemStack Contents
        {
            get => _contents;
            set
            {
                UninstallItemHooks();
                _contents = value;
                InstallItemHooks();
            }
        }

        /// <summary>
        /// ID of the slot, such that Inventory.GetSlot(ID) == this. DO NOT WRITE unless you are the owning Inventory.
        /// </summary>
        public int ID { get => _id; set => _id = value; }

        /// <summary>
        /// Properties of this slot. Not to be confused with item properties.
        /// </summary>
        public PropertyBag<SlotProperty> SlotProperties => _slotProperties;

        ReadOnlyItemStack ReadOnlyInventorySlot.Contents => _contents; //Interface compat

        public static bool SwapContents(InventorySlot a, InventorySlot b, object cause)
        {
            //Run pre hook
            if (a.hooksImpl.ExecuteTrySwapSlots(a, b, cause) != QueryEventResult.Allow || b.hooksImpl.ExecuteTrySwapSlots(a, b, cause) != QueryEventResult.Allow) return false;

            //Swap (setter will handle hook refreshing)
            ItemStack tmp = a.Contents;
            a.Contents = b.Contents;
            b.Contents = tmp;

            //Run post hook
            a.hooksImpl.ExecutePostSwapSlots(a, b, cause);
            b.hooksImpl.ExecutePostSwapSlots(a, b, cause);

            return true;
        }

        public bool CanAccept(ItemStack newStack)
        {
            return ItemStack.CanMerge(newStack, Contents);
        }

        public void TryAccept(ItemStack newStack)
        {
            if (IsEmpty)
            {
                //Create a dummy object to transfer into
                Contents = newStack.Clone();
                Contents.quantity = 0;
            }

            ItemStack.TryMerge(newStack, Contents);
        }

        #region Hook lifetime helpers

        internal void InstallHooks()
        {
            InstallSlotHooks();
            InstallItemHooks();
        }

        internal void UninstallHooks()
        {
            UninstallSlotHooks();
            UninstallItemHooks();
        }

        private void InstallSlotHooks()
        {
            if (hooksImpl != null) throw new InvalidOperationException($"Slot #{ID}: hooks already installed");
            hooksImpl = ScriptableObject.CreateInstance<SlotHooksImplDetail>();

            foreach (SlotProperty p in SlotProperties) p.InstallHooks(this);
        }

        private void UninstallSlotHooks()
        {
            if (hooksImpl == null) throw new InvalidOperationException($"Slot #{ID}: hooks already uninstalled");

            foreach (SlotProperty p in SlotProperties) p.UninstallHooks(this);

            ScriptableObject.Destroy(hooksImpl);
            hooksImpl = null;
        }

        private void InstallItemHooks()
        {
            if (Contents?.itemType != null) Contents.itemType.InstallHooks(this);
        }

        private void UninstallItemHooks()
        {
            if (Contents?.itemType != null) Contents.itemType.UninstallHooks(this);
        }

        #endregion

        #region Hooks interface

        [SerializeField, HideInInspector] private SlotHooksImplDetail hooksImpl;

        public void HookTrySwapSlot  (TrySwapSlotsHook  listener, int priority) => hooksImpl.trySwapSlots .InsertHook(listener, priority);
        public void HookPostSwapSlots(PostSwapSlotsHook listener, int priority) => hooksImpl.postSwapSlots.InsertHook(listener, priority);
        public void UnhookTrySwapSlot  (TrySwapSlotsHook  listener) => hooksImpl.trySwapSlots .RemoveHook(listener);
        public void UnhookPostSwapSlots(PostSwapSlotsHook listener) => hooksImpl.postSwapSlots.RemoveHook(listener);

        #endregion
    }

    public interface ReadOnlyInventorySlot
    {
        /// <summary>
        /// Is there anything here (ie. can we accept incoming items of any type)? Not the same as null checking.
        /// </summary>
        public bool IsEmpty { get; }

        /// <summary>
        /// ID of the slot, such that Inventory.GetSlot(ID) == this.
        /// </summary>
        public int ID { get; }

        /// <summary>
        /// What items are present?
        /// </summary>
        public ReadOnlyItemStack Contents { get; }
    }

}