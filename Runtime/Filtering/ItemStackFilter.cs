using System;
using UnityEngine;

[Serializable]
public abstract class ItemStackFilter
{
	public abstract bool Matches(ItemStack itemStack);
}
