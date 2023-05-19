using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

[Serializable]
public sealed class PropertyBag<TBase> : IEnumerable<TBase> where TBase : class // Exclude structs and POD, since they don't have inheritance
{
    [Serializable]
    private class Container // Serialization helper
    {
        [SerializeReference] public TBase obj;
    }
    [SerializeField] private List<Container> contents;

    public T Add<T>() where T : TBase, new()
    {
        if (!contents.Any(i => i.obj is T))
        {
            ConstructorInfo ctor = typeof(T).GetConstructor(new Type[] { });
            T obj = (T)ctor.Invoke(new object[] { });
            contents.Add(new Container() { obj = obj });
            return obj;
        }
        else throw new InvalidOperationException($"Tried to add a {typeof(T).Name}, but it already exists!");
    }

    public T Get<T>() where T : TBase
    {
        foreach (Container i in contents) if (i.obj is T val) return val;
        return default(T);
    }

    public bool TryGet<T>(out T val) where T : TBase
    {
        foreach (Container i in contents) if (i.obj is T) { val = (T)i.obj; return true; }

        val = default(T);
        return false;
    }

    public bool Remove<T>() where T : TBase
    {
        return contents.RemoveAll(i => i.obj is T) != 0;
    }

    IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator(); // Defer to main impl
    public IEnumerator<TBase> GetEnumerator()
    {
        foreach (Container i in contents) yield return i.obj;
    }
}