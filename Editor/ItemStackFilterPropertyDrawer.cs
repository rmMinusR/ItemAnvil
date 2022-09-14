using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemStackFilter))]
public sealed class ItemStackFilterPropertyDrawer : PropertyDrawer
{
    private float TypeSelectorDropdownHeight => EditorGUIUtility.singleLineHeight;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //Show type menu
        PolymorphicTypeUtil.ExtractType()

        Rect typeSelectorRect = Rect.MinMaxRect(position.xMin, position.xMax, position.yMin, position.yMin + TypeSelectorDropdownHeight);
        List<Type> types = GetValidTypes();
        string[] names = types.Select(t => t.Name).ToArray();
        
        int toAdd = EditorGUILayout.Popup(0, names);
        if (toAdd != 0)
        {
            //TODO make it play nice with undo
            //Undo.RecordObject(item, "Add Property: "+names[toAdd]);
            item.AddProperty((ItemProperty)addable[toAdd].GetConstructor(new Type[] { }).Invoke(new object[] { }));
            //Undo.FlushUndoRecordObjects();
        }

        Rect contentRect      = Rect.MinMaxRect(position.xMin, position.xMax, position.yMin + TypeSelectorDropdownHeight, position.yMax);
        base.OnGUI(contentRect, property, label);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return base.GetPropertyHeight(property, label) + TypeSelectorDropdownHeight;
    }

    static List<Type> GetValidTypes()
    {
        if (__TypeCache == null)
        {
            __TypeCache = new List<Type>();
            __TypeCache.AddRange(TypeCache.GetTypesDerivedFrom<ItemStackFilter>().Where(t => !t.IsAbstract));
        }

        return __TypeCache;
    }
    static List<Type> __TypeCache;
}