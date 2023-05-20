using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A unified way to store properties. In general, only one property of each type may exist per bag. Used in Items and ItemStacks.
/// </summary>
/// <typeparam name="TBase">Base class of all properties</typeparam>
[Serializable]
public sealed class PropertyBag<TBase> : IEnumerable<TBase>, ICloneable //Should prob be an ISet, but that's a lot of work. TODO.
    where TBase : class, ICloneable // Exclude structs and POD, since they don't have inheritance. ICloneable to fix inappropriate ref sharing
{
    [Serializable]
    private class Container : ISerializationCallbackReceiver // Serialization helper
    {
        [SerializeReference] public TBase value;

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            value = (TBase) value.Clone(); // Prevent shared references in a list or when copying. Stupid, but it works.
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
    [SerializeField] private List<Container> contents;

    public T Add<T>() where T : TBase, new()
    {
        if (!contents.Any(i => i.value is T))
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { });
            T obj = (T)ctor.Invoke(new object[] { });
            contents.Add(new Container() { value = obj });
            return obj;
        }
        else throw new InvalidOperationException($"Tried to add a {typeof(T).Name}, but it already exists!");
    }
    
    public void Add<T>(T toAdd) where T : TBase
    {
        if (!contents.Any(i => i.value is T))
        {
            contents.Add(new Container() { value = toAdd });
        }
        else throw new InvalidOperationException($"Tried to add a {typeof(T).Name}, but it already exists!");
    }

    public T Get<T>() where T : TBase
    {
        foreach (Container i in contents) if (i.value is T val) return val;
        return default;
    }

    static bool TypeMatches(Type sub, Type super) => sub == super || sub.IsSubclassOf(super);

    public TBase Get(Type type)
    {
        foreach (Container i in contents) if (TypeMatches(i.value.GetType(), type)) return i.value;
        return default;
    }

    public bool TryGet<T>(out T val) where T : TBase
    {
        foreach (Container i in contents)
        {
            if (i.value is T _val)
            {
                val = _val;
                return true;
            }
        }

        val = default;
        return false;
    }

    public bool Remove<T>() where T : TBase
    {
        return contents.RemoveAll(i => i.value is T) != 0;
    }

    public bool Remove(TBase toRemove)
    {
        return contents.RemoveAll(i => i.value == toRemove) != 0;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator(); // Defer to main impl
    public IEnumerator<TBase> GetEnumerator()
    {
        foreach (Container i in contents) yield return i.value;
    }

    public object Clone()
    {
        return MemberwiseClone(); //FIXME deep clone, this will share property instances!
    }
}