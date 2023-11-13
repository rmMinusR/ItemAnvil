using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using System;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class AggregateHookPointTests
    {
        [Test]
        public void MultipleHooks_FireInOrder()
        {
            // Arrange
            HookPoint<Action> hooksA = new HookPoint<Action>();
            HookPoint<Action> hooksB = new HookPoint<Action>();
            List<int> canary = new List<int>();
            hooksA.InsertHook(() => canary.Add(-1), -1);
            hooksB.InsertHook(() => canary.Add(0), 0);
            hooksB.InsertHook(() => canary.Add(1), 1);
            hooksA.InsertHook(() => canary.Add(2), 2);
            hooksB.InsertHook(() => canary.Add(3), 3);

            // Act
            HookPoint<Action>.Aggregate(hooksA, hooksB).Process(h => h());

            // Assert
            Assert.AreEqual(canary.Count, 5);
            Assert.AreEqual(-1, canary[0]);
            Assert.AreEqual(0, canary[1]);
            Assert.AreEqual(1, canary[2]);
            Assert.AreEqual(2, canary[3]);
            Assert.AreEqual(3, canary[4]);
        }

        [Test]
        public void DuplicateSources_FireOnce()
        {
            // Arrange
            HookPoint<Action> hooksA = new HookPoint<Action>();
            HookPoint<Action> hooksB = new HookPoint<Action>();
            List<int> canary = new List<int>();
            hooksB.InsertHook(() => canary.Add(0), 0);
            hooksA.InsertHook(() => canary.Add(1), 1);

            // Act
            HookPoint<Action>.Aggregate(hooksA, hooksA, hooksB).Process(h => h());

            // Assert
            Assert.AreEqual(canary.Count, 2);
            Assert.AreEqual(0, canary[0]);
            Assert.AreEqual(1, canary[1]);
        }
    }

}