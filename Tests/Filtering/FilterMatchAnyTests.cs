using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[Category("Filtering")]
public class FilterMatchAnyTests
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
    public void MatchAny_CriteriaMatch_AllMatch()
    {
        // Arrange
        var category1 = CreateItemCategory("Category1");
        var category2 = CreateItemCategory("Category2");
        var item1 = CreateItem("Item1", category1);
        var item2 = CreateItem("Item2", category2);
        var itemStack = new ItemStack(item1);
        var criteria = new List<ItemFilter>
        {
            new FilterMatchCategory { category = category1 },
            new FilterMatchType { match = item1 }
        };
        var filterMatchAny = new FilterMatchAny { criteria = criteria };

        // Act
        var result = filterMatchAny.Matches(itemStack);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void MatchAny_CriteriaMatch_SomeMatch()
    {
        // Arrange
        var category1 = CreateItemCategory("Category1");
        var category2 = CreateItemCategory("Category2");
        var item1 = CreateItem("Item1", category1);
        var item2 = CreateItem("Item2", category2);
        var itemStack = new ItemStack(item1);
        var criteria = new List<ItemFilter>
        {
            new FilterMatchCategory { category = category1 },
            new FilterMatchType { match = item2 }
        };
        var filterMatchAny = new FilterMatchAny { criteria = criteria };

        // Act
        var result = filterMatchAny.Matches(itemStack);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void MatchAny_CriteriaMatch_NoneMatch()
    {
        // Arrange
        var category1 = CreateItemCategory("Category1");
        var category2 = CreateItemCategory("Category2");
        var item1 = CreateItem("Item1", category1);
        var item2 = CreateItem("Item2", category2);
        var itemStack = new ItemStack(item1);
        var criteria = new List<ItemFilter>
        {
            new FilterMatchCategory { category = category2 },
            new FilterMatchType { match = item2 }
        };
        var filterMatchAny = new FilterMatchAny { criteria = criteria };

        // Act
        var result = filterMatchAny.Matches(itemStack);

        // Assert
        Assert.IsFalse(result);
    }
}
