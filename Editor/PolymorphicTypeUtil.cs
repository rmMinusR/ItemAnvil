using System;
using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

internal static class PolymorphicTypeUtil
{
    internal static Type ExtractType(SerializedProperty property)
    {
        object target = property.serializedObject.targetObject;
        string[] pathFragments = property.propertyPath.Replace(".Array.data[", ".[").Split('.');
        //Debug.Log("Full path: "+string.Join(".", pathFragments));
        FieldInfo fi = null;
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
                fi = target.GetType().GetField(f, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
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
        return fi.FieldType; //FIXME might not be accurate?
    }

    internal static object ExtractFieldValue(SerializedProperty property)
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
}