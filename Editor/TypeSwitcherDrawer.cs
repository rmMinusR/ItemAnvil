using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeSwitcherAttribute))]
public class TypeSwitcherDrawer : PropertyDrawer
{
    const float DROPDOWN_WIDTH = 100;

    //TODO parallel arrays bad
    List<Type> types;
    GUIContent[] displayStrings = null;

    public override void OnGUI(Rect rootPosition, SerializedProperty property, GUIContent label)
    {
        //Rect targetLineRect = Rect.MinMaxRect(rootPosition.xMin, rootPosition.yMax, rootPosition.xMax, rootPosition.yMax+EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing);
        //Rect dropdownRect = Rect.MinMaxRect(targetLineRect.xMax-DROPDOWN_WIDTH, targetLineRect.yMin, targetLineRect.xMax, targetLineRect.yMax);
        Rect dropdownRect = Rect.MinMaxRect(rootPosition.xMin, rootPosition.yMin, rootPosition.xMax, rootPosition.yMin+EditorGUIUtility.singleLineHeight);
        Rect contentRect = rootPosition;//Rect.MinMaxRect(rootPosition.xMin, dropdownRect.yMax, rootPosition.xMax, rootPosition.yMax);

        //Flyweight populating list
        if (displayStrings == null)
        {
            types = new List<Type>();
            types.AddRange(GetSubclasses(fieldInfo.FieldType));
            List<string> strings = types.Select(t => t.Name).ToList();
            strings.Insert(0, "(null)");
            displayStrings = strings.Select(i => new GUIContent(i)).ToArray();
        }

        //Awful magic numbers, but -1 is null, making type list 0-indexed
        int currentTypeIndex = types.FindIndex(t => t.Assembly.GetName().Name+" "+t.Name == property.managedReferenceFullTypename);
        int targetTypeIndex = EditorGUI.Popup(dropdownRect, label, currentTypeIndex+1, displayStrings) - 1;
        if (targetTypeIndex != currentTypeIndex)
        {
            if (targetTypeIndex != -1)
            {
                Type targetType = types[targetTypeIndex];
                //Debug.Log("Switching to "+targetType);
                property.managedReferenceValue = Instantiate(targetType);
            }
            else
            {
                //Debug.Log("Switching to null");
                property.managedReferenceValue = null;
            }
        }

        EditorGUI.PropertyField(contentRect, property, true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceFullTypename.Length > 0) //property value != null
        {
            return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(property);
        }
        else return EditorGUIUtility.singleLineHeight;
    }

    private static object Instantiate(Type type)
    {
        IEnumerable<ConstructorInfo> matches = type.GetConstructors(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Where(i => i.GetParameters().Length == 0);
        if (matches.Any())
        {
            return matches.First().Invoke(new object[] { });
        }
        else throw new ArgumentException($"{type.Name} has no default constructor and cannot be instantiated!");
    }

    private static IReadOnlyList<Type> GetSubclasses(Type @base)
    {
        //Try to retrieve from cache
        List<Type> val;
        if (__typeCache.TryGetValue(@base, out val)) return val;

        //Populate cache
        val = new List<Type>();
        val.AddRange(TypeCache.GetTypesDerivedFrom(@base));
        val.RemoveAll(i => i.IsAbstract); //Cannot instantiate abstracts
        if (!@base.IsGenericType) val.RemoveAll(i => i.IsGenericType); //Cannot infer type parameters for generics. TODO does TypeCache's return values need parameterization?
        __typeCache[@base] = val;
        return val;
    }
    private static Dictionary<Type, List<Type>> __typeCache = new Dictionary<Type, List<Type>>();
}
