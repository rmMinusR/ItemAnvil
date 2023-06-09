using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[TestFixture]
public class FixedSlotInventoryTests : InventoryTests<FixedSlotInventory>
{
    protected override FixedSlotInventory CreateInventory() => new FixedSlotInventory(30);
}

[TestFixture]
public class CondensingInventoryTests : InventoryTests<CondensingInventory>
{
    protected override CondensingInventory CreateInventory() => new CondensingInventory();
}

public abstract class InventoryTests<TInventory> where TInventory : Inventory
{
    protected abstract TInventory CreateInventory();

    [Test, Combinatorial]
    public void AddItem_AddsItemToInventory([Values(1, 2, 5, 10)] int nToAdd)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item = ScriptableObject.CreateInstance<Item>();

        // Act
        inventory.AddItem(item, nToAdd);

        // Assert
        Assert.AreEqual(nToAdd, inventory.Count(item));
    }

    [Test, Combinatorial]
    public void AddItemStack_AddsItemStackToInventory([Values(1, 2, 5, 10)] int nToAdd)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item = ScriptableObject.CreateInstance<Item>();
        ItemStack stack = new ItemStack(item, nToAdd);

        // Act
        inventory.AddItem(stack);

        // Assert
        Assert.AreEqual(nToAdd, inventory.Count(item));
    }

    [Test, Combinatorial]
    public void TryRemove_RemovesItemsFromInventory([Values(1, 2, 5, 10)] int startingCount, [Values(1, 2, 5, 10)] int nToRemove)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item = ScriptableObject.CreateInstance<Item>();
        inventory.AddItem(item, startingCount);

        // Act
        IEnumerable<ItemStack> removedStacks = inventory.TryRemove(item, nToRemove);

        // Assert
        if (startingCount >= nToRemove)
        {
            // Had enough items
            Assert.AreEqual(startingCount-nToRemove, inventory.Count(item));
            Assert.AreEqual(nToRemove, removedStacks.Sum(stack => stack.quantity));
        }
        else
        {
            // Didn't have enough items
            Assert.AreEqual(startingCount, inventory.Count(item));
            Assert.AreEqual(0, removedStacks.Count());
        }
    }

    [Test, Combinatorial]
    public void RemoveAll_RemovesAllItemsOfTypeFromInventory([Values(1, 2, 5, 10)] int count)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item = ScriptableObject.CreateInstance<Item>();
        inventory.AddItem(item, count);

        // Act
        int removedCount = inventory.RemoveAll(item);

        // Assert
        Assert.AreEqual(0, inventory.Count(item));
        Assert.AreEqual(count, removedCount);
    }

    [Test, Combinatorial]
    public void Count_ReturnsCorrectItemCount([Values(1, 2, 5, 10)] int realCount1, [Values(1, 2, 5, 10)] int realCount2)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item1 = ScriptableObject.CreateInstance<Item>();
        Item item2 = ScriptableObject.CreateInstance<Item>();
        inventory.AddItem(item1, realCount1);
        inventory.AddItem(item2, realCount2);

        // Act
        int observedCount1 = inventory.Count(item1);
        int observedCount2 = inventory.Count(item2);

        // Assert
        Assert.AreEqual(realCount1, observedCount1);
        Assert.AreEqual(realCount2, observedCount2);
    }

    [Test, Combinatorial]
    public void GetContents_ReturnsCorrectItemStacks([Values(1, 2, 5, 10)] int count1, [Values(1, 2, 5, 10)] int count2)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item1 = ScriptableObject.CreateInstance<Item>();
        Item item2 = ScriptableObject.CreateInstance<Item>();
        inventory.AddItem(item1, count1);
        inventory.AddItem(item2, count2);

        // Act
        IEnumerable<ReadOnlyItemStack> contents = inventory.GetContents();

        // Assert
        Assert.AreEqual(2, contents.Count());
        Assert.IsTrue(contents.Any(stack => stack.itemType == item1 && stack.quantity == count1));
        Assert.IsTrue(contents.Any(stack => stack.itemType == item2 && stack.quantity == count2));
    }

    [Test, Combinatorial]
    public void FindFirst_ReturnsCorrectItemStack(
        [Values(1, 2, 5, 10)] int correctItemCount,
        [Values(1, 2, 3, 5, 7, 11, 13)] int correctItemStackIndex,
        [Values(0, 1, 2, 5, 10)] int confuserCount)
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item correct = ScriptableObject.CreateInstance<Item>();
        Item confuser = ScriptableObject.CreateInstance<Item>();
        correctItemStackIndex %= confuserCount+1;
        for (int i = 0; i < confuserCount+1; ++i)
        {
            if (i == correctItemStackIndex) inventory.AddItem(correct, correctItemCount);
            else                            inventory.AddItem(confuser, 1);
        }

        // Act
        ItemStack stack = inventory.FindFirst(correct);

        // Assert
        Assert.IsNotNull(stack);
        Assert.AreEqual(correct, stack.itemType);
        Assert.AreEqual(correctItemCount, stack.quantity);
    }

    [Test]
    public void CloneContents_ReturnsDeepCopyOfContents()
    {
        // Arrange
        TInventory inventory = CreateInventory();
        Item item = ScriptableObject.CreateInstance<Item>();
        inventory.AddItem(item, 5);

        // Act
        List<ItemStack> clonedContents = inventory.CloneContents();

        // Modify the cloned contents
        clonedContents[0].quantity = 10;

        // Assert
        Assert.AreEqual(5, inventory.Count(item));
    }
}
