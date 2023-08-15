using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    public sealed class ViewFilterMatches : MonoBehaviour
    {
        [SerializeField] private ItemStackViewCommon rendering;

        [Header("Data source")]
        [SerializeReference] [TypeSwitcher] public ItemFilter filter = new FilterMatchReference();
        public InventoryHolder inventoryHolder;

        private void Start()
        {
            Debug.Assert(inventoryHolder != null, "No inventory connected!", this);
            Debug.Assert(inventoryHolder.inventory != null, "Inventory connected, but not configured!", this);
        }

        private void LateUpdate()
        {
            if(filter != null)
            {
                if (inventoryHolder == null || inventoryHolder.inventory == null) rendering.WriteCount("NO INV");
                else rendering.WriteCount(inventoryHolder.inventory.Count(filter));
            }
        }
    }

}