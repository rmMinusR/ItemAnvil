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
        [SerializeField] public TMP_Text priceText;
        internal Marketable.Mode priceMode;
        [SerializeField] public string priceFormat = "${0}";

        public void WriteType(Item type)
        {
            if (type == null)
            {
                WriteIcon(null);
                WriteCount("NO ITEM");
                if (priceText) priceText.gameObject.SetActive(true);
                WritePrice("NO ITEM");
            }
            else
            {
                WriteIcon(type.displayIcon);

                if (type.Properties.Contains<Marketable>()) //TODO convert to object chaining?
                {
                    MarketTransactionContext.StaticAssess(type, priceMode, out bool showPrice, out int priceAmount);
                    if (priceText) priceText.gameObject.SetActive(showPrice);
                    WritePrice(priceAmount);
                }
                else
                {
                    if (priceText) priceText.gameObject.SetActive(false);
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
    
        public void WritePrice(int price)
        {
            WritePrice(string.Format(priceFormat, price));
        }

        public void WritePrice(string text)
        {
            if (priceText != null) priceText.text = text;
        }
    }

}