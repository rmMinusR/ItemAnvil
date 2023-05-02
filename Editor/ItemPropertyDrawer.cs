using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemProperty))]
[CustomPropertyDrawer(typeof(ItemInstanceProperty))] //TODO make its own thing?
public class ItemPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float y = 0;
        Type type = ExtractType(property);
        foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null || f.GetCustomAttribute<SerializeReference>() != null)
            {
                SerializedProperty i = property.FindPropertyRelative(f.Name);

                Rect drawRect = Rect.MinMaxRect(position.xMin, position.yMin + y, position.xMax, position.yMin+y+EditorGUI.GetPropertyHeight(i));
                EditorGUI.PropertyField(drawRect, i);
                y += EditorGUI.GetPropertyHeight(i);
            }
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        Type type = ExtractType(property);
        foreach (FieldInfo f in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (f.IsPublic || f.GetCustomAttribute<SerializeField>() != null || f.GetCustomAttribute<SerializeReference>() != null)
            {
                SerializedProperty i = property.FindPropertyRelative(f.Name);
                height += EditorGUI.GetPropertyHeight(i);
            }
        }
        
        return height;
    }

    private Type ExtractType(SerializedProperty property)
    {
        return ExtractFieldValue(property)?.GetType();
    }

    private object ExtractFieldValue(SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;
        string[] pathFragments = property.propertyPath.Replace(".Array.data[", ".[").Split('.');
        //Debug.Log("Full path: "+string.Join(".", pathFragments));
        foreach (string f in pathFragments)
        {
            if (f[0] == '[')
            {
                //Extract from array or list
                int index = int.Parse(f.Substring(1, f.Length-2));
                //Debug.Log("Index: "+target.GetType().Name+"["+index+"]");
                     if (target is IList        l) target =  l[index];
                else if (target is object[]    na) target = na[index];
                else if (target is Array       oa) target = oa.GetValue(index);
                else Debug.LogError("Unhandled: "+target.GetType());
            }
            else
            {
                //Extract from field
                FieldInfo fi = target.GetType().GetField(f, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                /*
                if (fi == null)
                {
                    Debug.LogError("Failed to locate field: "+f);
                    Debug.LogError("Valid fields for "+target.GetType().Name+": "
                                    +string.Join(", ", target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                                                                        .Where(i => i.GetCustomAttribute<SerializeField>() != null || i.GetCustomAttribute<SerializeReference>() != null)
                                                                        .Select(i => i.Name)));
                }
                // */
                if (!fi.FieldType.IsValueType)
                {
                    //Debug.Log("Ref field: "+target.GetType().Name+"."+f);
                    target = fi.GetValue(target);
                }
                else
                {
                    //Debug.Log("Val field: "+target.GetType().Name+"."+f);
                    target = fi.GetValueDirect(__makeref(target));
                }
            }
        }
        return target;
    }

    /*
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        Debug.Log(property.propertyPath + " (" + property.type + "): " + property.CountInProperty());

        float y = 0;
        SerializedProperty it = property.Copy();
        try
        {
            if (!it.NextVisible(true)) return; //Enter children
        }
        catch(InvalidOperationException e)
        {
            //Pain
            //Debug.LogException(e);
            return;
        }
        SerializedProperty end = it.GetEndProperty();
        while (it != end)
        {
            Rect drawRect = Rect.MinMaxRect(position.xMin, position.yMin + y, position.xMax, position.yMin+y+EditorGUI.GetPropertyHeight(it));
            EditorGUI.PropertyField(drawRect, it);
            y += EditorGUI.GetPropertyHeight(it);
            if (!it.NextVisible(false)) break;
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float height = 0;
        SerializedProperty it = property.Copy();
        if (!it.NextVisible(true)) return 0; //Enter children
        //while (it != it.GetEndProperty())
        SerializedProperty end = it.GetEndProperty();
        while (it != end)
        {
            height += EditorGUI.GetPropertyHeight(it);
            if (!it.NextVisible(false)) break;
        }
        
        return height;
    }
    // */
}
