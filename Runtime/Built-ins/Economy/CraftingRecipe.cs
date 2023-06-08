using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class CraftingRecipe : ScriptableObject, ICraftingRecipe
{
    public virtual bool TryExchange(Inventory crafter, int multiplier)
    {
        if(IsValid(crafter, multiplier))
        {
            DoExchange(crafter, multiplier);
            return true;
        }

        return false;
    }

    public abstract bool IsValid(Inventory crafter, int multiplier);

    protected abstract bool DoExchange(Inventory crafter, int multiplier);
}


public interface ICraftingRecipe
{
    public bool TryExchange(Inventory crafter, int multiplier);
    public bool IsValid(Inventory crafter, int multiplier);
}
