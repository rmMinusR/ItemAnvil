using UnityEngine;
using NUnit.Framework;

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
        filter.matches = new Item[] { match1, match2, nonMatch };

        ItemStack matchingItemStack = CreateItemStack(match1);
        matchingItemStack.itemType = match2;

        // Act
        bool result = filter.Matches(matchingItemStack);

        // Assert
        Assert.IsTrue(result);
    }
}
