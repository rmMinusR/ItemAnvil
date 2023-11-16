using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class FuzzyCraftingRecipeTests
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

        private class DummyProp : ItemProperty
        {
            protected override void InstallHooks(InventorySlot context) { }
            protected override void UninstallHooks(InventorySlot context) { }
        }


        [Test]
        public void ExactlyEnoughItems_Crafts()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            ItemCategory category = new ItemCategory();
            ingredientB.categories.Add(category);
            inventory.AddItem(ingredientA, 1, null);
            inventory.AddItem(ingredientB, 2, null);
            Item result = CreateItem("Result");

            FuzzyCraftingRecipe recipe = new FuzzyCraftingRecipe(
                new FuzzyCraftingRecipe.Ingredient[] {
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchType    () { match = ingredientA }, quantity = 1 },
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchCategory() { category = category }, quantity = 2 },
                },
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
            ItemCategory category = new ItemCategory();
            ingredientB.categories.Add(category);
            inventory.AddItem(ingredientA, 3, null);
            inventory.AddItem(ingredientB, 4, null);
            Item result = CreateItem("Result");

            FuzzyCraftingRecipe recipe = new FuzzyCraftingRecipe(
                new FuzzyCraftingRecipe.Ingredient[] {
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchType    () { match = ingredientA }, quantity = 1 },
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchCategory() { category = category }, quantity = 2 },
                },
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
            ItemCategory category = new ItemCategory();
            ingredientB.categories.Add(category);
            inventory.AddItem(ingredientA, 1, null);
            inventory.AddItem(ingredientB, 1, null);
            Item result = CreateItem("Result");

            FuzzyCraftingRecipe recipe = new FuzzyCraftingRecipe(
                new FuzzyCraftingRecipe.Ingredient[] {
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchType    () { match = ingredientA }, quantity = 1 },
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchCategory() { category = category }, quantity = 2 },
                },
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
            ItemCategory category = new ItemCategory();
            ingredientB.categories.Add(category);
            inventory.AddItem(ingredientA, 1, null);
            Item result = CreateItem("Result");

            FuzzyCraftingRecipe recipe = new FuzzyCraftingRecipe(
                new FuzzyCraftingRecipe.Ingredient[] {
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchType    () { match = ingredientA }, quantity = 1 },
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchCategory() { category = category }, quantity = 2 },
                },
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

        private class DummyInstanceProp : ItemInstanceProperty { }

        [Test]
        public void CopyInstanceProperties_CopiesProperties()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item ingredientA = CreateItem("Ingredient A");
            Item ingredientB = CreateItem("Ingredient B");
            ItemCategory category = new ItemCategory();
            ingredientB.categories.Add(category);
            inventory.AddItem(ingredientA, 1, null);
            {
                ItemStack stack = new ItemStack(ingredientB, 2);
                stack.instanceProperties.Add<DummyInstanceProp>();
                inventory.AddItem(stack, null);
            }
            Item result = CreateItem("Result");

            FuzzyCraftingRecipe recipe = new FuzzyCraftingRecipe(
                new FuzzyCraftingRecipe.Ingredient[] {
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchType    () { match = ingredientA }, quantity = 1 },
                    new FuzzyCraftingRecipe.Ingredient() { filter = new FilterMatchCategory() { category = category }, quantity = 2 },
                },
                new ItemStack[] { new ItemStack(result, 1) },
                true
            );

            // Act
            bool ret = recipe.TryExchange(inventory, 1);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(0, inventory.Count(ingredientA));
            Assert.AreEqual(0, inventory.Count(ingredientB));
            Assert.AreEqual(1, inventory.Count(result));
            ItemStack resultStack = inventory.FindFirst(result);
            Assert.NotNull(resultStack);
            Assert.IsTrue(resultStack.instanceProperties.Contains<DummyInstanceProp>());
        }
    }

}