using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEditor.Graphs;
using UnityEngine;
using static UnityEngine.UIElements.UxmlAttributeDescription;

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
            if (a.Hooks.ExecuteTrySwapSlots(a, b, cause) != QueryEventResult.Allow || b.Hooks.ExecuteTrySwapSlots(a, b, cause) != QueryEventResult.Allow) return false;

            //Swap (setter will handle hook refreshing)
            ItemStack tmp = a.Contents;
            a.Contents = b.Contents;
            b.Contents = tmp;

            //Run post hook
            a.Hooks.ExecutePostSwapSlots(a, b, cause);
            b.Hooks.ExecutePostSwapSlots(a, b, cause);

            return true;
        }

        public bool CanAccept(ItemStack newStack, object cause)
        {
            return ItemStack.CanMerge(newStack, Contents)
                && inventory.Hooks.CanSlotAccept.Process(h => h(this, newStack.Clone(), newStack, cause)) == QueryEventResult.Allow;
        }

        public void TryAccept(ItemStack newStack, object cause)
        {
            ItemStack finalToMerge = newStack.Clone();
            if (inventory.Hooks.CanSlotAccept.Process(h => h(this, finalToMerge, newStack, cause)) == QueryEventResult.Allow)
            {
                if (IsEmpty)
                {
                    //Create a dummy object to transfer so ItemStack.TryMerge works out of the gate
                    Contents = finalToMerge.Clone();
                    Contents.quantity = 0;
                }

                ItemStack.MergeUnchecked(finalToMerge, Contents);
                newStack.quantity -= finalToMerge.quantity;
            }

        }

        #region Hook lifetime helpers

        private bool slotHooksInstalled = false;
        private Item hookedItemType = null;

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
            if (!slotHooksInstalled)
            {
                slotHooksInstalled = true;
                foreach (SlotProperty p in SlotProperties) p.InstallHooks(this);
            }
        }

        private void UninstallSlotHooks()
        {
            if (slotHooksInstalled)
            {
                slotHooksInstalled = false;
                foreach (SlotProperty p in SlotProperties) p.UninstallHooks(this);
            }
        }

        private void InstallItemHooks()
        {
            Item heldType = Contents?.itemType;
            if (hookedItemType != heldType) UninstallItemHooks();
            if (heldType != null) heldType.InstallHooks(this);
            hookedItemType = heldType;
        }

        private void UninstallItemHooks()
        {
            if (hookedItemType != null) hookedItemType.UninstallHooks(this);
            hookedItemType = null;
        }

#endregion

        #region Hooks interface

        [SerializeField, HideInInspector] private SlotHooksImplDetail _hooksImpl;
        public SlotHooksImplDetail Hooks => _hooksImpl != null ? _hooksImpl : (_hooksImpl = ScriptableObject.CreateInstance<SlotHooksImplDetail>());

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