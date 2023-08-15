using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.UI
{

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
        [SerializeField] private GameObject emptyHint;

        [Space]
        [SerializeField] private Transform stackViewParent;

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
                ViewInventorySlot view = Instantiate(itemStackUIPrefab, stackViewParent);
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
    }

}