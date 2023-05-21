using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemStack))]
public class ItemStackDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent mainLabel)
    {
        //Helpers since GUILayout isn't available here
        float workingAreaStart = position.yMin;
        Rect buildRect(float height)
        {
            Rect @out = Rect.MinMaxRect(position.xMin, workingAreaStart, position.xMax, workingAreaStart+height);
            workingAreaStart += height;
            return @out;
        }
        //Rect contentRect = Rect.MinMaxRect(position.xMin, position.yMin, position.xMax, position.yMin+EditorGUIUtility.singleLineHeight);

        //Try to draw field label, if it exists (and isn't a list)
        if (mainLabel != null && mainLabel.text.StartsWith("Element "))
        {
            EditorGUI.LabelField(buildRect(EditorGUIUtility.singleLineHeight), mainLabel);
        }

        //Main line
        {
            Rect contentRect = buildRect(EditorGUIUtility.singleLineHeight);

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

        //Draw instance properties
        {
            SerializedProperty instanceProperties = property.FindPropertyRelative("_instanceProperties");
            Rect instancePropertiesRect = buildRect(EditorGUI.GetPropertyHeight(instanceProperties));
            EditorGUI.PropertyField(instancePropertiesRect, instanceProperties);
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        height += EditorGUIUtility.singleLineHeight; //Idk why but this fixes some alignment issues
        height += EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_instanceProperties"));

        return height;
    }



    static List<Type> GetPropertyTypes()
    {
        if (__propertyTypeCache == null)
        {
            __propertyTypeCache = new List<Type>();
            __propertyTypeCache.AddRange(TypeCache.GetTypesDerivedFrom(typeof(ItemInstanceProperty)));
            __propertyTypeCache.RemoveAll(t => t.IsAbstract);
        }

        return __propertyTypeCache;
    }
    static List<Type> __propertyTypeCache;
}
