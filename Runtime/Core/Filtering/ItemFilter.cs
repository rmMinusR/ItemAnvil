using System;
using UnityEngine;

[Serializable]
public abstract class ItemFilter
{
    public abstract bool Matches(ReadOnlyItemStack itemStack);

    public abstract ItemFilter Clone();
}
