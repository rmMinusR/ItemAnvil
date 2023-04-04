using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "New Crafting Recipe", menuName = "Crafting Recipe")]
public class CraftingRecipe : ScriptableObject
{
    [SerializeField] private ItemStackFilter[] inputs;
    [SerializeField] private ItemStack[] outputs;

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public CraftingRecipe(IEnumerable<ItemStackFilter> inputs, IEnumerable<ItemStack> outputs)
    {
        this.inputs  = inputs .Select(s => s.Clone()).ToArray();
        this.outputs = outputs.Select(s => s.Clone()).ToArray();
    }

    public bool TryExchange(Inventory crafter, int multiplier)
    {
        if(IsValid(crafter, multiplier))
        {
            DoExchange(crafter, multiplier);
            return true;
        }

        return false;
    }

    public virtual bool IsValid(Inventory crafter, int multiplier)
    {
        //FIXME: If inputs has duplicate type, this incorrectly return true
        return inputs.All(i => crafter.Count(i.typeFilter) >= i.quantity * multiplier);
    }

    protected virtual bool DoExchange(Inventory crafter, int multiplier)
    {
        bool stillValid = true;
#if UNITY_EDITOR
        Debug.Assert(stillValid = IsValid(crafter, multiplier)); //Just to be sure...
#endif

        //FIXME: If inputs has duplicate type, breaks rollback-on-fail contract (exception safety level 2) because items will still be removed

        //Remove items
        foreach (ItemStackFilter i in inputs) stillValid &= crafter.TryRemove(i.typeFilter, i.quantity*multiplier);

        Debug.Assert(stillValid);

        //Add items provided recipe inputs were valid
        if (stillValid)
        {
            foreach (ItemStack i in outputs)
            {
                ItemStack stack = i.Clone();
                stack.quantity *= multiplier;
                crafter.AddItem(stack);
            }
        }

        return stillValid;
    }
}