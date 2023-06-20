using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Item Anvil/Simple Crafting Recipe")]
public sealed class SimpleCraftingRecipe : CraftingRecipe
{
    [Serializable]
    private class Ingredient // Almost an ItemStack, but no properties
    {
        public Item itemType;
        [Min(1)] public int quantity;

        public Ingredient(ItemStack src)
        {
            itemType = src.itemType;
            quantity = src.quantity;
        }
    }
    
    [SerializeField] private List<Ingredient> inputs;
    [SerializeField] private List<ItemStack> outputs;

    //IEnumerable here is usually an ItemStack[] or List<ItemStack>
    public SimpleCraftingRecipe(IEnumerable<ItemStack> inputs, IEnumerable<ItemStack> outputs)
    {
        //Read inputs and de-duplicate types
        this.inputs = new List<Ingredient>();
        foreach (ItemStack s in inputs)
        {
            int matchIndex = this.inputs.FindIndex(i => i.itemType == s.itemType);
            if (matchIndex != -1) this.inputs[matchIndex] = new Ingredient(s);
            else this.inputs.Add(new Ingredient(s));
        }

        //Read outputs. No need to de-duplicate.
        this.outputs = outputs.Select(i => i.Clone()).ToList();
    }

    public override bool IsValid(Inventory crafter, int multiplier)
    {
        //Would potentially error if we had duplicate types in our inputs
        return inputs.All(i => crafter.Count(i.itemType) >= i.quantity * multiplier);
    }

    protected override bool DoExchange(Inventory crafter, int multiplier)
    {
        bool stillValid = true;
#if UNITY_EDITOR
        Debug.Assert(stillValid = IsValid(crafter, multiplier)); //Just to be sure...
#endif

        List<ItemStack> inputs_instanced = new List<ItemStack>();

        //Attempt to remove items
        try
        {
            foreach (Ingredient i in inputs)
            {
                IEnumerable<ItemStack> inputs_removed = crafter.TryRemove(i.itemType, i.quantity * multiplier);
                inputs_instanced.AddRange(inputs_removed);
                stillValid &= inputs_removed.Count() > 0;
            }

            Debug.Assert(stillValid);
        }
        catch (Exception e)
        {
            //Something went wrong! Refund items.
            foreach (ItemStack s in inputs_instanced) crafter.AddItem(s);

            throw new Exception("Something went wrong while processing SimpleCraftingRecipe!", e);
        }

        //Add items provided recipe inputs were valid
        if (stillValid)
        {
            foreach (ItemStack i in outputs)
            {
                ItemStack stack = i.Clone();
                stack.quantity *= multiplier; //FIXME is this safe?
                crafter.AddItem(stack);
            }
        }

        return stillValid;
    }

    public void MultiplyInPlace(int scale)
    {
        foreach (Ingredient i in inputs) i.quantity *= scale;
        foreach (ItemStack s in outputs) s.quantity *= scale;
    }
}