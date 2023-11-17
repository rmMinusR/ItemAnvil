using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class StandardInventoryTests : InventoryTests
    {
        protected override Inventory _CreateInventory() => new StandardInventory(30);

        [Test]
        public void AddItem_Trivial_NoSpace_Fails()
        {
            // Arrange
            Inventory inventory = new StandardInventory(0);
            inventory.DoSetup();
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack testStack = new ItemStack(item);
            inventory.AddItem(testStack, null);

            // Assert
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(0, inventory.SlotCount);
            Assert.AreEqual(1, testStack.quantity);
        }

        [Test]
        public void AddItem_Trivial_NotEnoughSpace_Fails()
        {
            // Arrange
            Inventory inventory = new StandardInventory(1);
            inventory.DoSetup();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(itemA, 1, null);

            // Act
            ItemStack testStack = new ItemStack(itemB);
            inventory.AddItem(testStack, null);

            // Assert
            Assert.AreEqual(0, inventory.Count(itemB));
            Assert.AreEqual(1, inventory.SlotCount);
            Assert.AreEqual(1, testStack.quantity);
        }

        [Test]
        public void AddItem_PreFilled_NoSlots_Overflows()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item blocker = ScriptableObject.CreateInstance<Item>();
            foreach (InventorySlot s in inventory.Slots) s.Contents = new ItemStack(blocker); //Pre-fill
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack stack = new ItemStack(item);
            inventory.AddItem(stack, null);

            // Assert
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(1, stack.quantity);
        }
        
        [Test, Combinatorial]
        public void AddItem_PreFilled_NotEnoughSlots_PartiallyOverflows([Values(1, 2, 5, 10)] int additionalToAdd, [Values(1, 2, 5)] int stackLimit)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item blocker = ScriptableObject.CreateInstance<Item>();
            for (int i = 0; i < inventory.SlotCount-1; ++i) inventory.GetSlot(i).Contents = new ItemStack(blocker); //Pre-fill
            Item item = ScriptableObject.CreateInstance<Item>();
            item.Properties.Add<MaxStackSize>().size = stackLimit;

            // Act
            ItemStack stack = new ItemStack(item, stackLimit+additionalToAdd);
            inventory.AddItem(stack, null);

            // Assert
            Assert.AreEqual(stackLimit, inventory.Count(item));
            Assert.AreEqual(additionalToAdd, stack.quantity);
        }

        [Test]
        public void TryAddItem_Trivial_HasSpace_Succeeds()
        {
            // Arrange
            Inventory inventory = new StandardInventory(1);
            inventory.DoSetup();
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack testStack = new ItemStack(item);
            bool ret = inventory.TryAddItem(testStack, null);

            // Assert
            Assert.IsTrue(ret);
            Assert.AreEqual(1, inventory.Count(item));
            Assert.AreEqual(0, testStack.quantity);
        }

        [Test]
        public void TryAddItem_Trivial_NoSpace_Fails()
        {
            // Arrange
            Inventory inventory = new StandardInventory(0);
            inventory.DoSetup();
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack testStack = new ItemStack(item);
            bool ret = inventory.TryAddItem(testStack, null);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(1, testStack.quantity);
        }

        [Test]
        public void TryAddItem_PreFilled_NoSpace_Fails()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item blocker = ScriptableObject.CreateInstance<Item>();
            foreach (InventorySlot s in inventory.Slots) s.Contents = new ItemStack(blocker); //Pre-fill
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack stack = new ItemStack(item);
            bool ret = inventory.TryAddItem(stack, null);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(1, stack.quantity);
        }

        [Test, Combinatorial]
        public void TryAddItem_PreFilled_NotEnoughSlots_Fails([Values(1, 2, 5, 10)] int additionalToAdd, [Values(1, 2, 5)] int stackLimit)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item blocker = ScriptableObject.CreateInstance<Item>();
            for (int i = 0; i < inventory.SlotCount - 1; ++i) inventory.GetSlot(i).Contents = new ItemStack(blocker); //Pre-fill
            Item item = ScriptableObject.CreateInstance<Item>();
            item.Properties.Add<MaxStackSize>().size = stackLimit;

            // Act
            ItemStack stack = new ItemStack(item, stackLimit + additionalToAdd);
            bool ret = inventory.TryAddItem(stack, null);

            // Assert
            Assert.IsFalse(ret);
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(stackLimit + additionalToAdd, stack.quantity);
        }
    }

    [TestFixture]
    public class CondensingInventoryTests : InventoryTests
    {
        protected override Inventory _CreateInventory() => new CondensingInventory();
        
        private class DummyInstanceProp : ItemInstanceProperty { }

        [Test]
        public void Upgrader_HasParity()
        {
            // Arrange
            List<ItemStack> source = new List<ItemStack>();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            itemB.Properties.Add<MaxStackSize>().size = 2;

            Inventory inventory = CreateInventory();

            source.Add(new ItemStack(itemA, 1));
            source.Add(new ItemStack(itemB, 5));
            ItemStack stack = new ItemStack(itemA, 2);
            stack.instanceProperties.Add<DummyInstanceProp>();
            source.Add(stack);
            source.Add(new ItemStack(itemA, 0));
            source.Add(new ItemStack(null, 0));
            while (source.Count < inventory.SlotCount) source.Add(null);

            // Act
            inventory.GetType()
                     .GetField("contents", BindingFlags.NonPublic | BindingFlags.Instance)
                     .SetValue(inventory, source.Select(i => i?.Clone()).ToList());
            inventory.Validate();

            // Assert
            Assert.AreEqual(itemA, inventory.GetSlot(0).Contents.itemType);
            Assert.AreEqual(1    , inventory.GetSlot(0).Contents.quantity);
            Assert.AreEqual(0    , inventory.GetSlot(0).Contents.instanceProperties.Count);

            Assert.AreEqual(itemB, inventory.GetSlot(1).Contents.itemType);
            Assert.AreEqual(5    , inventory.GetSlot(1).Contents.quantity);
            Assert.AreEqual(0    , inventory.GetSlot(1).Contents.instanceProperties.Count);

            Assert.AreEqual(itemA, inventory.GetSlot(2).Contents.itemType);
            Assert.AreEqual(2    , inventory.GetSlot(2).Contents.quantity);
            Assert.IsTrue  (       inventory.GetSlot(2).Contents.instanceProperties.Contains<DummyInstanceProp>());
        }
    }

    [TestFixture]
    public class FixedInventoryTests : InventoryTests
    {
        protected override Inventory _CreateInventory() => new FixedSlotInventory(30);
        
        private class DummyInstanceProp : ItemInstanceProperty { }

        [Test]
        public void Upgrader_HasParity()
        {
            // Arrange
            List<ItemStack> source = new List<ItemStack>();
            Item itemA = ScriptableObject.CreateInstance<Item>();
            Item itemB = ScriptableObject.CreateInstance<Item>();
            itemB.Properties.Add<MaxStackSize>().size = 2;

            Inventory inventory = CreateInventory();

            source.Add(new ItemStack(itemA, 1));
            source.Add(new ItemStack(itemB, 5));
            ItemStack stack = new ItemStack(itemA, 2);
            stack.instanceProperties.Add<DummyInstanceProp>();
            source.Add(stack);
            source.Add(new ItemStack(itemA, 0));
            source.Add(new ItemStack(null, 0));
            while (source.Count < inventory.SlotCount) source.Add(null);

            // Act
            inventory.GetType()
                     .GetField("contents", BindingFlags.NonPublic | BindingFlags.Instance)
                     .SetValue(inventory, source.Select(i => i?.Clone()).ToList());
            inventory.Validate();

            // Assert
            Assert.AreEqual(itemA, inventory.GetSlot(0).Contents.itemType);
            Assert.AreEqual(1    , inventory.GetSlot(0).Contents.quantity);
            Assert.AreEqual(0    , inventory.GetSlot(0).Contents.instanceProperties.Count);

            Assert.AreEqual(itemB, inventory.GetSlot(1).Contents.itemType);
            Assert.AreEqual(5    , inventory.GetSlot(1).Contents.quantity);
            Assert.AreEqual(0    , inventory.GetSlot(1).Contents.instanceProperties.Count);

            Assert.AreEqual(itemA, inventory.GetSlot(2).Contents.itemType);
            Assert.AreEqual(2    , inventory.GetSlot(2).Contents.quantity);
            Assert.IsTrue  (       inventory.GetSlot(2).Contents.instanceProperties.Contains<DummyInstanceProp>());
        }
    }

    public abstract class InventoryTests
    {
        protected abstract Inventory _CreateInventory();
        protected Inventory CreateInventory()
        {
            Inventory inv = _CreateInventory();
            inv.DoSetup();
            return inv;
        }

        [Test, Combinatorial]
        public void AddItem_AddsItemToInventory([Values(1, 2, 5, 10)] int nToAdd)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            inventory.AddItem(item, nToAdd, null);

            // Assert
            Assert.AreEqual(nToAdd, inventory.Count(item));
        }

        [Test, Combinatorial]
        public void AddItemStack_AddsItemStackToInventory([Values(1, 2, 5, 10)] int nToAdd)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();

            // Act
            ItemStack stack = new ItemStack(item, nToAdd);
            inventory.AddItem(stack, null);

            // Assert
            Assert.AreEqual(nToAdd, inventory.Count(item));
            Assert.AreEqual(0, stack.quantity);
        }

        [Test, Combinatorial]
        public void TryRemove_RemovesItemsFromInventory([Values(1, 2, 5, 10)] int startingCount, [Values(1, 2, 5, 10)] int nToRemove)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(item, startingCount, null);

            // Act
            IEnumerable<ItemStack> removedStacks = removedStacks = inventory.TryRemove(item, nToRemove, null);

            // Assert
            if (startingCount >= nToRemove)
            {
                // Had enough items
                Assert.IsNotNull(removedStacks);
                Assert.AreEqual(startingCount-nToRemove, inventory.Count(item));
                Assert.AreEqual(nToRemove, removedStacks.Sum(stack => stack.quantity));
            }
            else
            {
                // Didn't have enough items
                Assert.IsNull(removedStacks);
                Assert.AreEqual(startingCount, inventory.Count(item));
            }
        }

        [Test, Combinatorial]
        public void RemoveAll_RemovesAllItemsOfTypeFromInventory([Values(1, 2, 5, 10)] int count)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(item, count, null);

            // Act
            int removedCount = inventory.RemoveAll(item, null);

            // Assert
            Assert.AreEqual(0, inventory.Count(item));
            Assert.AreEqual(count, removedCount);
        }

        [Test, Combinatorial]
        public void Count_ReturnsCorrectItemCount([Values(1, 2, 5, 10)] int realCount1, [Values(1, 2, 5, 10)] int realCount2)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item1 = ScriptableObject.CreateInstance<Item>();
            Item item2 = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(item1, realCount1, null);
            inventory.AddItem(item2, realCount2, null);

            // Act
            int observedCount1 = inventory.Count(item1);
            int observedCount2 = inventory.Count(item2);

            // Assert
            Assert.AreEqual(realCount1, observedCount1);
            Assert.AreEqual(realCount2, observedCount2);
        }

        [Test, Combinatorial]
        public void GetContents_ReturnsCorrectItemStacks([Values(1, 2, 5, 10)] int count1, [Values(1, 2, 5, 10)] int count2)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item1 = ScriptableObject.CreateInstance<Item>();
            Item item2 = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(item1, count1, null);
            inventory.AddItem(item2, count2, null);

            // Act
            IEnumerable<ReadOnlyItemStack> contents = inventory.GetContents();

            // Assert
            Assert.AreEqual(2, contents.Count());
            Assert.IsTrue(contents.Any(stack => stack.itemType == item1 && stack.quantity == count1));
            Assert.IsTrue(contents.Any(stack => stack.itemType == item2 && stack.quantity == count2));
        }

        [Test, Combinatorial]
        public void FindFirst_ReturnsCorrectItemStack(
            [Values(1, 2, 5, 10)] int correctItemCount,
            [Values(1, 2, 3, 5, 7, 11, 13)] int correctItemStackIndex,
            [Values(0, 1, 2, 5, 10)] int confuserCount)
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item correct = ScriptableObject.CreateInstance<Item>();
            Item confuser = ScriptableObject.CreateInstance<Item>();
            correctItemStackIndex %= confuserCount+1;
            for (int i = 0; i < confuserCount+1; ++i)
            {
                if (i == correctItemStackIndex) inventory.AddItem(correct, correctItemCount, null);
                else                            inventory.AddItem(confuser, 1, null);
            }

            // Act
            ItemStack stack = inventory.FindFirst(correct);

            // Assert
            Assert.IsNotNull(stack);
            Assert.AreEqual(correct, stack.itemType);
            Assert.AreEqual(correctItemCount, stack.quantity);
        }

        [Test]
        public void CloneContents_ReturnsDeepCopyOfContents()
        {
            // Arrange
            Inventory inventory = CreateInventory();
            Item item = ScriptableObject.CreateInstance<Item>();
            inventory.AddItem(item, 5, null);

            // Act
            List<ItemStack> clonedContents = inventory.CloneContents();

            // Modify the cloned contents
            clonedContents[0].quantity = 10;

            // Assert
            Assert.AreEqual(5, inventory.Count(item));
        }
    }

}