using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    [RequireComponent(typeof(ViewInventorySlot))]
    public sealed class SlotDragController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ViewInventorySlot slotView;

        private void Start()
        {
            slotView = GetComponent<ViewInventorySlot>();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!slotView.slot.IsEmpty)
            {
                eventData.Use();
                EventSystem.current.SetSelectedGameObject(gameObject);
            }
        }

        public void OnDrag(PointerEventData eventData) { }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (!slotView.slot.IsEmpty && eventData.pointerCurrentRaycast.gameObject != null)
            {
                SlotDragController dragTarget = eventData.pointerCurrentRaycast.gameObject.GetComponentInParent<SlotDragController>();
                if (dragTarget != null)
                {
                    InventorySlot.SwapContents(dragTarget.slotView.slot, slotView.slot);
                    dragTarget.slotView.Select();
                    eventData.Use();
                }
            }
        }
    }

}