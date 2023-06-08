using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public sealed class CallableMarketTransaction : MonoBehaviour, IPointerClickHandler
{
    [Space]
    [SerializeField] private InventoryHolder inventoryA;
    [SerializeField] private InventoryHolder inventoryB;
    private ViewItemStack view;
    [SerializeField] private Item itemType;
    [field: SerializeField, Min(0), Tooltip("How many items to buy/sell when right clicking")] public int AltInteractAmount { get; private set; } = 5;
    [field: SerializeField] public Mode mode { get; private set; }

    public enum Mode
    {
        Buying,
        Selling
    }

    public string MainControlName => "LMB"; //TODO stub. Make it work with InputSystem / controller
    public string AltControlName => "RMB"; //TODO stub. Make it work with InputSystem / controller

    private void Start()
    {
        view = GetComponent<ViewItemStack>();
        if (view != null) itemType = view.itemType;
        if (inventoryA == null) inventoryA = GameObject.FindWithTag("Player").GetComponent<InventoryHolder>();
        if (inventoryB == null) inventoryB = view.inventoryHolder != null ? view.inventoryHolder : view.GetComponentInParent<ViewInventory>().inventoryHolder;
        Debug.Assert(inventoryA != null);
    }

    private void Update()
    {
        if (view != null) itemType = view.itemType;
    }

    //Generic form for buy/sell, depending on mode
    public void TryExecute(int quantity)
    {
        switch (mode)
        {
            case Mode.Buying:
                TryBuy(quantity);
                break;

            case Mode.Selling:
                TrySell(quantity);
                break;

            default:
                throw new System.NotImplementedException();
        }
    }

    public void TryBuy(int quantity)
    {
        Marketable m = itemType.Properties.Get<Marketable>();
        if (m == null)
        {
            Debug.LogError(itemType + " has no property 'Marketable'", this);
            return;
        }
        else if (!m.isBuyable)
        {
#if UNITY_EDITOR
            Debug.Log(itemType + " cannot be bought", this);
#endif
            return;
        }

        Transaction transaction = new Transaction(new ItemStack[] { new ItemStack(itemType.Properties.Get<Marketable>().currency, m.buyPrice) },
                                                  new ItemStack[] { new ItemStack(itemType, 1) });
        transaction.MultiplyInPlace(quantity);
        TryPerformTransaction(transaction);
    }

    public void TrySell(int quantity)
    {
        Marketable m = itemType.Properties.Get<Marketable>();
        if (m == null)
        {
            Debug.LogError(itemType + " has no property 'Marketable'", this);
            return;
        }
        else if (!m.isSellable)
        {
#if UNITY_EDITOR
            Debug.LogError(itemType + " cannot be sold", this);
#endif
            return;
        }

        Transaction transaction = new Transaction(new ItemStack[] { new ItemStack(itemType, 1) },
                                                  new ItemStack[] { new ItemStack(itemType.Properties.Get<Marketable>().currency, m.sellPrice) });
        transaction.MultiplyInPlace(quantity);
        TryPerformTransaction(transaction);
    }

    public void TryPerformTransaction(Transaction transaction)
    {
        if(!transaction.TryExchange(inventoryA.inventory, inventoryB.inventory))
        {
            Debug.LogError("Transaction not performed - conditions not met");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
             if (eventData.button == PointerEventData.InputButton.Left ) TryExecute(1);
        else if (eventData.button == PointerEventData.InputButton.Right) TryExecute(AltInteractAmount);
    }
}
