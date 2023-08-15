using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    public abstract class BaseViewItemStack : MonoBehaviour
    {
        [Header("Render targets")]
        [SerializeField] protected Image icon;
        [SerializeField] protected TMP_Text count;
        [SerializeField] protected string countFormat = "x{0}";
        [SerializeField] protected TMP_Text sellPrice;
        [SerializeField] protected TMP_Text buyPrice;
        [SerializeField] protected string priceFormat = "${0}";

        protected virtual void WriteType(Item type)
        {
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

        protected void WriteIcon(Sprite sprite)
        {
            if (icon != null)
            {
                if (sprite != null)
                {
                    icon.gameObject.SetActive(true);
                    icon.sprite = sprite;
                }
                else icon.gameObject.SetActive(false);
            }
        }

        protected void WriteCount(int count)
        {
            WriteCount(string.Format(countFormat, count));
        }

        protected void WriteCount(string text)
        {
            if (count != null) count.text = text;
        }
    
        protected void WriteSellPrice(int price)
        {
            WriteSellPrice(string.Format(priceFormat, price));
        }

        protected void WriteSellPrice(string text)
        {
            if (sellPrice != null) sellPrice.text = text;
        }
    
        protected void WriteBuyPrice(int price)
        {
            WriteBuyPrice(string.Format(priceFormat, price));
        }

        protected void WriteBuyPrice(string text)
        {
            if (buyPrice != null) buyPrice.text = text;
        }
    }

}