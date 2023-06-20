using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace rmMinusR.ItemAnvil.UI
{

    [RequireComponent(typeof(TMP_Dropdown))]
    public sealed class SortButtonController : MonoBehaviour
    {
        [SerializeField] private ViewInventory binder;
        private Inventory inventory => binder.inventoryHolder.inventory;

        private TMP_Dropdown dropdown;

        private void Start()
        {
            dropdown = GetComponent<TMP_Dropdown>();
            dropdown.ClearOptions();
            dropdown.AddOptions(new List<string>()
            {
                "Sort...",
                "Name A-Z",
                "Name Z-A",
                "Quantity ^",
                "Quantity V",
                "Price ^",
                "Price V",
                "Properties ^",
                "Properties V"
            });

            dropdown.onValueChanged.AddListener(i =>
            {
                dropdown.SetValueWithoutNotify(0);
                Action callback = i switch
                {
                    1 => SortByNameAZ,
                    2 => SortByNameZA,
                    3 => SortByQuantityDesc,
                    4 => SortByQuantityAsc,
                    5 => SortByPriceDesc,
                    6 => SortByPriceAsc,
                    7 => SortByInstancePropertyCountDesc,
                    8 => SortByInstancePropertyCountAsc,
                    _ => throw new NotImplementedException(),
                };
                callback();
            });

            //FIXME there has to be a better way to do this
        }

        public void SortByNameAZ() => inventory.Sort(NameComparer.INSTANCE_AZ);
        public void SortByNameZA() => inventory.Sort(NameComparer.INSTANCE_ZA);

        private class NameComparer : IComparer<ReadOnlyItemStack>
        {
            public static NameComparer INSTANCE_AZ = new NameComparer();
            public static NameComparer INSTANCE_ZA = new NameComparer() { invert = true };

            private bool invert = false;

            public int Compare(ReadOnlyItemStack x, ReadOnlyItemStack y) => CaseInsensitiveComparer.Default.Compare(x?.itemType.displayName ?? "", y?.itemType.displayName ?? "") * (invert?-1:1);
        }

        public void SortByQuantityDesc() => inventory.Sort(i => -i?.quantity ?? 0);
        public void SortByQuantityAsc () => inventory.Sort(i =>  i?.quantity ?? 0);

        public void SortByPriceDesc() => inventory.Sort(i => -EvalPrice(i?.itemType) ?? 0);
        public void SortByPriceAsc () => inventory.Sort(i =>  EvalPrice(i?.itemType) ?? 0);
        private float? EvalPrice(Item type)
        {
            if (type == null || !type.Properties.TryGet(out Marketable market)) return null;

            if (market.isBuyable && market.isSellable) return Mathf.Max(market.buyPrice, market.sellPrice);
            else if (market.isBuyable) return market.buyPrice;
            else if (market.isSellable) return market.sellPrice;
            else return null;
        }

        public void SortByInstancePropertyCountDesc() => inventory.Sort(i => -i?.instanceProperties.Count() ?? 0);
        public void SortByInstancePropertyCountAsc () => inventory.Sort(i =>  i?.instanceProperties.Count() ?? 0);
    }

}