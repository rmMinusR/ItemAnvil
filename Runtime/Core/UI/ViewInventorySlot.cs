using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    public sealed class ViewInventorySlot : BaseViewItemStack
    {
        public InventoryHolder inventoryHolder { get; internal set; }
        public ReadOnlyItemStack mostRecentStack { get; private set; }

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
                WriteIcon(blankSprite);
            }
        }
    }

}