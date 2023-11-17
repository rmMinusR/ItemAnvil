using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// An inventory with a fixed number of slots. Attempting to add more items when full will fail. Slots may be null if empty.
    /// </summary>
    [Serializable]
    public sealed class FixedSlotInventory : StandardInventory
    {
        public FixedSlotInventory() : base() { }
        public FixedSlotInventory(int count) : base(count) { }

        //OBSOLETE: Does nothing. Use StandardInventory instead.

        #region Obsolete variable upgrader

        public override void Validate()
        {
            //Update obsolete members
#pragma warning disable CS0612
            if (contents.Count != 0)
            {
                if (slots.Any(s => !s.IsEmpty || s.SlotProperties.Count!=0)) throw new InvalidOperationException("Cannot update storage -- destination 'slots' must be empty");

                slots.Clear();
                for (int i = 0; i < contents.Count; ++i)
                {
                    AppendSlot().Contents = contents[i];
                }
                contents.Clear();

            }
#pragma warning restore CS0612

            base.Validate();
        }

        /// <summary>
        /// OUTDATED as of 0.5.1 - use "slots" instead
        /// Validate() should handle the upgrade gracefully
        /// </summary>
        [HideInInspector, Obsolete]
        [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

        #endregion Obsolete variable upgrader
    }

}