using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public sealed class MaxStackSizeTests
    {
        private Inventory CreateInventory()
        {
            Inventory inv = new StandardInventory(30);
            inv.DoSetup();
            return inv;
        }

        [Test, Combinatorial]
        public void LimitsSize([Values(0, 1, 5)] int initialAmt, [Values(1, 2, 5, 10)] int numToAdd, [Values(1, 2, 5)] int maxStackSize)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            item.Properties.Add<MaxStackSize>().size = maxStackSize;
            if (initialAmt != 0) inventory.AddItem(item, initialAmt, null);

            // Act
            inventory.AddItem(item, numToAdd, null);

            // Assert
            Assert.AreEqual(initialAmt+numToAdd, inventory.Count(item), "Correct number of items were added");
            int expectedNumStacks = Mathf.CeilToInt((initialAmt+numToAdd) / (float)maxStackSize);
            Assert.AreEqual(expectedNumStacks, inventory.FindAll(item).Count(), "Items were divided into expected number of stacks");
            foreach (ReadOnlyItemStack i in inventory.FindAll(item)) Assert.LessOrEqual(i.quantity, maxStackSize, "Stacks were correctly limited");
        }
    }

}