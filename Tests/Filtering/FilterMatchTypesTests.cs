using UnityEngine;
using NUnit.Framework;

namespace rmMinusR.ItemAnvil.Tests
{

    [Category("Filtering")]
    public class FilterMatchTypesTests
    {
        private Item CreateItem(string displayName)
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.displayName = displayName;
            return item;
        }

        private ItemStack CreateItemStack(Item itemType)
        {
            ItemStack itemStack = new ItemStack(itemType);
            return itemStack;
        }

        [Test]
        public void Matches_MatchingAtLeastOneItemType_ReturnsTrue()
        {
            // Arrange
            Item match1 = CreateItem("Match1");
            Item match2 = CreateItem("Match2");
            Item nonMatch = CreateItem("NonMatch");

            FilterMatchTypes filter = new FilterMatchTypes();
            filter.matches = new Item[] { match1, match2 };

            ItemStack matchingItemStack = CreateItemStack(match1);

            // Act
            bool result = filter.Matches(matchingItemStack);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        public void Matches_NonMatchingItemTypes_ReturnsFalse()
        {
            // Arrange
            Item match1 = CreateItem("Match1");
            Item match2 = CreateItem("Match2");
            Item nonMatch = CreateItem("NonMatch");

            FilterMatchTypes filter = new FilterMatchTypes();
            filter.matches = new Item[] { match1, match2 };

            ItemStack nonMatchingItemStack = CreateItemStack(nonMatch);

            // Act
            bool result = filter.Matches(nonMatchingItemStack);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        public void Matches_MatchingAllItemTypes_ReturnsTrue()
        {
            // Arrange
            Item match1 = CreateItem("Match1");
            Item match2 = CreateItem("Match2");
            Item nonMatch = CreateItem("NonMatch");

            FilterMatchTypes filter = new FilterMatchTypes();
            filter.matches = new Item[] { match1, match2 };

            // Act
            bool match1_result = filter.Matches(CreateItemStack(match1));
            bool match2_result = filter.Matches(CreateItemStack(match2));
            bool nonMatch_result = filter.Matches(CreateItemStack(nonMatch));

            // Assert
            Assert.IsTrue(match1_result);
            Assert.IsTrue(match2_result);
            Assert.IsFalse(nonMatch_result);
        }
    }

}