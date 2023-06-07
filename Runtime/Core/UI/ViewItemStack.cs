using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ViewItemStack : MonoBehaviour
{
    [Header("Data source")]
    public Item itemType;
    public InventoryHolder inventoryHolder;
    public ReadOnlyItemStack mostRecentStack { get; private set; } //TODO bad, class should be reworked

    public bool suppressActiveUpdate = false;

    [Space]
    [SerializeField] private Sprite blankSprite;

    [Header("Render targets")]
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text count;
    [SerializeField] private string countFormat = "x{0}";
    [SerializeField] private TMP_Text sellPrice;
    [SerializeField] private TMP_Text buyPrice;
    [SerializeField] private string priceFormat = "${0}";

    //TODO update only when inventory changes

    private void Start()
    {
        if (!suppressActiveUpdate)
        {
            Debug.Assert(inventoryHolder != null, "No inventory connected!", this);
            Debug.Assert(inventoryHolder.inventory != null, "Inventory connected, but not configured!", this);
        }
    }

    private void LateUpdate()
    {
        if(!suppressActiveUpdate) ActiveUpdate();
    }

    private void ActiveUpdate()
    {
        if(itemType != null)
        {
            if (inventoryHolder == null || inventoryHolder.inventory == null) WriteCount("NO INV");
            else WriteCount(inventoryHolder.inventory.Count(itemType));
        }

        WriteType(itemType);
    }

    public void WriteStack(ReadOnlyItemStack src)
    {
        mostRecentStack = src;

        if (src != null && src.itemType != null)
        {
            //Has data, show
            WriteCount(src.quantity);
            WriteType (src.itemType);
        }
        else
        {
            //No data, show blank
            WriteCount("");
            itemType = null;
            WriteIcon(blankSprite);
        }
    }

    private void WriteType(Item type)
    {
        itemType = type;

        if (type == null)
        {
            WriteIcon(null);
            WriteCount("NO ITEM");
            if (sellPrice != null) sellPrice.gameObject.SetActive(true);
            if (buyPrice  != null) buyPrice .gameObject.SetActive(true);
            WriteSellPrice("NO ITEM");
            WriteBuyPrice ("NO ITEM");
        }
        else
        {
            WriteIcon(type.displayIcon);

            if (type.Properties.TryGet(out Marketable m)) //TODO convert to object chaining?
            {
                if (sellPrice != null) sellPrice.gameObject.SetActive(m.isSellable);
                if (buyPrice  != null) buyPrice .gameObject.SetActive(m.isBuyable );

                WriteSellPrice(m.sellPrice);
                WriteBuyPrice (m.buyPrice );
            }
            else
            {
                if (sellPrice != null) sellPrice.gameObject.SetActive(false);
                if (buyPrice  != null) buyPrice .gameObject.SetActive(false);
            }
        }

    }

    private void WriteIcon(Sprite sprite)
    {
        if (icon != null) icon.sprite = sprite;
    }

    private void WriteCount(int count)
    {
        WriteCount(string.Format(countFormat, count));
    }

    private void WriteCount(string text)
    {
        if (count != null) count.text = text;
    }
    
    private void WriteSellPrice(int price)
    {
        WriteSellPrice(string.Format(priceFormat, price));
    }

    private void WriteSellPrice(string text)
    {
        if (sellPrice != null) sellPrice.text = text;
    }
    
    private void WriteBuyPrice(int price)
    {
        WriteBuyPrice(string.Format(priceFormat, price));
    }

    private void WriteBuyPrice(string text)
    {
        if (buyPrice != null) buyPrice.text = text;
    }
}
