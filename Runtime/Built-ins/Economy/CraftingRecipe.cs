using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    public abstract class CraftingRecipe : ScriptableObject, ICraftingRecipe
    {
        /// <summary>
        /// Attempt to perform crafting.
        /// </summary>
        /// <param name="crafter">Inventory that items will be taken from and given to</param>
        /// <param name="multiplier">How many times?</param>
        /// <returns>Whether or not the inventory had enough items. If false, no changes were made.</returns>
        public virtual bool TryExchange(Inventory crafter, int multiplier)
        {
            if(IsValid(crafter, multiplier))
            {
                DoExchange(crafter, multiplier);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Special version for UnityEvents, since they're picky about return values and parameter count
        /// </summary>
        public void TryExchange_UEv(InventoryHolder crafter) => TryExchange(crafter.inventory, 1);

        public abstract bool IsValid(Inventory crafter, int multiplier);

        protected abstract bool DoExchange(Inventory crafter, int multiplier);
    }


    public interface ICraftingRecipe
    {
        public bool TryExchange(Inventory crafter, int multiplier);
        public bool IsValid(Inventory crafter, int multiplier);
    }

}