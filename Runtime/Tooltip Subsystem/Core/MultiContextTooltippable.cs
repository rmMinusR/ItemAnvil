using UnityEngine.EventSystems;

namespace rmMinusR.Tooltips
{

    public sealed class MultiContextTooltippable : Tooltippable, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        private bool _hasContext = false;
        
        private bool HasContext
        {
            get => _hasContext;
            set
            {
                if (_hasContext != value)
                {
                    if (value) Show();
                    else Hide();
                }

                _hasContext = value;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => HasContext = true;
        public void OnPointerExit (PointerEventData eventData) => HasContext = false;
        public void OnSelect  (BaseEventData eventData) => HasContext = true;
        public void OnDeselect(BaseEventData eventData) => HasContext = false;
    }

}