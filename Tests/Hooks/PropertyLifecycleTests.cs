using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class PropertyLifecycleTests
    {
        private Inventory CreateInventory()
        {
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();
            return inv;
        }

        #region Item property

        private class ItemCanary : ItemProperty
        {
            public int installCount = 0;
            public int uninstallCount = 0;

            protected override void InstallHooks(InventorySlot context) { installCount++; }
            protected override void UninstallHooks(InventorySlot context) { uninstallCount++; }
        }

        [Test]
        public void Inventory_AddItem_ItemPropertyLifecycle()
        {
            // Arrange
            Item item = ScriptableObject.CreateInstance<Item>();
            ItemCanary canary = item.Properties.Add<ItemCanary>();

            Inventory inv = CreateInventory();

            // Act
            inv.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.AreEqual(0, canary.uninstallCount);
        }

        [Test]
        public void Inventory_RemoveItem_ItemPropertyLifecycle()
        {
            // Arrange
            Item item = ScriptableObject.CreateInstance<Item>();
            ItemCanary canary = item.Properties.Add<ItemCanary>();

            Inventory inv = CreateInventory();
            inv.AddItem(item, null);

            // Act
            inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.AreEqual(1, canary.uninstallCount);
        }

        #endregion

        /* TODO implement
        private class InstanceCanary : ItemInstanceProperty
        {
            public int installCount = 0;
            public int uninstallCount = 0;

            protected override void InstallHooks(InventorySlot context) { installCount++; }
            protected override void UninstallHooks(InventorySlot context) { uninstallCount++; }
        }
        */
        
        #region Slot property

        private class SlotCanary : SlotProperty
        {
            public int installCount = 0;
            public int uninstallCount = 0;

            protected override void InstallHooks(InventorySlot context) { installCount++; }
            protected override void UninstallHooks(InventorySlot context) { uninstallCount++; }
        }

        [Test]
        public void Slot_AddProperty_ByTypeParameter_BeforeSetup()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);

            // Act
            SlotCanary canary = inv.GetSlot(0).AddProperty<SlotCanary>();
            inv.DoSetup();

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.IsTrue(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        [Test]
        public void Slot_AddProperty_ByTypeParameter_AfterSetup()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);

            // Act
            inv.DoSetup();
            SlotCanary canary = inv.GetSlot(0).AddProperty<SlotCanary>();

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.IsTrue(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        [Test]
        public void Slot_AddProperty_ByValue_BeforeSetup()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);

            // Act
            SlotCanary canary = new SlotCanary();
            inv.GetSlot(0).AddProperty(canary);
            inv.DoSetup();

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.IsTrue(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        [Test]
        public void Slot_AddProperty_ByValue_AfterSetup()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);

            // Act
            inv.DoSetup();
            SlotCanary canary = new SlotCanary();
            inv.GetSlot(0).AddProperty(canary);

            // Assert
            Assert.AreEqual(1, canary.installCount);
            Assert.IsTrue(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        [Test]
        public void Slot_RemoveProperty()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);
            SlotCanary canary = inv.GetSlot(0).AddProperty<SlotCanary>();
            inv.DoSetup();

            // Act
            bool removed = inv.GetSlot(0).RemoveProperty<SlotCanary>();

            // Assert
            Assert.IsTrue(removed);
            Assert.AreEqual(1, canary.uninstallCount);
            Assert.IsFalse(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        [Test]
        public void Slot_RemoveProperty_NonExistent()
        {
            // Arrange
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();

            // Act
            bool removed = inv.GetSlot(0).RemoveProperty<SlotCanary>();

            // Assert
            Assert.IsFalse(removed);
            Assert.IsFalse(inv.GetSlot(0).SlotProperties.Contains<SlotCanary>());
        }

        #endregion

        #region Inventory property

        private class InventoryCanary : InventoryProperty
        {
            public int installCount = 0;

            protected override void InstallHooks() { installCount++; }
        }
        
        [Test]
        public void Inventory_AddProperty_BeforeSetup()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(30);

            // Act
            InventoryCanary canary = inv.AddProperty<InventoryCanary>();
            inv.DoSetup();

            // Assert
            Assert.AreEqual(1, canary.installCount);
        }

        [Test]
        public void Inventory_AddProperty_AfterSetup()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(30);

            // Act
            inv.DoSetup();
            InventoryCanary canary = inv.AddProperty<InventoryCanary>();

            // Assert
            Assert.AreEqual(1, canary.installCount);
        }

        #endregion
    }

}
