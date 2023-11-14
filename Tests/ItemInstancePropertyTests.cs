using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class ItemInstancePropertyTests
    {
        private Inventory CreateInventory()
        {
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();
            return inv;
        }

        private class TickCanaryProp : ItemInstanceProperty
        {
            public int timesTicked = 0;

            public override void Tick(InventorySlot slot, Inventory inventory, Component @object)
            {
                timesTicked++;
            }
        }

        private class DummyProp : ItemInstanceProperty { }


        [Test]
        public void TickPropagates()
        {
            // Arrange
            Inventory inv = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            {
                ItemStack stack = new ItemStack(item);
                stack.instanceProperties.Add<TickCanaryProp>();
                stack.instanceProperties.Add<DummyProp>();
                inv.AddItem(stack, null);
            }

            // Act
            InventoryHolder holder = new InventoryHolder();
            holder.inventory = inv;
            inv.Tick(holder);

            // Assert
            Assert.AreEqual(1, inv.GetSlot(0).Contents.instanceProperties.Get<TickCanaryProp>().timesTicked);
        }

        [Test]
        public void Equals_SameReference_IsTrue()
        {
            // Arrange
            TickCanaryProp prop = new TickCanaryProp();

            // Act
            bool res = prop.Equals(prop);

            // Assert
            Assert.IsTrue(res);
        }

        [Test]
        public void Equals_NullReference_IsFalse()
        {
            // Arrange
            TickCanaryProp prop = new TickCanaryProp();

            // Act
            bool res = prop.Equals(null);

            // Assert
            Assert.IsFalse(res);
        }

        [Test]
        public void Equals_SameFieldValues_IsTrue()
        {
            // Arrange
            TickCanaryProp propA = new TickCanaryProp();
            propA.timesTicked = 1;
            TickCanaryProp propB = new TickCanaryProp();
            propB.timesTicked = 1;

            // Act
            bool res = propA.Equals(propB);

            // Assert
            Assert.IsTrue(res);
        }

        [Test]
        public void Equals_DifferentFieldValues_IsFalse()
        {
            // Arrange
            TickCanaryProp propA = new TickCanaryProp();
            propA.timesTicked = 1;
            TickCanaryProp propB = new TickCanaryProp();
            propB.timesTicked = 2;

            // Act
            bool res = propA.Equals(propB);

            // Assert
            Assert.IsFalse(res);
        }

        [Test]
        public void HashCode_SameReference_Equal()
        {
            // Arrange
            TickCanaryProp prop = new TickCanaryProp();

            // Assert
            Assert.AreEqual(prop.GetHashCode(), prop.GetHashCode());
        }

        [Test]
        public void HashCode_SameFieldValues_Equal()
        {
            // Arrange
            TickCanaryProp propA = new TickCanaryProp();
            propA.timesTicked = 1;
            TickCanaryProp propB = new TickCanaryProp();
            propB.timesTicked = 1;

            // Assert
            Assert.AreEqual(propA.GetHashCode(), propB.GetHashCode());
        }

        [Test]
        public void HashCode_DifferentFieldValues_NotEqual()
        {
            // Arrange
            TickCanaryProp propA = new TickCanaryProp();
            propA.timesTicked = 1;
            TickCanaryProp propB = new TickCanaryProp();
            propB.timesTicked = 2;

            // Assert
            Assert.AreNotEqual(propA.GetHashCode(), propB.GetHashCode());
        }
    }

}