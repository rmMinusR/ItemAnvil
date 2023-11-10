using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class SortHookTests
    {
        private (Inventory, Item) Arrange(params InventoryProperty[] properties)
        {
            StandardInventory inv = new StandardInventory(30);
            foreach (InventoryProperty i in properties) inv.properties.Add(i);
            inv.DoSetup();

            Item item = ScriptableObject.CreateInstance<Item>();
            inv.GetSlot(1).Contents = new ItemStack(item);
            inv.GetSlot(2).Contents = new ItemStack(item);

            return (inv, item);
        }

        [Test]
        public void TrySortSlotHook_Fires()
        {
            // Arrange
            (Inventory inventory, Item item) = Arrange();
            int fireCount = 0;
            inventory.Hooks.TrySortSlot.InsertHook((ReadOnlyInventorySlot slot, object cause) =>
            {
                fireCount++;
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            inventory.Sort(stack => -stack?.quantity ?? 0, null);
            
            // Assert
            Assert.AreEqual(2, inventory.Count(item));
            Assert.NotZero(fireCount);
        }

        [Test]
        public void TrySortSlotHook_PreventSorting()
        {
            // Arrange
            (Inventory inventory, Item item) = Arrange();
            // Pin slot with ID 2
            inventory.Hooks.TrySortSlot.InsertHook((ReadOnlyInventorySlot slot, object cause) => slot.ID == 2 ? QueryEventResult.Deny : QueryEventResult.Allow, int.MaxValue);

            // Act
            inventory.Sort(stack => -stack?.quantity ?? 0, null);

            // Assert
            Assert.AreEqual(2, inventory.Count(item));
            Assert.AreEqual(item, inventory.GetSlot(2).Contents?.itemType);
        }

        [Test]
        public void PostSortHook_Fires()
        {
            // Arrange
            (Inventory inventory, Item item) = Arrange();
            int fireCount = 0;
            inventory.Hooks.PostSort.InsertHook((object cause) =>
            {
                fireCount++;
                return PostEventResult.Continue;
            }, int.MaxValue);

            // Act
            inventory.Sort(stack => -stack?.quantity ?? 0, null);

            // Assert
            Assert.AreEqual(2, inventory.Count(item));
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void PostSortHook_CanRerun()
        {
            // Arrange
            (Inventory inventory, Item item) = Arrange();
            int fireCount = 0;
            inventory.Hooks.PostSort.InsertHook((object cause) =>
            {
                fireCount++;
                return fireCount == 1 ? PostEventResult.Retry : PostEventResult.Continue;
            }, int.MaxValue);

            // Act
            inventory.Sort(stack => -stack?.quantity ?? 0, null);
            
            // Assert
            Assert.AreEqual(2, inventory.Count(item));
            Assert.AreEqual(2, fireCount);
        }
    }

}