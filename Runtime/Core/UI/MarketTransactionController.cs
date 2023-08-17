using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace rmMinusR.ItemAnvil.UI
{

    /// <summary>
    /// Makes the UI object attempt to perform the configured Transaction on click. Alternatively, can be called by UnityEvent.
    /// Requires a <seealso cref="MarketTransactionContext"/> in owning ViewInventory.
    /// </summary>
    [RequireComponent(typeof(ViewInventorySlot))]
    public sealed class MarketTransactionController : MonoBehaviour, IPointerClickHandler
    {
        public MarketTransactionContext context { get; private set; }
        private ViewInventorySlot view;
        public Item itemType => view.slot.Contents.itemType;
        [field: SerializeField, Min(0), Tooltip("How many items to buy/sell when right clicking")] public int BatchQuantity { get; private set; } = 5;
        
        public string MainControlName => "LMB"; //TODO stub. Make it work with InputSystem / controller
        public string AltControlName => "RMB"; //TODO stub. Make it work with InputSystem / controller

        private void Start()
        {
            view = GetComponent<ViewInventorySlot>();
            context = GetComponentInParent<MarketTransactionContext>();
            
            Debug.Assert(context != null, "Needs MarketTransactionContext to know what inventory we're transacting with", this);
        }

        #region Input handling
        
        public void OnPointerClick(PointerEventData eventData)
        {
            if (itemType.Properties.Contains<Marketable>())
            {
                     if (eventData.button == PointerEventData.InputButton.Left ) TryExecute(1);
                else if (eventData.button == PointerEventData.InputButton.Right) TryExecute(BatchQuantity);
            }
        }

        #endregion

        #region Main business logic

        public void TryExecute(int quantity)
        {
            Marketable m = itemType.Properties.Get<Marketable>();
            if (m == null) throw new InvalidOperationException(itemType + " has no property 'Marketable'");

            context.Assess(itemType, out bool operationAllowed, out int currencyAmount);

            if (!operationAllowed)
            {
    #if UNITY_EDITOR
                Debug.LogWarning($"Operation {context.PlayerAction} not allowed for {itemType}", this);
    #endif
                return;
            }

            Transaction transaction = new Transaction(new ItemStack[] { new ItemStack(itemType, 1) },
                                                      new ItemStack[] { new ItemStack(itemType.Properties.Get<Marketable>().currency, currencyAmount) });
            transaction.MultiplyInPlace(quantity);
            
            if(!transaction.TryExchange(context.Self.inventory, context.Other.inventory)) Debug.LogError("Transaction not performed - conditions not met");
        }

        #endregion
    }

}