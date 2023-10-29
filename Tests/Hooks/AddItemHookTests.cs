using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class AddItemHookTests
    {
        private Inventory CreateInventory(params InventoryProperty[] properties)
        {
            StandardInventory inv = new StandardInventory(30);
            foreach (InventoryProperty i in properties) inv.properties.Add(i);
            inv.DoSetup();
            return inv;
        }

        [Test]
        public void CanAddItemHook_Fires()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            
            int fireCount = 0;
            inventory.HookCanAddItem((ItemStack final, ReadOnlyItemStack original, object cause) =>
            {
                fireCount++;
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inventory.Count(item));
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void CanAddItemHook_RejectsItem()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();

            inventory.HookCanAddItem((ItemStack final, ReadOnlyItemStack original, object cause) => QueryEventResult.Deny, int.MaxValue);
            
            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.Zero(inventory.Count(item));
        }

        [Test]
        public void CanAddItemHook_AlterItem()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();

            inventory.HookCanAddItem((ItemStack final, ReadOnlyItemStack original, object cause) =>
            {
                final.quantity++;
                return QueryEventResult.Allow;
            }, int.MaxValue);
            
            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.AreEqual(2, inventory.Count(item));
        }

        [Test]
        public void CanSlotAcceptHook_Fires()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            
            inventory.AddItem(item, null);

            int fireCount = 0;
            inventory.HookCanSlotAccept((ReadOnlyInventorySlot slot, ReadOnlyItemStack stack, object cause) =>
            {
                fireCount++;
                return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.NotZero(inventory.Count(item));
            Assert.NotZero(fireCount);
        }

        [Test]
        public void CanSlotAcceptHook_PreventStacking()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();

            inventory.AddItem(item, null);

            bool fired = false;
            inventory.HookCanSlotAccept((ReadOnlyInventorySlot slot, ReadOnlyItemStack stack, object cause) =>
            {
                //Reject first attempt to merge
                if (!fired)
                {
                    fired = true;
                    return QueryEventResult.Deny;
                }
                else return QueryEventResult.Allow;
            }, int.MaxValue);

            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.AreEqual(2, inventory.Count(item)); //There will be two items
            Assert.AreEqual(2, inventory.GetContents().Count(i => i != null)); //They will be split across two slots
        }

        [Test]
        public void PostAddItemHook_Fires()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            
            int fireCount = 0;
            inventory.HookPostAddItem((ItemStack stack, object cause) =>
            {
                fireCount++;
                return PostEventResult.Continue;
            }, int.MaxValue);

            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inventory.Count(item));
            Assert.AreEqual(1, fireCount);
        }

        [Test]
        public void PostAddItemHook_CanRerun()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            
            int fireCount = 0;
            inventory.HookPostAddItem((ItemStack stack, object cause) =>
            {
                fireCount++;
                return fireCount == 1 ? PostEventResult.Retry : PostEventResult.Continue;
            }, int.MaxValue);

            // Act
            inventory.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inventory.Count(item));
            Assert.AreEqual(2, fireCount);
        }
    }

}