using NUnit.Framework;
using System.Security.Permissions;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class FilterSlotContentsTests
    {
        [Test, Combinatorial]
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
    }

}
