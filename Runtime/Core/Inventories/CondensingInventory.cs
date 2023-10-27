using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// An inventory that automatically expands and shrinks to fit its contents. All slots are guaranteed valid.
    /// </summary>
    [Serializable]
    public sealed class CondensingInventory : StandardInventory
    {
        public override void DoSetup()
        {
            //Install inventory-level hooks
            Hooks.postAddItem.InsertHook(_HandleOverflows, 0);
            Hooks.postRemove.InsertHook(_Condense, 0);
            
            //Install item-level hooks
            base.DoSetup();
        }

        //On overflow: add more slots and try again
        private PostEventResult _HandleOverflows(ItemStack overflow, object cause)
        {
            if (overflow.quantity > 0)
            {
                AppendSlot();
                return PostEventResult.Retry;
            }
            else return PostEventResult.Continue;
        }

        //After removal: Remove empty slots
        private void _Condense(object cause) => Condense();

        private void Condense()
        {
            //Remove empty slots
            for (int i = slots.Count-1; i >= 0; --i)
            {
                if (slots[i].IsEmpty) slots.RemoveAt(i);
            }

            //Fix IDs
            Validate();
        }

        #region Obsolete variable upgrader

        public override void Validate()
        {
            //Update obsolete members
#pragma warning disable CS0612
            if (contents.Count != 0)
            {
                if (slots.Count != 0) throw new InvalidOperationException("Cannot update storage -- destination 'slots' must be empty");

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
