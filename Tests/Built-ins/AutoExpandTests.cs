using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class AutoExpandTests
    {
        #region Adding items

        [Test]
        public void EmptyInventory_NoSpace_Expands()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(0);
            inv.DoSetup();
            inv.AddProperty<AutoExpand>();

            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            inv.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inv.SlotCount);
        }

        [Test]
        public void EmptyInventory_SomeSpace_DoesNotExpand()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(1);
            inv.DoSetup();
            inv.AddProperty<AutoExpand>();

            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            inv.AddItem(item, null);

            // Assert
            Assert.AreEqual(1, inv.SlotCount);
        }

        [Test]
        public void PartiallyFullInventory_SomeSpace_DoesNotExpand()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(2);
            inv.DoSetup();

            Item itemA = ScriptableObject.CreateInstance<Item>();
            inv.AddItem(itemA, null);
            inv.AddProperty<AutoExpand>();

            Item itemB = ScriptableObject.CreateInstance<Item>();

            // Act
            inv.AddItem(itemB, null);

            // Assert
            Assert.AreEqual(2, inv.SlotCount);
        }

        [Test]
        public void FullInventory_NoSpace_Expands()
        {
            // Arrange
            StandardInventory inv = new StandardInventory(1);
            inv.DoSetup();

            Item itemA = ScriptableObject.CreateInstance<Item>();
            inv.AddItem(itemA, null);
            inv.AddProperty<AutoExpand>();

            Item itemB = ScriptableObject.CreateInstance<Item>();

            // Act
            inv.AddItem(itemB, null);

            // Assert
            Assert.AreEqual(2, inv.SlotCount);
        }

        #endregion

        #region Removing items

        [Test, Combinatorial]
        public void Removing_Condenses_Pure([Values(1, 2, 3)] int nToAdd)
        {
            // Arrange
            StandardInventory inv = new StandardInventory(0);
            inv.DoSetup();
            inv.AddProperty<AutoExpand>();

            Item item = ScriptableObject.CreateInstance<Item>();
            item.Properties.Add<MaxStackSize>().size = 1;
            inv.AddItem(item, nToAdd, null);

            // Act
            inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(0, inv.SlotCount);
        }

        [Test, Combinatorial]
        public void Removing_Condenses_Messy([Values(1, 2, 3)] int nToAdd, [Values(false, true)] bool useConfuser)
        {
            // Arrange
            StandardInventory inv = new StandardInventory(0);
            inv.DoSetup();
            inv.AddProperty<AutoExpand>();

            Item item = ScriptableObject.CreateInstance<Item>();
            item.Properties.Add<MaxStackSize>().size = 1;

            Item confuser = ScriptableObject.CreateInstance<Item>();
            
            inv.AddItem(item, nToAdd, null);
            if (useConfuser) inv.AddItem(confuser, null);

            // Act
            inv.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(useConfuser?1:0, inv.SlotCount);
        }

        #endregion
    }

}
