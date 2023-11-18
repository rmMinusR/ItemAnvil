using NUnit.Framework;
using System.Security.Permissions;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class FilterSlotContentsTests
    {
        [Test]
        public void AddItem_ItemAllowed_SlotChosen()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(1);
            inv.DoSetup();

            Item item = ScriptableObject.CreateInstance<Item>();
            
            FilterSlotContents filterer = inv.GetSlot(0).AddProperty<FilterSlotContents>();
            filterer.allowedItems = new FilterMatchType { match = item };

            // Act
            inv.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inv.SlotCount);
            Assert.IsFalse(inv.GetSlot(0).IsEmpty);
            Assert.AreEqual(item, inv.GetSlot(0).Contents.itemType);
            Assert.AreEqual(1   , inv.GetSlot(0).Contents.quantity);
        }

        [Test, Combinatorial]
        public void AddItem_ItemNotAllowed_SlotUntouched([Values(false, true)] bool useAutoExpand)
        {
            // Arrange
            StandardInventory inv = new StandardInventory(1);
            inv.DoSetup();
            if (useAutoExpand) inv.AddProperty<AutoExpand>();

            Item item = ScriptableObject.CreateInstance<Item>();
            Item blocker = ScriptableObject.CreateInstance<Item>();

            FilterSlotContents filterer = inv.GetSlot(0).AddProperty<FilterSlotContents>();
            filterer.allowedItems = new FilterMatchType { match = blocker };
            
            // Act
            inv.AddItem(item, null);

            // Assert
            Assert.IsTrue(inv.GetSlot(0).IsEmpty);
            if (useAutoExpand)
            {
                Assert.AreEqual(2, inv.SlotCount);
                Assert.IsFalse(inv.GetSlot(1).IsEmpty);
                Assert.AreEqual(item, inv.GetSlot(1).Contents.itemType);
                Assert.AreEqual(1   , inv.GetSlot(1).Contents.quantity);
            }
        }

        [Test]
        public void SwapSlots_ItemAllowed_Succeeds()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(2);
            inv.DoSetup();

            ItemCategory category = ScriptableObject.CreateInstance<ItemCategory>();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            itemA.categories.Add(category);
            Item itemB = ScriptableObject.CreateInstance<Item>();
            itemB.categories.Add(category);

            inv.AddItem(itemA, null);
            inv.AddItem(itemB, null);

            FilterSlotContents filterer = inv.GetSlot(0).AddProperty<FilterSlotContents>();
            filterer.allowedItems = new FilterMatchCategory { category = category };

            // Act
            bool ret = InventorySlot.SwapContents(inv.GetSlot(0), inv.GetSlot(1), null);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(itemB, inv.GetSlot(0).Contents.itemType);
            Assert.AreEqual(1    , inv.GetSlot(0).Contents.quantity);
            Assert.AreEqual(itemA, inv.GetSlot(1).Contents.itemType);
            Assert.AreEqual(1    , inv.GetSlot(1).Contents.quantity);
        }

        [Test]
        public void SwapSlots_ItemNotAllowed_Fails()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(2);
            inv.DoSetup();

            ItemCategory category = ScriptableObject.CreateInstance<ItemCategory>();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            itemA.categories.Add(category);
            Item itemB = ScriptableObject.CreateInstance<Item>();
            itemB.categories.Add(category);

            inv.AddItem(itemA, null);
            inv.AddItem(itemB, null);

            FilterSlotContents filterer = inv.GetSlot(0).AddProperty<FilterSlotContents>();
            filterer.allowedItems = new FilterMatchType { match = itemA };

            // Act
            bool ret = InventorySlot.SwapContents(inv.GetSlot(0), inv.GetSlot(1), null);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(itemA, inv.GetSlot(0).Contents.itemType);
            Assert.AreEqual(1    , inv.GetSlot(0).Contents.quantity);
            Assert.AreEqual(itemB, inv.GetSlot(1).Contents.itemType);
            Assert.AreEqual(1    , inv.GetSlot(1).Contents.quantity);
        }
    }

}
