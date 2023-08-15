using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.ItemAnvil.UI
{
    [RequireComponent(typeof(ScrollRect))]
    public sealed class ViewInventory : MonoBehaviour
    {
        public InventoryHolder inventoryHolder;
        [SerializeField] private ViewInventorySlot itemStackUIPrefab;

        [Space]
        [SerializeReference] [TypeSwitcher(order = 2)] private ItemFilter displayFilter = null;
        [SerializeField] private FilterBehavior filterBehavior;
        private enum FilterBehavior
        {
            HideSlot,
            ShowBlank
        }

        [Space]
        [SerializeField] private AnimationCurve autoScrollCurve = AnimationCurve.Linear(0, 0, 0.3f, 1);

        [Space]
        [SerializeField] private GameObject emptyHint;
        [SerializeField] private RectTransform contentContainer;
        
        private List<ViewInventorySlot> slotViews = new List<ViewInventorySlot>();

        private void Start()
        {
            Debug.Assert(inventoryHolder != null, "No inventory connected!", this);
            Debug.Assert(inventoryHolder.inventory != null, "Inventory connected, but not configured!", this);
        }
    
        private void Update()
        {
            UpdateUI();
        }

        //Primary logic function
        public void UpdateUI()
        {
            //Build which ItemStacks to show
            //TODO can we do this more efficiently with enumerators?
            List<InventorySlot> slots = new List<InventorySlot>(inventoryHolder.inventory.Slots);
            if (displayFilter != null) slots.RemoveAll(i => displayFilter.Matches(i.Contents));
            
            //Ensure we have the same number of UI elements as ItemStacks
            //TODO can be optimized
            while (slotViews.Count < slots.Count)
            {
                ViewInventorySlot view = Instantiate(itemStackUIPrefab, contentContainer);
                slotViews.Add(view);
            }
            while (slotViews.Count > slots.Count)
            {
                Destroy(slotViews[slotViews.Count-1].gameObject);
                slotViews.RemoveAt(slotViews.Count-1);
            }

            //Write stack data
            for (int i = 0; i < slots.Count; ++i) slotViews[i].WriteSlot(slots[i]);

            //Show/hide empty hint
            if (emptyHint != null) emptyHint.SetActive(slots.Count == 0);
        }


        #region ScrollTo utility function

        public void ScrollTo(ViewInventorySlot view) => ScrollTo(view.slot.ID);
        public void ScrollTo(ReadOnlyInventorySlot slot) => ScrollTo(slot.ID);
        public void ScrollTo(int slotID)
        {
            if (currentScrollRoutine != null) StopCoroutine(currentScrollRoutine);
            currentScrollRoutine = StartCoroutine(ScrollTo_Ticker(slotID));
        }

        private Coroutine currentScrollRoutine;

        private IEnumerator ScrollTo_Ticker(int slotID)
        {
            ScrollRect scroller = GetComponent<ScrollRect>();
            scroller.StopMovement();
            Vector2 deltaToApply = ScrollRectExtensions.GetAreaOutOfBounds(scroller.viewport, (RectTransform)slotViews[slotID].transform);
            
            float t = 0;
            float endTime = autoScrollCurve.keys.Last().time;
            Vector2 contentStartPos = scroller.content.position;
            while (t < endTime)
            {
                yield return null;
                t += Time.deltaTime;
                scroller.content.position = contentStartPos + deltaToApply*autoScrollCurve.Evaluate(t);
            }

            currentScrollRoutine = null;
        }

        #endregion

    }

}