using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(PropertyBag<>))]
public sealed class PropertyBagDrawer : PropertyDrawer
{
    static IEnumerable<SerializedProperty> GetElements(SerializedProperty array)
    {
        for (int i = 0; i < array.arraySize; ++i)
        {
            yield return array.GetArrayElementAtIndex(i);
        }
    }

    const float removeButtonWidth = 70;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty propArr = property.FindPropertyRelative("contents");
        string _getTypenameOfProp(int id)
        {
            string typename = propArr.GetArrayElementAtIndex(id).FindPropertyRelative("value").managedReferenceFullTypename;
            int splitInd = Math.Max(typename.LastIndexOf('.'), typename.LastIndexOf(' ')) + 1;
            typename = typename.Substring(splitInd);
            return typename;
        }

        //Helpers since GUILayout isn't available here
        float workingAreaStart = position.yMin;
        Rect buildRect(float height)
        {
            Rect @out = Rect.MinMaxRect(position.xMin, workingAreaStart, position.xMax, workingAreaStart+height);
            workingAreaStart += height;
            return @out;
        }

        //Show name label
        EditorGUI.LabelField(buildRect(EditorGUIUtility.singleLineHeight), label, EditorStyles.boldLabel);

        //Show add menu
        List<Type> addable = new List<Type>();
        addable.Add(null);
        addable.AddRange(GetPropertyTypes().Where(type => GetElements(propArr).All(container => container.FindPropertyRelative("value").managedReferenceFullTypename != type.Name))); //FIXME this may not work with namespaces...
        string[] names = addable.Select(t => t?.Name ?? "Add...").ToArray();
        int toAdd = EditorGUI.Popup(buildRect(EditorGUIUtility.singleLineHeight), 0, names);
        if (toAdd != 0)
        {
            propArr.InsertArrayElementAtIndex(propArr.arraySize);
            propArr.GetArrayElementAtIndex(propArr.arraySize - 1).FindPropertyRelative("value").managedReferenceValue = addable[toAdd].GetConstructor(new Type[] { }).Invoke(new object[] { });
        }

        //Show property list
        int? toRemove = null;
        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < propArr.arraySize; ++i)
            {
                buildRect(EditorGUIUtility.singleLineHeight); //Separator

                Rect headerRect = buildRect(EditorGUIUtility.singleLineHeight);
                Rect headerLabelRect = headerRect; headerLabelRect.xMax -= removeButtonWidth;
                Rect removeButtonRect = headerRect; removeButtonRect.xMin = headerLabelRect.xMax;

                //Property header
                EditorGUI.LabelField(headerLabelRect, _getTypenameOfProp(i), EditorStyles.boldLabel);
                if (GUI.Button(removeButtonRect, "Remove")) toRemove = i;

                //Property data
                try
                {
                    SerializedProperty prop = propArr.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                    EditorGUI.PropertyField(buildRect(EditorGUI.GetPropertyHeight(prop)), prop);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
        
        if (toRemove.HasValue)
        {
            //string typename = _getTypenameOfProp(toRemove.Value);
            //Undo.RecordObject(item, "Remove Property: " + typename);
            propArr.DeleteArrayElementAtIndex(toRemove.Value);
            //serializedObject.ApplyModifiedPropertiesWithoutUndo();
            //Undo.FlushUndoRecordObjects();
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        void buildRect(float _height) { height += _height; }

        SerializedProperty propArr = property.FindPropertyRelative("contents");
        
        //Show name label
        buildRect(EditorGUIUtility.singleLineHeight);

        //Show add menu
        buildRect(EditorGUIUtility.singleLineHeight);

        //Properties
        using (new EditorGUI.IndentLevelScope())
        {
            for (int i = 0; i < propArr.arraySize; ++i)
            {
                buildRect(EditorGUIUtility.singleLineHeight); //Separator

                buildRect(EditorGUIUtility.singleLineHeight); //headerRect
                
                //Property data
                try
                {
                    SerializedProperty prop = propArr.GetArrayElementAtIndex(i).FindPropertyRelative("value");
                    buildRect(EditorGUI.GetPropertyHeight(prop));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        return height;
    }

    List<Type> GetPropertyTypes()
    {
        if (__propertyTypeCache == null)
        {
            __propertyTypeCache = new List<Type>();
            __propertyTypeCache.AddRange(TypeCache.GetTypesDerivedFrom(fieldInfo.FieldType.GenericTypeArguments[0]).Where(t => !t.IsAbstract));
        }

        return __propertyTypeCache;
    }
    List<Type> __propertyTypeCache;
}
