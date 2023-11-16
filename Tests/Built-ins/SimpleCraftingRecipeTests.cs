using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class SimpleCraftingRecipeTests
    {
        private Inventory CreateInventory()
        {
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();
            return inv;
        }

        private Item CreateItem(string name)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.name = name;
            item.displayName = name;
            return item;
        }

        [Test]
        public void ExactlyEnoughItems_Crafts()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            inventory.AddItem(ingredientA, 1, null);
            inventory.AddItem(ingredientB, 2, null);
            Item result = CreateItem("Result");

            SimpleCraftingRecipe recipe = new SimpleCraftingRecipe(
                new ItemStack[] { new ItemStack(ingredientA, 1), new ItemStack(ingredientB, 2) },
                new ItemStack[] { new ItemStack(result, 1) }
            );

            // Act
            bool ret = recipe.TryExchange(inventory, 1);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(0, inventory.Count(ingredientA));
            Assert.AreEqual(0, inventory.Count(ingredientB));
            Assert.AreEqual(1, inventory.Count(result));
        }

        [Test]
        public void MoreThanEnoughItems_Crafts()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            inventory.AddItem(ingredientA, 3, null);
            inventory.AddItem(ingredientB, 4, null);
            Item result = CreateItem("Result");

            SimpleCraftingRecipe recipe = new SimpleCraftingRecipe(
                new ItemStack[] { new ItemStack(ingredientA, 1), new ItemStack(ingredientB, 2) },
                new ItemStack[] { new ItemStack(result, 1) }
            );

            // Act
            bool ret = recipe.TryExchange(inventory, 1);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(2, inventory.Count(ingredientA));
            Assert.AreEqual(2, inventory.Count(ingredientB));
            Assert.AreEqual(1, inventory.Count(result));
        }

        [Test]
        public void InsufficientQuantity_DoesNotCraft()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            inventory.AddItem(ingredientA, 1, null);
            inventory.AddItem(ingredientB, 1, null);
            Item result = CreateItem("Result");

            SimpleCraftingRecipe recipe = new SimpleCraftingRecipe(
                new ItemStack[] { new ItemStack(ingredientA, 1), new ItemStack(ingredientB, 2) },
                new ItemStack[] { new ItemStack(result, 1) }
            );

            // Act
            bool ret = recipe.TryExchange(inventory, 1);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(1, inventory.Count(ingredientA));
            Assert.AreEqual(1, inventory.Count(ingredientB));
            Assert.AreEqual(0, inventory.Count(result));
        }

        [Test]
        public void MissingItems_DoesNotCraft()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            inventory.AddItem(ingredientA, 1, null);
            Item result = CreateItem("Result");

            SimpleCraftingRecipe recipe = new SimpleCraftingRecipe(
                new ItemStack[] { new ItemStack(ingredientA, 1), new ItemStack(ingredientB, 2) },
                new ItemStack[] { new ItemStack(result, 1) }
            );
            
            // Act
            bool ret = recipe.TryExchange(inventory, 1);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(1, inventory.Count(ingredientA));
            Assert.AreEqual(0, inventory.Count(ingredientB));
            Assert.AreEqual(0, inventory.Count(result));
        }
    }

}