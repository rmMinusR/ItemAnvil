using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(TypeSwitcherAttribute))]
public class TypeSwitcherDrawer : PropertyDrawer
{
    //Parallel arrays bad, but it's better for performance
    List<Type> types;
    GUIContent[] displayStrings = null;
    void PopulateDropdownContents()
    {
        types = new List<Type>();
        types.AddRange(GetSubclasses(fieldInfo.FieldType));
        List<string> strings = types.Select(t => t.Name).ToList();
        strings.Insert(0, "(null)");
        displayStrings = strings.Select(i => new GUIContent(i)).ToArray();
    }
    
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.managedReferenceFullTypename.Length > 0) //property value != null
        {
            return EditorGUIUtility.singleLineHeight + EditorGUI.GetPropertyHeight(property);
        }
        else return EditorGUIUtility.singleLineHeight;
    }

    public override void OnGUI(Rect rootPosition, SerializedProperty property, GUIContent label)
    {
        Rect dropdownRect = Rect.MinMaxRect(rootPosition.xMin, rootPosition.yMin, rootPosition.xMax, rootPosition.yMin+EditorGUIUtility.singleLineHeight);
        Rect contentRect = rootPosition;

        if (displayStrings == null) PopulateDropdownContents();
        
        //Awful magic numbers, but -1 is null, making type list 0-indexed
        int currentTypeIndex = types.FindIndex(t => t.Assembly.GetName().Name+" "+t.Name == property.managedReferenceFullTypename);
        int targetTypeIndex = EditorGUI.Popup(dropdownRect, label, currentTypeIndex+1, displayStrings) - 1;

        EditorGUI.PropertyField(contentRect, property, GUIContent.none, true);

        //Apply type switching
        //Note that this often causes error messages saying that SerializedProperty has disappeared. These can safely be ignored.
        if (targetTypeIndex != currentTypeIndex)
        {
            if (targetTypeIndex != -1)
            {
                Type targetType = types[targetTypeIndex];
                
                SerialRepr snapshot = null;
                if (fieldInfo.GetCustomAttribute<TypeSwitcherAttribute>().keepData) snapshot = SerialRepr.Extract(property);

                object instance = Instantiate(targetType);
                property.managedReferenceValue = instance;

                snapshot?.Apply(property);
            }
            else
            {
                property.managedReferenceValue = null;
            }
        }
    }

    #region SerialRepr object tree

    private abstract class SerialRepr
    {
        public static SerialRepr Extract(SerializedProperty src)
        {
            //Debug.Log(src.propertyPath);
            if (src.hasChildren)
            {
                if (src.isArray) return new SerialRepr_Array(src);
                else return new SerialRepr_Composite(src);
            }
            else return new SerialRepr_RawValue(src);
        }

        public abstract void Apply(SerializedProperty dst);
    }

    private sealed class SerialRepr_RawValue : SerialRepr
    {
        SerializedPropertyType type;
        PropertyInfo accessor;
        object value;

        public SerialRepr_RawValue(SerializedProperty src)
        {
            type = src.propertyType;

            string accessorName = src.propertyType.ToString()+"Value";
            if (type == SerializedPropertyType.Integer) accessorName = nameof(SerializedProperty.intValue);
            accessor = typeof(SerializedProperty).GetProperties().FirstOrDefault(i => i.Name.ToLower() == accessorName.ToLower());
            
            value = accessor.GetValue(src);
        }

        public override void Apply(SerializedProperty dst)
        {
            if (dst.propertyType == type)
            {
                accessor.SetValue(dst, value);
            }
            else Debug.LogWarning($"Trivial type mismatch: {type} => {dst.propertyType}");
        }
    }

    private sealed class SerialRepr_Array : SerialRepr
    {
        SerialRepr[] values;

        public SerialRepr_Array(SerializedProperty src)
        {
            values = new SerialRepr[src.arraySize];
            for (int i = 0; i < src.arraySize; ++i) values[i] = SerialRepr.Extract(src.GetArrayElementAtIndex(i));
        }

        public override void Apply(SerializedProperty dst)
        {
            //Trim excess
            while (dst.arraySize > values.Length) dst.DeleteArrayElementAtIndex(dst.arraySize-1);

            //Expand if insufficient
            while (dst.arraySize < values.Length) dst.InsertArrayElementAtIndex(dst.arraySize);

            //Write values
            for (int i = 0; i < values.Length; ++i) values[i].Apply(dst.GetArrayElementAtIndex(i));
        }
    }

    private sealed class SerialRepr_Composite : SerialRepr
    {
        Dictionary<string, SerialRepr> contents;

        public SerialRepr_Composite(SerializedProperty src)
        {
            contents = new Dictionary<string, SerialRepr>();
            foreach (SerializedProperty child in src.Copy())
            {
                bool isImmediateChild = child.propertyPath.Count(i => i == '.') == src.propertyPath.Count(i => i == '.')+1;
                if (isImmediateChild && child.propertyType != SerializedPropertyType.ArraySize) contents[child.name] = SerialRepr.Extract(child);
            }
        }

        public override void Apply(SerializedProperty dst)
        {
            if (dst.hasChildren && !dst.isArray)
            {
                foreach (KeyValuePair<string, SerialRepr> child in contents)
                {
                    SerializedProperty dstProp = dst.FindPropertyRelative(child.Key);
                    if (dstProp != null) child.Value.Apply(dstProp);
                }
            }
            else Debug.LogWarning($"Composite type mismatch: {dst.propertyType}");
        }
    }

    #endregion SerialRepr object tree
    
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
