using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    public sealed class ViewInventorySlot : Selectable, IPointerClickHandler, ISelectHandler
    {
        [SerializeField] private ItemStackViewCommon rendering;
        [SerializeField] private GameObject highlightCursor;

        public InventoryHolder inventoryHolder { get; internal set; }
        public InventorySlot slot { get; internal set; }

        public void WriteSlot(InventorySlot src)
        {
            slot = src;

            if (slot != null && !slot.IsEmpty)
            {
                //Has data, show
                rendering.WriteCount(slot.Contents.quantity);
                rendering.WriteType(slot.Contents.itemType);
            }
            else
            {
                //No data, show blank
                rendering.WriteCount("");
                rendering.WriteIcon(null);
            }
        }

        public void OnPointerClick(PointerEventData eventData) => EventSystem.current.SetSelectedGameObject(gameObject);
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);
            ScrollTo();
            if (TryGetComponent(out Animator animator)) animator.SetBool("isCurrentSelection", true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);
            if (TryGetComponent(out Animator animator)) animator.SetBool("isCurrentSelection", false);
        }

        public void ScrollTo() => GetComponentInParent<ViewInventory>().ScrollTo(this);
    }

}