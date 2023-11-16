using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class InventorySlotTests
    {
        private Inventory CreateInventory()
        {
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();
            return inv;
        }

        [Test]
        public void SwapContents_SameInventory_Parity()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(itemA, 5, null);
            inventory.AddItem(itemB, 3, null);

            // Act
            bool ret = InventorySlot.SwapContents(inventory.GetSlot(0), inventory.GetSlot(1), null);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(itemB, inventory.GetSlot(0).Contents.itemType);
            Assert.AreEqual(3    , inventory.GetSlot(0).Contents.quantity);
            Assert.AreEqual(itemA, inventory.GetSlot(1).Contents.itemType);
            Assert.AreEqual(5    , inventory.GetSlot(1).Contents.quantity);
        }

        [Test]
        public void SwapContents_MultiInventory_Parity()
        {
            // Arrange
            Inventory inventoryA = CreateInventory();
            Inventory inventoryB = CreateInventory();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventoryA.AddItem(itemA, 5, null);
            inventoryB.AddItem(itemB, 3, null);

            // Act
            bool ret = InventorySlot.SwapContents(inventoryA.GetSlot(0), inventoryB.GetSlot(0), null);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(itemB, inventoryA.GetSlot(0).Contents.itemType);
            Assert.AreEqual(3    , inventoryA.GetSlot(0).Contents.quantity);
            Assert.AreEqual(itemA, inventoryB.GetSlot(0).Contents.itemType);
            Assert.AreEqual(5    , inventoryB.GetSlot(0).Contents.quantity);
        }

        [Test]
        public void SwapContents_SameInventory_HooksCalled()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(itemA, 5, null);
            inventory.AddItem(itemB, 3, null);

            int preHookFired = 0;
            inventory.Hooks.TrySwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                preHookFired++;
                return QueryEventResult.Allow;
            }, 0);

            int postHookFired = 0;
            inventory.Hooks.PostSwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                postHookFired++;
            }, 0);
            
            // Act
            bool ret = InventorySlot.SwapContents(inventory.GetSlot(0), inventory.GetSlot(1), null);

            // Assert
            Assert.IsTrue(ret);
            Assert.NotZero(preHookFired);
            Assert.NotZero(postHookFired);
        }

        [Test]
        public void SwapContents_MultiInventory_HooksCalled()
        {
            // Arrange
            Inventory inventoryA = CreateInventory();
            Inventory inventoryB = CreateInventory();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventoryA.AddItem(itemA, 5, null);
            inventoryB.AddItem(itemB, 3, null);

            int preHookFiredA = 0;
            inventoryA.Hooks.TrySwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                preHookFiredA++;
                return QueryEventResult.Allow;
            }, 0);

            int postHookFiredA = 0;
            inventoryA.Hooks.PostSwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                postHookFiredA++;
            }, 0);

            int preHookFiredB = 0;
            inventoryB.Hooks.TrySwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                preHookFiredB++;
                return QueryEventResult.Allow;
            }, 0);

            int postHookFiredB = 0;
            inventoryB.Hooks.PostSwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) =>
            {
                postHookFiredB++;
            }, 0);

            // Act
            bool ret = InventorySlot.SwapContents(inventoryA.GetSlot(0), inventoryB.GetSlot(0), null);

            // Assert
            Assert.IsTrue(ret);
            Assert.NotZero(preHookFiredA);
            Assert.NotZero(postHookFiredA);
            Assert.NotZero(preHookFiredB);
            Assert.NotZero(postHookFiredB);
        }

        [Test]
        public void SwapContents_PreHook_Cancels()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(itemA, 5, null);
            inventory.AddItem(itemB, 3, null);

            inventory.Hooks.TrySwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) => QueryEventResult.Deny, 0);
            int postHookCanary = 0;
            inventory.Hooks.PostSwapSlots.InsertHook((InventorySlot slotA, InventorySlot slotB, object cause) => postHookCanary++, 0);

            // Act
            bool ret = InventorySlot.SwapContents(inventory.GetSlot(0), inventory.GetSlot(1), null);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(0, postHookCanary);
            Assert.AreEqual(itemA, inventory.GetSlot(0).Contents.itemType);
            Assert.AreEqual(5    , inventory.GetSlot(0).Contents.quantity);
            Assert.AreEqual(itemB, inventory.GetSlot(1).Contents.itemType);
            Assert.AreEqual(3    , inventory.GetSlot(1).Contents.quantity);
        }
    }

}