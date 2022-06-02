using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed class ViewItemInventory : MonoBehaviour
{
    public ItemInventory inventory;
    [SerializeField] private ViewItemStack itemStackUIPrefab;
    [SerializeField] private Item[] doNotShow;

    [SerializeField] private GameObject emptyHint;

    [Space]
    [SerializeField] private Transform stackViewParent;

#if USING_INSPECTORSUGAR
    [InspectorReadOnly]
#endif
    [SerializeField] private List<ViewItemStack> stackViews;

    private void Start()
    {
        Debug.Assert(inventory != null, "No inventory connected!", this);
    }
    
    private void Update()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        //Build which ItemStacks to show
        //TODO can we do this more efficiently with enumerators?
        List<ItemStack> stacks = inventory.CloneContents();
        stacks.RemoveAll(s => !s.itemType.showInMainInventory);
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
