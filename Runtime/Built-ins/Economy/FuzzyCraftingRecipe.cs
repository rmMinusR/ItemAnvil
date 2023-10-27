using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    [CreateAssetMenu(menuName = "Item Anvil/Fuzzy Crafting Recipe")]
    public sealed class FuzzyCraftingRecipe : CraftingRecipe
    {
        [Serializable]
        private class Ingredient // Almost an ItemStack, but fuzzy
        {
            [SerializeReference] [TypeSwitcher] public ItemFilter filter;
            [Min(1)] public int quantity;
        }
    
        [SerializeField] private List<Ingredient> inputs;
        [SerializeField] private List<ItemStack> outputs;

        [Header("Advanced")]
        [SerializeField] private bool copyInstancePropertiesToOutputs = false;

        /// <summary>
        /// Can the given inventory craft the specified item(s)?
        /// </summary>
        public override bool IsValid(Inventory crafter, int multiplier)
        {
            //FIXME Would potentially error if we had duplicate types in our inputs
            return inputs.All(i => crafter.Count(i.filter) >= i.quantity * multiplier);
        }

        /// <summary>
        /// Business logic function that actually handles the exchange. DO NOT CALL DIRECTLY.
        /// </summary>
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
                    IEnumerable<ItemStack> inputs_removed = crafter.TryRemove(i.filter, i.quantity * multiplier, this);
                    inputs_instanced.AddRange(inputs_removed);
                    stillValid &= inputs_removed.Count() > 0;
                }

                Debug.Assert(stillValid);
            }
            catch (Exception e)
            {
                //Something went wrong! Refund items.
                foreach (ItemStack s in inputs_instanced) crafter.AddItem(s, this);

                throw new Exception("Something went wrong while processing SimpleCraftingRecipe!", e);
            }

            //Add items provided recipe inputs were valid
            if (stillValid)
            {
                foreach (ItemStack i in outputs)
                {
                    ItemStack stack = i.Clone();
                    stack.quantity *= multiplier; //FIXME is this safe?

                    if (copyInstancePropertiesToOutputs)
                    {
                        //FIXME will likely error if instance property types overlap
                        foreach (ItemStack input in inputs_instanced) foreach (ItemInstanceProperty prop in input.instanceProperties) stack.instanceProperties.Add(prop);
                    }

                    crafter.AddItem(stack, this);
                }
            }

            return stillValid;
        }

        /// <summary>
        /// A quick way to scale up a recipe. Note that there is no option to scale down.
        /// </summary>
        public void MultiplyInPlace(int scale)
        {
            foreach (Ingredient i in inputs) i.quantity *= scale;
            foreach (ItemStack s in outputs) s.quantity *= scale;
        }
    }

}