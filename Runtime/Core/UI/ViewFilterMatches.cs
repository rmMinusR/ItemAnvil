using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ViewFilterMatches : BaseViewItemStack
{
    [Header("Data source")]
    public Item itemType;
    public InventoryHolder inventoryHolder;

    private void Start()
    {
        Debug.Assert(inventoryHolder != null, "No inventory connected!", this);
        Debug.Assert(inventoryHolder.inventory != null, "Inventory connected, but not configured!", this);
    }

    private void LateUpdate()
    {
        if(itemType != null)
        {
            if (inventoryHolder == null || inventoryHolder.inventory == null) WriteCount("NO INV");
            else WriteCount(inventoryHolder.inventory.Count(itemType));
        }

        WriteType(itemType);
    }
}
