using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeSwitcherAttribute))]
public class TypeSwitcherDrawer : DecoratorDrawer
{
    const float DROPDOWN_WIDTH = 100;

    //TODO parallel arrays bad
    List<Type> types;
    string[] displayStrings = null;

    TypeSwitcherAttribute attr => (TypeSwitcherAttribute)attribute;

    public override void OnGUI(Rect rootPosition)
    {
        Rect targetLineRect = Rect.MinMaxRect(rootPosition.xMin, rootPosition.yMax, rootPosition.xMax, rootPosition.yMax+EditorGUIUtility.singleLineHeight+EditorGUIUtility.standardVerticalSpacing);

        Rect dropdownRect = Rect.MinMaxRect(targetLineRect.xMax-DROPDOWN_WIDTH, targetLineRect.yMin, targetLineRect.xMax, targetLineRect.yMax);

        //Flyweight populating list
        if (displayStrings == null)
        {
            types = new List<Type>();
            types.Add(null); //Dummy, placeholder for "Switch to..."
            //types.AddRange(GetSubclasses(attr.type));
            displayStrings = types.Select(t => t?.Name ?? "Switch to...").ToArray();
        }

        int toSwitch = EditorGUI.Popup(dropdownRect, 0, displayStrings);
        if (toSwitch != 0)
        {
            Debug.Log("Switching to "+types[toSwitch]);
        }
    }

    public override float GetHeight()
    {
        return 0; //EditorGUIUtility.singleLineHeight;
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
