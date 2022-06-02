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
    public ItemInventory inventory;

#if USING_INSPECTORSUGAR
    [InspectorReadOnly]
#endif
    public bool suppressActiveUpdate = false;

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
            Debug.Assert(inventory != null, "No inventory connected!", this);
        }
    }

    private void LateUpdate()
    {
        if(!suppressActiveUpdate) ActiveUpdate();
    }

    private void ActiveUpdate()
    {
        if(itemType == null)
        {
            WriteIcon(null);
            WriteCount("NO ITEM");
            WriteSellPrice("NO ITEM");
            WriteBuyPrice ("NO ITEM");
        }
        else
        {
            WriteIcon( itemType.displayIcon );

            WriteSellPrice(itemType.sellPrice);
            WriteBuyPrice (itemType.buyPrice );

            if (inventory == null) WriteCount("NO INV");
            else WriteCount(inventory.Count(itemType));
        }
    }

    public void WriteStack(ItemStack src)
    {
        itemType = src.itemType;
        WriteIcon(src.itemType.displayIcon);
        WriteCount(src.quantity);
        WriteSellPrice(src.itemType.sellPrice);
        WriteBuyPrice (src.itemType.buyPrice );
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
