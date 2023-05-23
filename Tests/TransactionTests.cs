using System.Collections.Generic;
using NUnit.Framework;

public class TransactionTests
{
    [Test]
    public void TryExchange_ValidTransaction_SuccessfullyExchangesItems()
    {
        // Arrange
        var inventoryA = new CondensingInventory();
        var inventoryB = new CondensingInventory();

        var itemA = new Item() { displayName = "ItemA" };
        var itemB = new Item() { displayName = "ItemB" };

        inventoryA.AddItem(itemA, 3);
        inventoryB.AddItem(itemB, 2);

        var itemsAtoB = new List<ItemStack>
        {
            new ItemStack(itemA, 2),
        };

        var itemsBtoA = new List<ItemStack>
        {
            new ItemStack(itemB, 1),
        };

        var transaction = new Transaction(itemsAtoB, itemsBtoA);

        // Act
        var exchangeResult = transaction.TryExchange(inventoryA, inventoryB);

        // Assert
        Assert.IsTrue(exchangeResult);
        Assert.AreEqual(1, inventoryA.Count(itemA));
        Assert.AreEqual(3, inventoryB.Count(itemB));
    }

    [Test]
    public void TryExchange_InvalidTransaction_DoesNotExchangeItems()
    {
        // Arrange
        var inventoryA = new CondensingInventory();
        var inventoryB = new CondensingInventory();

        var itemA = new Item() { displayName = "ItemA" };
        var itemB = new Item() { displayName = "ItemB" };

        inventoryA.AddItem(itemA, 3);
        inventoryB.AddItem(itemB, 2);

        var itemsAtoB = new List<ItemStack>
        {
            new ItemStack(itemA, 4), // More items than available in inventoryA
        };

        var itemsBtoA = new List<ItemStack>
        {
            new ItemStack(itemB, 1),
        };

        var transaction = new Transaction(itemsAtoB, itemsBtoA);

        // Act
        var exchangeResult = transaction.TryExchange(inventoryA, inventoryB);

        // Assert
        Assert.IsFalse(exchangeResult);
        Assert.AreEqual(3, inventoryA.Count(itemA)); // No items should be removed from inventoryA
        Assert.AreEqual(2, inventoryB.Count(itemB)); // No items should be added to inventoryB
    }

    [Test]
    public void IsValid_ValidTransaction_ReturnsTrue()
    {
        // Arrange
        var inventoryA = new CondensingInventory();
        var inventoryB = new CondensingInventory();

        var itemA = new Item() { displayName = "ItemA" };
        var itemB = new Item() { displayName = "ItemB" };

        inventoryA.AddItem(itemA, 3);
        inventoryB.AddItem(itemB, 2);

        var itemsAtoB = new List<ItemStack>
        {
            new ItemStack(itemA, 2),
        };

        var itemsBtoA = new List<ItemStack>
        {
            new ItemStack(itemB, 1),
        };

        var transaction = new Transaction(itemsAtoB, itemsBtoA);

        // Act
        var isValid = transaction.IsValid(inventoryA, inventoryB);

        // Assert
        Assert.IsTrue(isValid);
    }

    [Test]
    public void IsValid_InvalidTransaction_ReturnsFalse()
    {
        // Arrange
        var inventoryA = new CondensingInventory();
        var inventoryB = new CondensingInventory();

        var itemA = new Item() { displayName = "ItemA" };
        var itemB = new Item() { displayName = "ItemB" };

        inventoryA.AddItem(itemA, 3);
        inventoryB.AddItem(itemB, 2);

        var itemsAtoB = new List<ItemStack>
        {
            new ItemStack(itemA, 4), // More items than available in inventoryA
        };

        var itemsBtoA = new List<ItemStack>
        {
            new ItemStack(itemB, 1),
        };

        var transaction = new Transaction(itemsAtoB, itemsBtoA);

        // Act
        var isValid = transaction.IsValid(inventoryA, inventoryB);

        // Assert
        Assert.IsFalse(isValid);
    }
}
