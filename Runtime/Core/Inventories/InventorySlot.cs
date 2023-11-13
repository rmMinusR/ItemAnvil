using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
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
            IExecuteOnlyHookPoint<TrySwapSlotsHook> pre = HookPoint<TrySwapSlotsHook>.Aggregate(a.inventory.Hooks.TrySwapSlots, b.inventory.Hooks.TrySwapSlots);
            if (pre.Process(h => h(a, b, cause)) != QueryEventResult.Allow) return false;
            
            //Swap (setter will handle hook refreshing)
            (b.Contents, a.Contents) = (a.Contents, b.Contents);

            //Run post hook
            IExecuteOnlyHookPoint<PostSwapSlotsHook> post = HookPoint<PostSwapSlotsHook>.Aggregate(a.inventory.Hooks.PostSwapSlots, b.inventory.Hooks.PostSwapSlots);
            post.Process(h => h(a, b, cause));
            
            return true;
        }

        public bool CanAccept(ItemStack newStack, object cause)
        {
            return ItemStack.CanMerge(newStack, Contents)
                && inventory.Hooks.CanSlotAccept.Process(h => h(this, newStack.Clone(), newStack, cause)) == QueryEventResult.Allow;
        }

        public void TryAccept(ItemStack newStack, object cause)
        {
            if (!ItemStack.CanMerge(newStack, Contents)) return;

            ItemStack finalToMerge = newStack.Clone();
            if (IsEmpty)
            {
                //Create a dummy object to transfer so ItemStack.MergeUnchecked works out of the gate
                Contents = finalToMerge.Clone();
                Contents.quantity = 0;

                if (inventory.Hooks.CanSlotAccept.Process(h => h(this, finalToMerge, newStack, cause)) == QueryEventResult.Allow)
                {
                    newStack.quantity -= finalToMerge.quantity;
                    ItemStack.MergeUnchecked(finalToMerge, Contents);
                }
                else Contents = null;
            }
            else
            {
                if (inventory.Hooks.CanSlotAccept.Process(h => h(this, finalToMerge, newStack, cause)) == QueryEventResult.Allow)
                {
                    newStack.quantity -= finalToMerge.quantity;
                    ItemStack.MergeUnchecked(finalToMerge, Contents);
                }
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