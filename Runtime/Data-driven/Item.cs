using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Data container for how items should look and act. One of these must exist per item type.
/// MUST be created through Unity and passed in by Inspector, can not by created or retrieved through code.
/// </summary>
/// <seealso cref="ItemStack"/>
/// <author>Robert Christensen</author>

[CreateAssetMenu(fileName = "New Item Type", menuName = "Item Type")]
public sealed class Item : ScriptableObject
{
    [Header("Display settings")]
    public string displayName = "Item";
    public Sprite displayIcon;
    [TextArea] public string displayTooltip;
    public bool showInMainInventory = true;

    [Space]
    
    [SerializeReference] private ItemProperty[] properties;

    public T GetProperty<T>() where T : ItemProperty
    {
        foreach (ItemProperty i in properties)
        {
            if (i is T t) return t;
        }
        return null;
    }

    public IEnumerable<T> GetProperties<T>() where T : ItemProperty
    {
        return properties.Select(i => i as T).Where(i => i != null);
    }

    public bool TryGetProperty<T>(out T val) where T : ItemProperty
    {
        foreach (ItemProperty i in properties)
        {
            if (i is T t)
            {
                val = t;
                return true;
            }
        }

        val = null;
        return false;
    }
}
