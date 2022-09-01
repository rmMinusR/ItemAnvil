using System.Collections;
using TMPro;
using Tooltips;
using UnityEditor;
using UnityEngine;

public sealed class TooltipItemStackName
#if USING_TOOLTIPS
    : ContentPart
#else
    : MonoBehaviour
#endif
{
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text text;
    [SerializeField] [InspectorReadOnly] private ViewItemStack dataSource;

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

//Show warning message if tooltip package is not present.
#if UNITY_EDITOR && !USING_TOOLTIPS
[CustomEditor(typeof(TooltipItemStackName))]
class ViewItemStackTooltipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Optional Tooltip package is missing. This component will have no effect.", MessageType.Error);
        DrawDefaultInspector();
    }
}
#endif
