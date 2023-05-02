using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemStack))]
public class ItemStackPropertyDrawer : PropertyDrawer
{
    bool instancePropertiesFoldout = false;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent mainLabel)
    {
        Rect contentRect = Rect.MinMaxRect(position.xMin, position.yMin, position.xMax, position.yMin+EditorGUIUtility.singleLineHeight);

        //Try to draw field label, if it exists (and isn't a list)
        if (mainLabel != null && mainLabel.text.StartsWith("Element "))
        {
            Rect mainLabelRect = Rect.MinMaxRect(position.xMin, position.yMin, position.xMin+EditorGUIUtility.labelWidth, position.yMin+EditorGUIUtility.singleLineHeight);
            EditorGUI.LabelField(mainLabelRect, mainLabel);
            contentRect = Rect.MinMaxRect(position.xMin+EditorGUIUtility.labelWidth, contentRect.yMin, contentRect.xMax, contentRect.yMax);
        }

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

        //Draw instance properties
        Rect instancePropertiesRect = Rect.MinMaxRect(contentRect.xMin + 20, position.yMin+EditorGUIUtility.singleLineHeight, position.xMax, position.yMax);
        {
            Rect workingRect = Rect.MinMaxRect(instancePropertiesRect.xMin, instancePropertiesRect.yMin, instancePropertiesRect.xMax, instancePropertiesRect.yMin+EditorGUIUtility.singleLineHeight);
            instancePropertiesFoldout = EditorGUI.Foldout(workingRect, instancePropertiesFoldout, "Instance Properties");
            workingRect.y += workingRect.height;

            if (instancePropertiesFoldout)
            {
                SerializedProperty instanceProperties = property.FindPropertyRelative("_instanceProperties");
                List<string> existingPropertyTypes = new List<string>();
                for (int i = 0; i < instanceProperties.arraySize; ++i)
                {
                    SerializedProperty prop = instanceProperties.GetArrayElementAtIndex(i).FindPropertyRelative("property");
                    existingPropertyTypes.Add(prop.managedReferenceFieldTypename.Split(' ').Last());
                    workingRect = Rect.MinMaxRect(workingRect.xMin, workingRect.yMin, workingRect.xMax, workingRect.yMin+EditorGUI.GetPropertyHeight(prop, true));
                    EditorGUI.PropertyField(workingRect, prop, null);
                    workingRect.y += EditorGUI.GetPropertyHeight(prop, true);
                }

                //Show add menu
                workingRect = Rect.MinMaxRect(workingRect.xMin, workingRect.yMin, workingRect.xMax, workingRect.yMin + EditorGUIUtility.singleLineHeight);
                bool shouldAdd = GUI.Button(workingRect, "Add...");
                workingRect.y += workingRect.height;
                if (shouldAdd)
                {
                    List<Type> addable = new List<Type>(GetPropertyTypes());
                    addable.RemoveAll(i => existingPropertyTypes.Contains(i?.Name));
                    
                    void onMenuClick(object toAdd)
                    {
                        instanceProperties.InsertArrayElementAtIndex(instanceProperties.arraySize);
                        SerializedProperty prop = instanceProperties.GetArrayElementAtIndex(instanceProperties.arraySize-1).FindPropertyRelative("property");
                        prop.managedReferenceValue = ((Type)toAdd).GetConstructor(new Type[] { }).Invoke(new object[] { });
                        prop.serializedObject.ApplyModifiedProperties();
                    }

                    GenericMenu menu = new GenericMenu();
                    foreach (Type i in GetPropertyTypes()) menu.AddItem(new GUIContent(i.Name), false, onMenuClick, i);

                    menu.ShowAsContext();
                }
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight;

        //Foldout
        height += EditorGUIUtility.singleLineHeight;

        if (instancePropertiesFoldout)
        {
            SerializedProperty instanceProperties = property.FindPropertyRelative("_instanceProperties");
            for (int i = 0; i < instanceProperties.arraySize; ++i)
            {
                //Height of each instance property
                height += EditorGUI.GetPropertyHeight(instanceProperties.GetArrayElementAtIndex(i).FindPropertyRelative("property"), true);
            }

            height += EditorGUIUtility.singleLineHeight; //Add button
        }

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
