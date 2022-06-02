using System.Collections;
using UnityEditor;
using UnityEngine;

public sealed class ViewItemStackTooltip
#if USING_TOOLTIPS
    : Tooltippable
#endif
{
    [SerializeField] private ViewItemStack dataSource;

#if USING_TOOLTIPS
    public override string GetHeader()
    {
        return dataSource.itemType.displayName;
    }

    public override string GetBody()
    {
        return dataSource.itemType.displayTooltip;
    }
#endif
}


//Show warning message if tooltip package is not present.
#if UNITY_EDITOR && !USING_TOOLTIPS
[CustomEditor(typeof(ViewItemStackTooltip))]
class ViewItemStackTooltipEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Optional Tooltip package is missing. This component will have no effect.", MessageType.Error);
        DrawDefaultInspector();
    }
}
#endif