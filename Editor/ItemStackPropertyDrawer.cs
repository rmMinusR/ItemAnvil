using System;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemStack))]
public class ItemStackPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent mainLabel)
    {
        Rect contentRect;

        //Try to draw field label, if it exists
        if (mainLabel != null)
        {
            Rect mainLabelRect = Rect.MinMaxRect(position.xMin, position.yMin, position.xMin+EditorGUIUtility.labelWidth, position.yMax);
            EditorGUI.LabelField(mainLabelRect, mainLabel);
            contentRect = Rect.MinMaxRect(position.xMin+EditorGUIUtility.labelWidth, position.yMin, position.xMax, position.yMax);
        }
        else contentRect = position;

        //Calculate content rects
        const int COUNT_LABEL_WIDTH = 15;
        float split = Mathf.Lerp(contentRect.xMin, contentRect.xMax, 3/5f);
        Rect itemTypeRect   = Rect.MinMaxRect(contentRect.xMin       , contentRect.yMin, split                  , contentRect.yMax);
        Rect countLabelRect = Rect.MinMaxRect(split                  , contentRect.yMin, split+COUNT_LABEL_WIDTH, contentRect.yMax);
        Rect countRect      = Rect.MinMaxRect(split+COUNT_LABEL_WIDTH, contentRect.yMin, contentRect.xMax       , contentRect.yMax);

        //Draw item type
        SerializedProperty itemType = property.FindPropertyRelative("_itemType");
        itemType.objectReferenceValue = EditorGUI.ObjectField(itemTypeRect, itemType.objectReferenceValue, typeof(Item), false);

        //Draw "x"
        GUIStyle centeredLabel = new GUIStyle(EditorStyles.label);
        centeredLabel.alignment = TextAnchor.MiddleCenter;
        EditorGUI.LabelField(countLabelRect, "x", centeredLabel);

        //Draw quantity
        SerializedProperty quantity = property.FindPropertyRelative("_quantity");
        quantity.intValue = EditorGUI.IntField(countRect, quantity.intValue);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight;
    }
}
