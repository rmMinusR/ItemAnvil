using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public sealed class CallableMarketTransaction : MonoBehaviour, IPointerClickHandler
{
    [Space]
    [SerializeField] private CondensingInventory inventoryA;
    [SerializeField] private CondensingInventory inventoryB;
    private ViewItemStack view;
    [SerializeField] private Item itemType;
    [SerializeField] private Item currencyItem;

    [Space]
    [SerializeField] private UnityEvent onLMB;
    [SerializeField] private UnityEvent onRMB;

    private void Start()
    {
        view = GetComponent<ViewItemStack>();
        if (view != null) itemType = view.itemType;
        if (inventoryA == null) inventoryA = GameObject.FindWithTag("Player").GetComponent<CondensingInventory>();
        if (inventoryB == null) inventoryB = view.inventory != null ? view.inventory : view.GetComponentInParent<ViewInventory>().inventory;
        Debug.Assert(inventoryA != null);
    }

    private void Update()
    {
        if (view != null) itemType = view.itemType;
    }

    public void TryBuy(int quantity)
    {
        Marketable m = itemType.GetProperty<Marketable>();
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

        Transaction transaction = new Transaction(new ItemStack[] { new ItemStack(currencyItem, m.buyPrice) },
                                                  new ItemStack[] { new ItemStack(itemType    , 1         ) });
        transaction.MultiplyInPlace(quantity);
        TryPerformTransaction(transaction);
    }

    public void TrySell(int quantity)
    {
        Marketable m = itemType.GetProperty<Marketable>();
        if (m == null)
        {
            Debug.LogError(itemType + " has no property 'Marketable'", this);
            return;
        }
        else if (!m.isSellable)
        {
#if UNITY_EDITOR
            Debug.Log(itemType + " cannot be sold", this);
#endif
            return;
        }

        Transaction transaction = new Transaction(new ItemStack[] { new ItemStack(itemType    , 1          ) },
                                                  new ItemStack[] { new ItemStack(currencyItem, m.sellPrice) });
        transaction.MultiplyInPlace(quantity);
        TryPerformTransaction(transaction);
    }

    public void TryPerformTransaction(Transaction transaction)
    {
        Debug.Log("A= "+inventoryA+" B= "+inventoryB);
        transaction.Log();
        if(!transaction.TryExchange(inventoryA, inventoryB))
        {
            Debug.LogError("Failed - conditions not met");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
             if (eventData.button == PointerEventData.InputButton.Left ) onLMB.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right) onRMB.Invoke();
    }
}
