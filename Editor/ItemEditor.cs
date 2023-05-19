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
