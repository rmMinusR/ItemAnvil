using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rmMinusR.Tooltips
{

    public sealed class TooltippableShownOnHover : Tooltippable, IPointerEnterHandler, IPointerExitHandler
    {
        public void OnPointerEnter(PointerEventData eventData) => Show();

        public void OnPointerExit(PointerEventData eventData) => Hide();
    }

}
