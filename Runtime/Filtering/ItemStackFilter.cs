using System;
using UnityEngine;

[Serializable]
public abstract class ItemFilter
{
    public abstract bool Matches(ItemStack itemStack);

    public abstract ItemFilter Clone();
}
