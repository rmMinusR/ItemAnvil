using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

/// <summary>
/// A unified way to store properties. In general, only one property of each type may exist per bag. Used in Items and ItemStacks to store ItemProperties and ItemInstanceProperties.
/// </summary>
/// <typeparam name="TBase">Base class of all properties</typeparam>
[Serializable]
public sealed class PropertyBag<TBase> : ISet<TBase>, ICloneable
                          where TBase : class, ICloneable // Exclude structs and POD, since they don't have inheritance. ICloneable to fix inappropriate ref sharing
{
    [Serializable]
    private class Container : ISerializationCallbackReceiver // Serialization helper
    {
        [SerializeReference] public TBase value;

        public void OnBeforeSerialize()
        {
#if UNITY_EDITOR
            if (value != null) value = (TBase) value.Clone(); // Prevent shared references in a list or when copying. Stupid, but it works.
#endif
        }

        public void OnAfterDeserialize()
        {
        }
    }
    [SerializeField] private List<Container> contents = new List<Container>();

    public int Count => contents.Count;
    public bool IsReadOnly => false;

    public T Add<T>() where T : TBase, new()
    {
        if (!contents.Any(i => i.value is T))
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { });
            T obj = (T)ctor.Invoke(new object[] { });
            contents.Add(new Container() { value = obj });
            return obj;
        }
        else throw new InvalidOperationException($"Tried to add a {typeof(T).Name}, but one already exists!");
    }
    
    public bool Add(TBase toAdd)
    {
        if (!contents.Any(i => i.value.GetType() == toAdd.GetType()))
        {
            contents.Add(new Container() { value = toAdd });
            return true;
        }
        else
        {
            Debug.LogWarning($"Tried to add a {toAdd.GetType().Name}, but one already exists!");
            return false;
        }
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

    /////////////// ISet impl ///////////////

    public void ExceptWith   (IEnumerable<TBase> other) => contents.RemoveAll(c =>  other.Contains(c.value));
    public void IntersectWith(IEnumerable<TBase> other) => contents.RemoveAll(c => !other.Contains(c.value));

    public bool IsProperSubsetOf  (IEnumerable<TBase> other) => IsSubsetOf  (other) && Count != other.Count();
    public bool IsProperSupersetOf(IEnumerable<TBase> other) => IsSupersetOf(other) && Count != other.Count();

    public bool IsSubsetOf  (IEnumerable<TBase> other) => this.All(i => other.Contains(i));
    public bool IsSupersetOf(IEnumerable<TBase> other) => other.All(i => this.Contains(i));

    public bool Overlaps(IEnumerable<TBase> other)
    {
        foreach (TBase i in other) if (this .Contains(i)) return true;
        foreach (TBase i in this ) if (other.Contains(i)) return true;
        return false;
    }

    public bool SetEquals(IEnumerable<TBase> other) => IsSubsetOf(other) && IsSupersetOf(other);

    public void SymmetricExceptWith(IEnumerable<TBase> other)
    {
        throw new NotImplementedException();
    }

    public void UnionWith(IEnumerable<TBase> other)
    {
        throw new NotImplementedException();
    }

    public bool Contains(TBase item) => contents.Any(i => i.value == item);
    public void Clear() => contents.Clear();

    public void CopyTo(TBase[] array, int arrayIndex)
    {
        if (array.Length < contents.Count+arrayIndex) throw new IndexOutOfRangeException("Not enough space in destination array");

        for (int i = 0; i < contents.Count; ++i)
        {
            array[i+arrayIndex] = contents[i].value;
        }
    }

    void ICollection<TBase>.Add(TBase item)
    {
        this.Add(item); // Defer to main impl
    }
}