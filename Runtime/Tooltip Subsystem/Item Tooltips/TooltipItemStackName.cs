using System.Collections;
using TMPro;
using rmMinusR.Tooltips;
using UnityEditor;
using UnityEngine;

public sealed class TooltipItemStackName : ContentPart
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text text;

#if USING_INSPECTORSUGAR
    [InspectorReadOnly] [SerializeField]
#endif
    private ViewItemStack dataSource;

    protected override void UpdateTarget(Tooltippable newTarget)
    {
        dataSource = newTarget.GetComponent<ViewItemStack>();
        root.SetActive(dataSource != null);
    }

    private void Update()
    {
        text.text = dataSource.itemType.displayName;
    }
}
