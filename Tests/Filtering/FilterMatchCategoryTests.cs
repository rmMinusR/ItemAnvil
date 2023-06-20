using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Tests
{

    [Category("Filtering")]
    public class FilterMatchCategoryTests
    {
        private ItemCategory CreateItemCategory(string categoryName)
        {
            ItemCategory category = ScriptableObject.CreateInstance<ItemCategory>();
            category.name = categoryName;
            return category;
        }

        private ItemStack CreateItemStack(Item item, int quantity)
        {
            ItemStack itemStack = new ItemStack();
            itemStack.itemType = item;
            itemStack.quantity = quantity;
            return itemStack;
        }

        [Test]
        public void Matches_MatchingCategory_ReturnsTrue()
        {
            // Arrange
            ItemCategory categoryToMatch = CreateItemCategory("TestCategory");
            FilterMatchCategory filter = new FilterMatchCategory();
            filter.category = categoryToMatch;
            ItemStack itemStack = CreateItemStack(ScriptableObject.CreateInstance<Item>(), 1);
            itemStack.itemType.categories = new List<ItemCategory>() { categoryToMatch };

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_NonMatchingCategory_ReturnsFalse()
        {
            // Arrange
            ItemCategory categoryToMatch = CreateItemCategory("TestCategory");
            FilterMatchCategory filter = new FilterMatchCategory();
            filter.category = categoryToMatch;
            ItemStack itemStack = CreateItemStack(ScriptableObject.CreateInstance<Item>(), 1);
            itemStack.itemType.categories = new List<ItemCategory>() { CreateItemCategory("DifferentCategory") };

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsFalse(result);
        }
    }

}