using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using System.Collections.Generic;

[Category("Filtering")]
public class FilterMatchAllTests
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
    public void MatchAll_CriteriaMatch_AllMatch()
    {
        // Arrange
        ItemCategory category = CreateItemCategory("Category 1");
        FilterMatchCategory filterCategory = new FilterMatchCategory { category = category };

        Item item = CreateItem("Item 1", category);
        FilterMatchType filterType = new FilterMatchType { match = item };

        Item[] items = { item };
        FilterMatchTypes filterTypes = new FilterMatchTypes { matches = items };

        FilterMatchAll filterMatchAll = new FilterMatchAll
        {
            criteria = new List<ItemFilter> { filterCategory, filterType, filterTypes }
        };

        ItemStack itemStack = new ItemStack(item);

        // Act
        bool result = filterMatchAll.Matches(itemStack);

        // Assert
        Assert.IsTrue(result);
    }

    [Test]
    public void MatchAll_CriteriaMatch_SomeMatch()
    {
        // Arrange
        ItemCategory category1 = CreateItemCategory("Category 1");
        ItemCategory category2 = CreateItemCategory("Category 2");
        FilterMatchCategory filterCategory = new FilterMatchCategory { category = category1 };

        Item item1 = CreateItem("Item 1", category1);
        FilterMatchType filterType = new FilterMatchType { match = item1 };

        Item item2 = CreateItem("Item 2", category2);
        FilterMatchTypes filterTypes = new FilterMatchTypes { matches = new Item[] { item2 } };

        FilterMatchAll filterMatchAll = new FilterMatchAll
        {
            criteria = new List<ItemFilter> { filterCategory, filterType, filterTypes }
        };

        ItemStack itemStack = new ItemStack(item1);

        // Act
        bool result = filterMatchAll.Matches(itemStack);

        // Assert
        Assert.IsFalse(result);
    }

    [Test]
    public void MatchAll_CriteriaMatch_NoneMatch()
    {
        // Arrange
        ItemCategory category1 = CreateItemCategory("Category 1");
        ItemCategory category2 = CreateItemCategory("Category 2");
        FilterMatchCategory filterCategory = new FilterMatchCategory { category = category1 };

        Item item1 = CreateItem("Item 1", category1);
        FilterMatchType filterType = new FilterMatchType { match = item1 };

        Item item2 = CreateItem("Item 2", category2);
        FilterMatchTypes filterTypes = new FilterMatchTypes { matches = new Item[] { item2 } };

        FilterMatchAll filterMatchAll = new FilterMatchAll
        {
            criteria = new List<ItemFilter> { filterCategory, filterType, filterTypes }
        };

        Item item3 = CreateItem("Item 3", category1);
        ItemStack itemStack = new ItemStack(item3);

        // Act
        bool result = filterMatchAll.Matches(itemStack);

        // Assert
        Assert.IsFalse(result);
    }
}
