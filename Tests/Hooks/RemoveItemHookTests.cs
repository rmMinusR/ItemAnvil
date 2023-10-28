using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class RemoveItemHookTests
    {
        private (Inventory, Item) Arrange(int itemCount = 1, params InventoryProperty[] properties)
        {
            StandardInventory inv = new StandardInventory(30);
            foreach (InventoryProperty i in properties) inv.properties.Add(i);
            inv.DoSetup();

            Item item = ScriptableObject.CreateInstance<Item>();
            inv.AddItem(item, itemCount, null);

            return (inv, item);
        }

        [Test]
        public void RemoveItemHook_TryRemove_Fires()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            int timesFired = 0;
            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => {
                timesFired++;
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            IEnumerable<ItemStack> removed = inv.TryRemove(item, 1, null);

            // Assert
            Assert.NotZero(timesFired);
            Assert.AreEqual(1, removed.Sum(stack => stack.quantity));
        }

        [Test]
        public void RemoveItemHook_RemoveAll_Fires()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            int timesFired = 0;
            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => {
                timesFired++;
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            int removed = inv.RemoveAll(item, null);

            // Assert
            Assert.NotZero(timesFired);
            Assert.AreEqual(1, removed);
        }

        [Test]
        public void RemoveItemHook_TryRemove_Skips()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            inv.GetSlot(1).Contents = new ItemStack(item);

            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => slot.ID == 0 ? QueryEventResult.Deny : QueryEventResult.Allow, int.MaxValue);

            // Act
            IEnumerable<ItemStack> removed = inv.TryRemove(item, 1, null);

            // Assert
            Assert.AreEqual(1, removed.Sum(stack => stack.quantity));
            Assert.AreEqual(1, inv.Count(item));
        }

        [Test]
        public void RemoveItemHook_RemoveAll_Skips()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            inv.GetSlot(1).Contents = new ItemStack(item);

            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) => slot.ID == 0 ? QueryEventResult.Deny : QueryEventResult.Allow, int.MaxValue);

            // Act
            int removed = inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(1, removed);
            Assert.AreEqual(1, inv.Count(item));
        }

        [Test]
        public void RemoveItemHook_TryRemove_ChangeConsumptionPerStack()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange(itemCount: 2);
            inv.GetSlot(1).Contents = inv.GetSlot(0).Contents.Clone();

            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) =>
            {
                removed.quantity = Mathf.Min(removed.quantity, 1);
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            IEnumerable<ItemStack> removed = inv.TryRemove(item, 2, null);

            // Assert
            Assert.AreEqual(2, removed.Sum(stack => stack.quantity));
            Assert.AreEqual(2, inv.Count(item));
            Assert.AreEqual(2, inv.GetContents().Count(stack => !ItemStack.IsEmpty(stack)));
        }

        [Test]
        public void RemoveItemHook_RemoveAll_ChangeConsumptionPerStack()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange(itemCount: 2);
            inv.GetSlot(1).Contents = inv.GetSlot(0).Contents.Clone();
            
            inv.HookRemoveItem((ReadOnlyInventorySlot slot, ItemStack removed, ReadOnlyItemStack originalRemoved, object cause) =>
            {
                removed.quantity = Mathf.Min(removed.quantity, 1);
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            int removed = inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(2, removed);
            Assert.AreEqual(2, inv.Count(item));
            Assert.AreEqual(2, inv.GetContents().Count(stack => !ItemStack.IsEmpty(stack)));
        }

        [Test]
        public void PostRemoveHook_TryRemove_Fires()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            int timesFired = 0;
            inv.HookPostRemove((object cause) => {
                timesFired++;
            }, int.MaxValue);

            // Act
            IEnumerable<ItemStack> removed = inv.TryRemove(item, 1, null);

            // Assert
            Assert.AreEqual(1, timesFired);
            Assert.AreEqual(1, removed.Sum(stack => stack.quantity));
        }

        [Test]
        public void PostRemoveHook_RemoveAll_Fires()
        {
            // Arrange
            (Inventory inv, Item item) = Arrange();
            int timesFired = 0;
            inv.HookPostRemove((object cause) => {
                timesFired++;
            }, int.MaxValue);

            // Act
            int removed = inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(1, timesFired);
            Assert.AreEqual(1, removed);
        }
    }

}