using System.Collections;
using TMPro;
using rmMinusR.Tooltips;
using UnityEditor;
using UnityEngine;

public sealed class TooltipItemStackName : TooltipPart
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text text;

    private ViewItemStack dataSource;

    protected override void UpdateTarget(Tooltippable newTarget)
    {
        dataSource = newTarget.GetComponent<ViewItemStack>();
        root.SetActive(dataSource != null);

        //Render text if active
        if (root.activeSelf) text.text = dataSource.itemType.displayName;
    }
}
