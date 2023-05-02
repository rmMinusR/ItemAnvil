using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item))]
public class ItemEditor : Editor
{
    public override void OnInspectorGUI()
    {
        bool refreshIcon = false;
        Item item = (Item) target;

        Sprite prevDisplayIcon = item.displayIcon;

        DrawDefaultInspector();
        
        refreshIcon |= (prevDisplayIcon != item.displayIcon);

        EditorGUILayout.Separator();
        EditorGUILayout.LabelField("Properties", EditorStyles.boldLabel);

        serializedObject.ApplyModifiedProperties();

        //Show add menu
        List<Type> addable = new List<Type>();
        addable.Add(null);
        addable.AddRange(GetPropertyTypes().Where(t => !item.TryGetProperty(t, out _)));
        string[] names = addable.Select(t => t?.Name ?? "Add...").ToArray();
        int toAdd = EditorGUILayout.Popup(0, names);
        if (toAdd != 0)
        {
            //TODO make it play nice with undo
            //Undo.RecordObject(item, "Add Property: "+names[toAdd]);
            item.AddProperty((ItemProperty)addable[toAdd].GetConstructor(new Type[] { }).Invoke(new object[] { }));
            //Undo.FlushUndoRecordObjects();
        }

        serializedObject.Update();

        //Show existing properties

        SerializedProperty propArr = serializedObject.FindProperty("properties");
        string _getTypenameOfProp(int id)
        {
            string typename = propArr.GetArrayElementAtIndex(id).FindPropertyRelative("value").managedReferenceFullTypename;
            int splitInd = Math.Max(typename.LastIndexOf('.'), typename.LastIndexOf(' ')) + 1;
            typename = typename.Substring(splitInd);
            return typename;
        }

        int? toRemove = null;
        for(int i = 0; i < propArr.arraySize; ++i)
        {
            EditorGUI.indentLevel++;
            EditorGUILayout.Separator();

            //Property header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(_getTypenameOfProp(i), EditorStyles.boldLabel);
            if (GUILayout.Button("Remove")) toRemove = i;
            EditorGUILayout.EndHorizontal();

            //Property data
            try
            {
                EditorGUILayout.PropertyField(propArr.GetArrayElementAtIndex(i).FindPropertyRelative("value"));
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            EditorGUI.indentLevel--;
        }

        if (toRemove.HasValue)
        {
            //string typename = _getTypenameOfProp(toRemove.Value);
            //Undo.RecordObject(item, "Remove Property: " + typename);
            propArr.DeleteArrayElementAtIndex(toRemove.Value);
            //serializedObject.ApplyModifiedPropertiesWithoutUndo();
            //Undo.FlushUndoRecordObjects();
        }

        serializedObject.ApplyModifiedPropertiesWithoutUndo();

        if(refreshIcon)
        {
            //Update asset icon
            //FIXME inconsistent, and overkill in terms of what we're refreshing
            //TODO use instead: https://docs.unity.cn/2021.2/Documentation/ScriptReference/EditorGUIUtility.SetIconForObject.html
            EditorUtility.SetDirty(target);
            string path = AssetDatabase.GetAssetPath(target);
            AssetDatabase.ForceReserializeAssets(new string[] { path }, ForceReserializeAssetsOptions.ReserializeAssetsAndMetadata);
            AssetImporter.GetAtPath(path).SaveAndReimport();
            AssetPreview.GetAssetPreview(target);
        }
    }

    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
    {
        Item item = (Item) target;

        if (item.displayIcon != null) return AssetPreview.GetAssetPreview(item.displayIcon);
        else return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }

    static List<Type> GetPropertyTypes()
    {
        if (__propertyTypeCache == null)
        {
            __propertyTypeCache = new List<Type>();
            __propertyTypeCache.AddRange(TypeCache.GetTypesDerivedFrom(typeof(ItemProperty)).Where(t => !t.IsAbstract));
        }

        return __propertyTypeCache;
    }
    static List<Type> __propertyTypeCache;
}
