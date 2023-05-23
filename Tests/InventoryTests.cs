using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

[TestFixture]
public class InventoryTests
{
    [Test]
    public void FixedSlotInventory_AddItem_AddsItemToInventory()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();

        // Act
        inventory.AddItem(item, 1);

        // Assert
        Assert.AreEqual(1, inventory.Count(item));
    }

    [Test]
    public void FixedSlotInventory_AddItemStack_AddsItemStackToInventory()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();
        ItemStack stack = new ItemStack(item, 5);

        // Act
        inventory.AddItem(stack);

        // Assert
        Assert.AreEqual(5, inventory.Count(item));
    }

    [Test]
    public void FixedSlotInventory_TryRemove_RemovesItemsFromInventory()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        IEnumerable<ItemStack> removedStacks = inventory.TryRemove(item, 3);

        // Assert
        Assert.AreEqual(2, inventory.Count(item));
        Assert.AreEqual(3, removedStacks.Sum(stack => stack.quantity));
    }

    [Test]
    public void FixedSlotInventory_RemoveAll_RemovesAllItemsOfTypeFromInventory()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        int removedCount = inventory.RemoveAll(item);

        // Assert
        Assert.AreEqual(0, inventory.Count(item));
        Assert.AreEqual(5, removedCount);
    }

    [Test]
    public void FixedSlotInventory_Count_ReturnsCorrectItemCount()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item1 = new Item();
        Item item2 = new Item();
        inventory.AddItem(item1, 3);
        inventory.AddItem(item2, 5);

        // Act
        int count1 = inventory.Count(item1);
        int count2 = inventory.Count(item2);

        // Assert
        Assert.AreEqual(3, count1);
        Assert.AreEqual(5, count2);
    }

    [Test]
    public void FixedSlotInventory_GetContents_ReturnsCorrectItemStacks()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item1 = new Item();
        Item item2 = new Item();
        inventory.AddItem(item1, 3);
        inventory.AddItem(item2, 5);

        // Act
        IEnumerable<ReadOnlyItemStack> contents = inventory.GetContents();

        // Assert
        Assert.AreEqual(2, contents.Count());
        Assert.IsTrue(contents.Any(stack => stack.itemType == item1 && stack.quantity == 3));
        Assert.IsTrue(contents.Any(stack => stack.itemType == item2 && stack.quantity == 5));
    }

    [Test]
    public void FixedSlotInventory_Find_ReturnsCorrectItemStack()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        ItemStack stack = inventory.Find(item);

        // Assert
        Assert.IsNotNull(stack);
        Assert.AreEqual(item, stack.itemType);
        Assert.AreEqual(5, stack.quantity);
    }

    [Test]
    public void FixedSlotInventory_CloneContents_ReturnsDeepCopyOfContents()
    {
        // Arrange
        FixedSlotInventory inventory = new FixedSlotInventory(30);
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        List<ItemStack> clonedContents = inventory.CloneContents();

        // Modify the cloned contents
        clonedContents[0].quantity = 10;

        // Assert
        Assert.AreEqual(5, inventory.Count(item));
    }

    [Test]
    public void CondensingInventory_AddItem_AddsItemToInventory()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();

        // Act
        inventory.AddItem(item, 1);

        // Assert
        Assert.AreEqual(1, inventory.Count(item));
    }

    [Test]
    public void CondensingInventory_AddItemStack_AddsItemStackToInventory()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();
        ItemStack stack = new ItemStack(item, 5);

        // Act
        inventory.AddItem(stack);

        // Assert
        Assert.AreEqual(5, inventory.Count(item));
    }

    [Test]
    public void CondensingInventory_TryRemove_RemovesItemsFromInventory()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        IEnumerable<ItemStack> removedStacks = inventory.TryRemove(item, 3);

        // Assert
        Assert.AreEqual(2, inventory.Count(item));
        Assert.AreEqual(3, removedStacks.Sum(stack => stack.quantity));
    }

    [Test]
    public void CondensingInventory_RemoveAll_RemovesAllItemsOfTypeFromInventory()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        int removedCount = inventory.RemoveAll(item);

        // Assert
        Assert.AreEqual(0, inventory.Count(item));
        Assert.AreEqual(5, removedCount);
    }

    [Test]
    public void CondensingInventory_Count_ReturnsCorrectItemCount()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item1 = new Item();
        Item item2 = new Item();
        inventory.AddItem(item1, 3);
        inventory.AddItem(item2, 5);

        // Act
        int count1 = inventory.Count(item1);
        int count2 = inventory.Count(item2);

        // Assert
        Assert.AreEqual(3, count1);
        Assert.AreEqual(5, count2);
    }

    [Test]
    public void CondensingInventory_GetContents_ReturnsCorrectItemStacks()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item1 = new Item();
        Item item2 = new Item();
        inventory.AddItem(item1, 3);
        inventory.AddItem(item2, 5);

        // Act
        IEnumerable<ReadOnlyItemStack> contents = inventory.GetContents();

        // Assert
        Assert.AreEqual(2, contents.Count());
        Assert.IsTrue(contents.Any(stack => stack.itemType == item1 && stack.quantity == 3));
        Assert.IsTrue(contents.Any(stack => stack.itemType == item2 && stack.quantity == 5));
    }

    [Test]
    public void CondensingInventory_Find_ReturnsCorrectItemStack()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        ItemStack stack = inventory.Find(item);

        // Assert
        Assert.IsNotNull(stack);
        Assert.AreEqual(item, stack.itemType);
        Assert.AreEqual(5, stack.quantity);
    }

    [Test]
    public void CondensingInventory_CloneContents_ReturnsDeepCopyOfContents()
    {
        // Arrange
        CondensingInventory inventory = new CondensingInventory();
        Item item = new Item();
        inventory.AddItem(item, 5);

        // Act
        List<ItemStack> clonedContents = inventory.CloneContents();

        // Modify the cloned contents
        clonedContents[0].quantity = 10;

        // Assert
        Assert.AreEqual(5, inventory.Count(item));
    }
}