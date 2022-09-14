using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ItemProperty))]
public class ItemPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        float y = 0;
        Type type = PolymorphicTypeUtil.ExtractType(property);
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
        Type type = PolymorphicTypeUtil.ExtractType(property);
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
}
