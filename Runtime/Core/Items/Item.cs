using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data structure describing how items should look and act. One of these must exist per item type.
/// MUST be created through Unity and passed in by Inspector, can not by created or retrieved through code.
/// </summary>
/// <seealso cref="ItemStack"/>
/// <seealso cref="ItemProperty"/>

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public sealed class Item : ScriptableObject
{
    [Header("Display settings")]
    public string displayName = "Item";
    public Sprite displayIcon;
    [TextArea] public string displayTooltip;
    public bool showInMainInventory = true;

    [HideInInspector] //Skip in default draw pass, we'll render this manually after
    [SerializeField] private List<ItemPropertyWrapper> properties = new List<ItemPropertyWrapper>();

    #region Helpers for dealing with properties

    [Serializable]
    internal struct ItemPropertyWrapper
    {
        [SerializeReference] public ItemProperty value;
    }

    public T GetProperty<T>() where T : ItemProperty
    {
        foreach (ItemPropertyWrapper i in properties)
        {
            if (i.value is T t) return t;
        }
        return null;
    }

    private bool TypeMatches(Type sub, Type super) => sub == super || sub.IsSubclassOf(super);

    public ItemProperty GetProperty(Type type)
    {
        foreach (ItemPropertyWrapper i in properties)
        {
            if (TypeMatches(i.value.GetType(), type)) return i.value;
        }
        return null;
    }

    public IEnumerable<T> GetProperties<T>() where T : ItemProperty
    {
        foreach (ItemPropertyWrapper w in properties)
        {
            if (w.value is T t) yield return t;
        }
    }

    public IEnumerable<ItemProperty> GetProperties(Type type)
    {
        foreach (ItemPropertyWrapper w in properties)
        {
            if (TypeMatches(w.value.GetType(), type)) yield return w.value;
        }
    }

    public bool TryGetProperty<T>(out T val) where T : ItemProperty
    {
        foreach (ItemPropertyWrapper i in properties)
        {
            if (i.value is T t)
            {
                val = t;
                return true;
            }
        }

        val = null;
        return false;
    }

    public bool TryGetProperty(Type type, out ItemProperty val)
    {
        foreach (ItemPropertyWrapper i in properties)
        {
            if (i.value != null && TypeMatches(i.value.GetType(), type))
            {
                val = i.value;
                return true;
            }
        }

        val = null;
        return false;
    }

    public bool AddProperty(ItemProperty val, bool overwrite = false)
    {
        bool alreadyExists = TryGetProperty(val.GetType(), out ItemProperty p);

        if (alreadyExists && overwrite) RemoveProperty(p);

        if (!alreadyExists || overwrite)
        {
            properties.Add(new ItemPropertyWrapper { value = val });
            return true;
        }
        else return false;
    }

    public void RemoveProperty(ItemProperty val)
    {
        properties.RemoveAll(i => i.value == val);
    }

    #endregion
}
