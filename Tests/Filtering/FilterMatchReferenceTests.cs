using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Tests
{

    public class FilterMatchReferenceTests
    {
        private ItemCategory CreateItemCategory(string displayName)
        {
            ItemCategory itemCategory = ScriptableObject.CreateInstance<ItemCategory>();
            itemCategory.name = displayName;
            return itemCategory;
        }

        private Item CreateItem(string displayName, params ItemCategory[] categories)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.displayName = displayName;
            item.name = displayName;
            item.categories = new List<ItemCategory>(categories);
            return item;
        }

        [Test]
        public void FilterMatchByExample_MatchByItemType_ReturnsTrue()
        {
            // Arrange
            ItemCategory category1 = CreateItemCategory("Category1");
            ItemCategory category2 = CreateItemCategory("Category2");

            Item itemToMatchBy = CreateItem("ItemToMatchBy", category1);
            Item itemNotToMatch = CreateItem("ItemNotToMatch", category2);

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = new ItemStack(itemToMatchBy);

            // Act
            bool result = filter.Matches(new ItemStack(itemToMatchBy));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_IgnoreItemType_ReturnsTrue()
        {
            // Arrange
            ItemCategory category1 = CreateItemCategory("Category1");
            ItemCategory category2 = CreateItemCategory("Category2");

            Item itemToMatchBy = CreateItem("ItemToMatchBy", category1);
            Item itemNotToMatch = CreateItem("ItemNotToMatch", category2);

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = new ItemStack(itemToMatchBy);
            filter.matchType = false;

            // Act
            bool result = filter.Matches(new ItemStack(itemNotToMatch));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByQuantity_Ignore_ReturnsTrue()
        {
            // Arrange
            ItemCategory category1 = CreateItemCategory("Category1");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category1);

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = new ItemStack(itemToMatchBy, 10);
            filter.matchQuantity = FilterMatchReference.MatchMode.Ignore;

            // Act
            bool result = filter.Matches(new ItemStack(itemToMatchBy, 5));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByQuantity_Fuzzy_ReturnsTrue()
        {
            // Arrange
            ItemCategory category1 = CreateItemCategory("Category1");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category1);

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = new ItemStack(itemToMatchBy, 8);
            filter.matchQuantity = FilterMatchReference.MatchMode.Fuzzy;

            // Act
            bool result = filter.Matches(new ItemStack(itemToMatchBy, 10));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByQuantity_Exact_ReturnsTrue()
        {
            // Arrange
            ItemCategory category1 = CreateItemCategory("Category1");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category1);

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = new ItemStack(itemToMatchBy, 10);
            filter.matchQuantity = FilterMatchReference.MatchMode.Exact;

            // Act
            bool result = filter.Matches(new ItemStack(itemToMatchBy, 10));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_IgnoreMode_ReturnsTrue()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);
            Item itemNotToMatch = CreateItem("ItemNotToMatch", category);

            // Create the exampleToMatchBy with specific instance properties
            ItemStack exampleToMatchBy = new ItemStack(itemToMatchBy);
            exampleToMatchBy.instanceProperties.Add(new InstanceProperty1());
            exampleToMatchBy.instanceProperties.Add(new InstanceProperty2());

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = exampleToMatchBy;
            filter.matchInstanceProperties = FilterMatchReference.MatchMode.Ignore;

            // Act
            bool result = filter.Matches(new ItemStack(itemNotToMatch));

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_FuzzyMatch_ReturnsTrue_ForExactProperties()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            // Create instance properties
            InstanceProperty1 prop1ToMatchBy = new InstanceProperty1();
            InstanceProperty2 prop2ToMatchBy = new InstanceProperty2();

            // Add instance properties to the exampleToMatchBy
            ItemStack itemStackToMatchBy = new ItemStack(itemToMatchBy);
            itemStackToMatchBy.instanceProperties.Add(prop1ToMatchBy);
            itemStackToMatchBy.instanceProperties.Add(prop2ToMatchBy);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = itemStackToMatchBy;

            // Create an ItemStack with matching instance properties (fuzzy match)
            Item itemMatch = CreateItem("ItemMatch", category);
            ItemStack itemStackMatch = new ItemStack(itemMatch);
            itemStackMatch.instanceProperties.Add(prop1ToMatchBy);
            itemStackMatch.instanceProperties.Add(prop2ToMatchBy);

            // Act
            bool result = filter.Matches(itemStackMatch);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_FuzzyMatch_ReturnsTrue_ForPartialProperties()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            // Create instance properties
            InstanceProperty1 prop1ToMatchBy = new InstanceProperty1();

            // Add instance properties to the exampleToMatchBy
            ItemStack itemStackToMatchBy = new ItemStack(itemToMatchBy);
            itemStackToMatchBy.instanceProperties.Add(prop1ToMatchBy);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = itemStackToMatchBy;

            // Create an ItemStack with partially matching instance properties (fuzzy match)
            Item itemPartialMatch = CreateItem("ItemPartialMatch", category);
            ItemStack itemStackPartialMatch = new ItemStack(itemPartialMatch);
            itemStackPartialMatch.instanceProperties.Add(prop1ToMatchBy);

            // Act
            bool result = filter.Matches(itemStackPartialMatch);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_FuzzyMatch_ReturnsFalse_ForNoProperties()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            // Create instance properties
            InstanceProperty1 prop1ToMatchBy = new InstanceProperty1();
            InstanceProperty2 prop2ToMatchBy = new InstanceProperty2();

            // Add instance properties to the exampleToMatchBy
            ItemStack itemStackToMatchBy = new ItemStack(itemToMatchBy);
            itemStackToMatchBy.instanceProperties.Add(prop1ToMatchBy);
            itemStackToMatchBy.instanceProperties.Add(prop2ToMatchBy);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = itemStackToMatchBy;

            // Create an ItemStack without matching instance properties
            Item itemNotMatch = CreateItem("ItemNotMatch", category);
            ItemStack itemStackNotMatch = new ItemStack(itemNotMatch);

            // Act
            bool result = filter.Matches(itemStackNotMatch);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_Exact_SameProperties_ReturnsTrue()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.matchInstanceProperties = FilterMatchReference.MatchMode.Exact;
            filter.stack = new ItemStack(itemToMatchBy);
            filter.stack.instanceProperties.Add(new InstanceProperty1());
            filter.stack.instanceProperties.Add(new InstanceProperty2());

            // Create an ItemStack to match against the example
            ItemStack itemStack = new ItemStack(itemToMatchBy);
            itemStack.instanceProperties.Add(new InstanceProperty1());
            itemStack.instanceProperties.Add(new InstanceProperty2());

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_Exact_MissingProperties_ReturnsFalse()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = new ItemStack(itemToMatchBy);
            filter.matchInstanceProperties = FilterMatchReference.MatchMode.Exact;

            // Create an example ItemStack with the instance properties
            ItemStack exampleItemStack = new ItemStack(itemToMatchBy);
            exampleItemStack.instanceProperties.Add(new InstanceProperty1());
            exampleItemStack.instanceProperties.Add(new InstanceProperty2());

            // Create an ItemStack missing one property
            ItemStack itemStack = new ItemStack(itemToMatchBy);
            itemStack.instanceProperties.Add(new InstanceProperty1());

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_Exact_AdditionalProperties_ReturnsFalse()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);

            FilterMatchReference filter = new FilterMatchReference();
            filter.matchType = false;
            filter.stack = new ItemStack(itemToMatchBy);
            filter.matchInstanceProperties = FilterMatchReference.MatchMode.Exact;

            // Create an example ItemStack with the instance properties
            ItemStack exampleItemStack = new ItemStack(itemToMatchBy);
            exampleItemStack.instanceProperties.Add(new InstanceProperty1());

            // Create an ItemStack with additional property
            ItemStack itemStack = new ItemStack(itemToMatchBy);
            itemStack.instanceProperties.Add(new InstanceProperty1());
            itemStack.instanceProperties.Add(new InstanceProperty2());

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void FilterMatchByExample_MatchByInstanceProperties_NotAllPropertiesPresent_ReturnsFalse()
        {
            // Arrange
            ItemCategory category = CreateItemCategory("Category");
            Item itemToMatchBy = CreateItem("ItemToMatchBy", category);
            Item itemNotToMatch = CreateItem("ItemNotToMatch", category);

            // Create the exampleToMatchBy with specific instance properties
            ItemStack exampleToMatchBy = new ItemStack(itemToMatchBy);
            exampleToMatchBy.instanceProperties.Add(new InstanceProperty1());
            exampleToMatchBy.instanceProperties.Add(new InstanceProperty2());

            FilterMatchReference filter = new FilterMatchReference();
            filter.stack = exampleToMatchBy;

            // Act
            bool result = filter.Matches(new ItemStack(itemNotToMatch));

            // Assert
            Assert.IsFalse(result);
        }

        private class InstanceProperty1 : ItemInstanceProperty { }
        private class InstanceProperty2 : ItemInstanceProperty { }
    }

}