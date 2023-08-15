using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{
    [Serializable]
    public sealed class ItemStackViewCommon
    {
        [SerializeField] public Image icon;
        [SerializeField] public TMP_Text count;
        [SerializeField] public string countFormat = "x{0}";
        [SerializeField] public TMP_Text sellPrice;
        [SerializeField] public TMP_Text buyPrice;
        [SerializeField] public string priceFormat = "${0}";

        public void WriteType(Item type)
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

        public void WriteIcon(Sprite sprite)
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

        public void WriteCount(int count)
        {
            WriteCount(string.Format(countFormat, count));
        }

        public void WriteCount(string text)
        {
            if (count != null) count.text = text;
        }
    
        public void WriteSellPrice(int price)
        {
            WriteSellPrice(string.Format(priceFormat, price));
        }

        public void WriteSellPrice(string text)
        {
            if (sellPrice != null) sellPrice.text = text;
        }
    
        public void WriteBuyPrice(int price)
        {
            WriteBuyPrice(string.Format(priceFormat, price));
        }

        public void WriteBuyPrice(string text)
        {
            if (buyPrice != null) buyPrice.text = text;
        }
    }

}