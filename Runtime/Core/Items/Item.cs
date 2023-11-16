using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Represents the concept of an item, or its type. Not to be confused with an ItemStack, which is an owned item.
    /// </summary>
    /// <remarks>
    /// For example, consider a car: there may be many of a given model, but this represents the model, not any specific car.
    /// In scripting, these should be created through Unity and passed in by Inspector, not created or retrieved through code.
    /// </remarks>
    /// <seealso cref="ItemStack"/>
    /// <seealso cref="ItemProperty"/>

    [CreateAssetMenu(menuName = "Item Anvil/Item")]
    public sealed class Item : ScriptableObject
    {
        [Header("Display settings")]
        public string displayName = "Item";
        public Sprite displayIcon;
        [TextArea] public string displayTooltip;
        
        [Space]
        public List<ItemCategory> categories = new List<ItemCategory>();

        // DO NOT modify while game is running, as hooks will not be installed!
        [field: Space, SerializeField] public PropertyBag<ItemProperty> Properties { get; private set; } = new PropertyBag<ItemProperty>();

        internal void InstallHooks(InventorySlot inventorySlot)
        {
            foreach (ItemProperty p in Properties) p.InstallHooks(inventorySlot);
        }

        internal void UninstallHooks(InventorySlot inventorySlot)
        {
            foreach (ItemProperty p in Properties) p.UninstallHooks(inventorySlot);
        }
    }

}