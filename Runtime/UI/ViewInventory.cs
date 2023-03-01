using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ViewInventory : MonoBehaviour
{
    public InventoryHolder inventoryHolder;
    [SerializeField] private ViewItemStack itemStackUIPrefab;
    [SerializeField] private Item[] doNotShow; //TODO proper filtering

    [SerializeField] private GameObject emptyHint;

    [Space]
    [SerializeField] private Transform stackViewParent;

#if USING_INSPECTORSUGAR
    [InspectorReadOnly] [SerializeField]
#endif
    private List<ViewItemStack> stackViews = new List<ViewItemStack>();

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
        List<ReadOnlyItemStack> stacks = new List<ReadOnlyItemStack>(inventoryHolder.inventory.GetContents());
        stacks.RemoveAll(s => !s.itemType?.showInMainInventory ?? false);
        stacks.RemoveAll(s => doNotShow.Contains(s.itemType));
        
        //Ensure we have the same number of UI elements as ItemStacks
        //TODO can be optimized
        while (stackViews.Count < stacks.Count)
        {
            ViewItemStack view = Instantiate(itemStackUIPrefab.gameObject, stackViewParent).GetComponent<ViewItemStack>();
            view.suppressActiveUpdate = true;
            stackViews.Add(view);
        }
        while (stackViews.Count > stacks.Count)
        {
            Destroy(stackViews[stackViews.Count-1].gameObject);
            stackViews.RemoveAt(stackViews.Count-1);
        }

        //Write stack data
        for (int i = 0; i < stacks.Count; ++i) stackViews[i].WriteStack(stacks[i]);

        //Show/hide empty hint
        if (emptyHint != null) emptyHint.SetActive(stacks.Count == 0);
    }
}
