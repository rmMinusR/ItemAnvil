using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [Serializable]
    public sealed class InventorySlot : ReadOnlyInventorySlot
    {
        public InventorySlot(int id)
        {
            _id = id;
        }

        [SerializeField] private ItemStack _contents;
        [SerializeField] [HideInInspector] private int _id;
        
        public bool IsEmpty => ItemStack.IsEmpty(_contents);

        //Boilerplate R/W properties

        /// <summary>
        /// What items are present?
        /// </summary>
        public ItemStack Contents { get => _contents; set => _contents = value; }

        /// <summary>
        /// ID of the slot, such that Inventory.GetSlot(ID) == this. DO NOT WRITE unless you are the owning Inventory.
        /// </summary>
        public int ID { get => _id; set => _id = value; }

        ReadOnlyItemStack ReadOnlyInventorySlot.Contents => _contents; //Interface compat

        public static void SwapContents(InventorySlot a, InventorySlot b)
        {
            ItemStack tmp = a._contents;
            a._contents = b._contents;
            b._contents = tmp;
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