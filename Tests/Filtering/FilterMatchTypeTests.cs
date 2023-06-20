using UnityEngine;
using NUnit.Framework;

namespace rmMinusR.ItemAnvil.Tests
{

    [Category("Filtering")]
    public class FilterMatchTypeTests
    {
        private Item CreateItem(string displayName)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.displayName = displayName;
            return item;
        }

        private ItemStack CreateItemStack(Item item)
        {
            ItemStack itemStack = new ItemStack(item);
            return itemStack;
        }

        [Test]
        public void Matches_ItemTypeMatches_ReturnsTrue()
        {
            // Arrange
            Item item = CreateItem("Test Item");
            FilterMatchType filter = new FilterMatchType();
            filter.match = item;

            ItemStack itemStack = CreateItemStack(item);

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_ItemTypeDoesNotMatch_ReturnsFalse()
        {
            // Arrange
            Item item1 = CreateItem("Item 1");
            Item item2 = CreateItem("Item 2");
            FilterMatchType filter = new FilterMatchType();
            filter.match = item1;

            ItemStack itemStack = CreateItemStack(item2);

            // Act
            bool result = filter.Matches(itemStack);

            // Assert
            Assert.IsFalse(result);
        }
    }

}