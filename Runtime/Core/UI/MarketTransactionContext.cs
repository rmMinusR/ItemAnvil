using System;
using UnityEngine;

namespace rmMinusR.ItemAnvil.UI
{

    /// <summary>
    /// Place this on an inventory view to let it perform transactions with another inventory.
    /// </summary>
    /// <seealso cref="MarketTransactionController"/>
    [RequireComponent(typeof(ViewInventory))]
    public sealed class MarketTransactionContext : MonoBehaviour
    {
        public InventoryHolder Self => GetComponent<ViewInventory>().inventoryHolder;
        [field: SerializeField] public InventoryHolder Other { get; private set; }

        private void Start()
        {
            Debug.Assert(Self != null);
            Debug.Assert(Other != null);
        }

        [field: SerializeField, Tooltip("What is the player doing when interacting?")]
        public Marketable.Mode PlayerAction { get; private set; }

        public void Assess(Item type, out bool operationAllowed, out int currencyAmount)
        {
            StaticAssess(type, PlayerAction, out operationAllowed, out currencyAmount);
        }

        public static void StaticAssess(Item type, Marketable.Mode action, out bool operationAllowed, out int currencyAmount)
        {
            Marketable m = type.Properties.Get<Marketable>();
            switch (action)
            {
                case Marketable.Mode.Buying:
                    operationAllowed = m.isBuyable;
                    currencyAmount = m.buyPrice;
                    break;

                case Marketable.Mode.Selling:
                    operationAllowed = m.isSellable;
                    currencyAmount = m.sellPrice;
                    break;

                default: throw new NotImplementedException();
            }
        }
    }

}