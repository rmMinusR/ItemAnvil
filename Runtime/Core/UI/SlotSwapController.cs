using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{

    [RequireComponent(typeof(ViewInventorySlot))]
    public sealed class SlotSwapController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        private ViewInventorySlot slotView;

        [SerializeField] private InputActionReference startSwapControl;
        [SerializeField] private InputActionReference finishSwapControl;

        private void OnEnable()
        {
            slotView = GetComponent<ViewInventorySlot>();

            //Note: HandleDragStateChange is safe against being called twice in the same frame, so start and finish could be the same control
            startSwapControl.action.performed += HandleDragStateChange;
            finishSwapControl.action.performed += HandleDragStateChange;

            //In case these aren't on a PlayerInput component
            if (!startSwapControl.action.enabled) startSwapControl.action.Enable();
            if (!finishSwapControl.action.enabled) finishSwapControl.action.Enable();
        }

        private void OnDisable()
        {
            startSwapControl.action.performed -= HandleDragStateChange;
            finishSwapControl.action.performed -= HandleDragStateChange;
        }

        private bool beingSwapped = false;
        private int nextFrameCanHandleInput = 0;
        private void SuppressInputsThisFrame() => nextFrameCanHandleInput = Time.frameCount+1;

        private void HandleDragStateChange(InputAction.CallbackContext ctx)
        {
            //Input cooldown
            if (Time.frameCount < nextFrameCanHandleInput) return;

            if (!beingSwapped)
            {
                bool isTarget = kbmHoverTarget == this || (kbmHoverTarget == null && slotView.IsSelected);
                if (ctx.action == startSwapControl.action && isTarget && slotView.IsInteractable() && !slotView.slot.IsEmpty)
                {
                    //Start drag
                    beingSwapped = true;
                    SuppressInputsThisFrame();
                    if (slotView.TryGetComponent(out Animator animator)) animator.SetBool("beingSwapped", true);
                }
            }
            else
            {
                if (ctx.action == finishSwapControl.action && slotView.IsInteractable())
                {
                    //Finish drag
                    SlotSwapController dragTarget = kbmHoverTarget; //First try to fetch from what the mouse hovers over
                    if (!dragTarget && EventSystem.current.currentSelectedGameObject) dragTarget = EventSystem.current.currentSelectedGameObject.GetComponent<SlotSwapController>(); //Then try to use current selection (for gamepad etc)
                    
                    if (dragTarget != null && dragTarget != this && dragTarget.slotView.IsInteractable())
                    {
                        //Try to perform swap
                        dragTarget.SuppressInputsThisFrame();
                        InventorySlot.SwapContents(dragTarget.slotView.slot, slotView.slot);
                        dragTarget.slotView.Select();
                    }

                    beingSwapped = false;
                    SuppressInputsThisFrame();
                    if (slotView.TryGetComponent(out Animator animator)) animator.SetBool("beingSwapped", false);
                }
            }
        }

        private static SlotSwapController kbmHoverTarget;

        public void OnPointerEnter(PointerEventData eventData)
        {
            kbmHoverTarget = this;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (kbmHoverTarget == this) kbmHoverTarget = null;
        }

        //Consume mouse drag events (although they are actually handled elsewhere)
        public void OnBeginDrag(PointerEventData eventData) => eventData.Use();
        public void OnDrag(PointerEventData eventData) => eventData.Use();
        public void OnEndDrag(PointerEventData eventData) => eventData.Use();
    }

}