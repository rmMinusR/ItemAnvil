using NUnit.Framework;
using rmMinusR.ItemAnvil.Hooks;
using System;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Tests
{

    [TestFixture]
    public class HookPointTests
    {
        [Test]
        public void SingleHook_Fires()
        {
            // Arrange
            HookPoint<Action> hooks = new HookPoint<Action>();
            int fireCount = 0;
            hooks.InsertHook(() => fireCount++, 0);

            // Act
            hooks.Process(h => h());
            
            // Assert
            Assert.AreEqual(fireCount, 1);
        }

        [Test]
        public void RemovedHook_DoesNotFire()
        {
            // Arrange
            HookPoint<Action> hooks = new HookPoint<Action>();
            int fireCount = 0;
            Action hook = () => fireCount++;
            hooks.InsertHook(hook, 0);
            hooks.RemoveHook(hook);

            // Act
            hooks.Process(h => h());

            // Assert
            Assert.AreEqual(fireCount, 0);
        }

        [Test]
        public void MultipleHooks_FireInOrder()
        {
            // Arrange
            HookPoint<Action> hooks = new HookPoint<Action>();
            List<int> canary = new List<int>();
            hooks.InsertHook(() => canary.Add(1), 1);
            hooks.InsertHook(() => canary.Add(2), 2);
            hooks.InsertHook(() => canary.Add(0), 0);
            hooks.InsertHook(() => canary.Add(-1), -1);

            // Act
            hooks.Process(h => h());

            // Assert
            Assert.AreEqual(canary.Count, 4);
            Assert.AreEqual(-1, canary[0]);
            Assert.AreEqual(0, canary[1]);
            Assert.AreEqual(1, canary[2]);
            Assert.AreEqual(2, canary[3]);
        }

        [Test]
        public void Process_QueryEventResult_Allow_Continues()
        {
            // Arrange
            HookPoint<Func<QueryEventResult>> hooks = new HookPoint<Func<QueryEventResult>>();
            hooks.InsertHook(() => QueryEventResult.Allow, 0);
            int canary = 0;
            hooks.InsertHook(() => {
                canary++;
                return QueryEventResult.Allow;
            }, 1);

            // Act
            QueryEventResult res = hooks.Process(h => h());

            // Assert
            Assert.AreEqual(QueryEventResult.Allow, res);
            Assert.AreEqual(1, canary);
        }

        [Test]
        public void Process_QueryEventResult_Deny_Interrupts()
        {
            // Arrange
            HookPoint<Func<QueryEventResult>> hooks = new HookPoint<Func<QueryEventResult>>();
            hooks.InsertHook(() => QueryEventResult.Deny, 0);
            int canary = 0;
            hooks.InsertHook(() => {
                canary++;
                return QueryEventResult.Allow;
            }, 1);

            // Act
            QueryEventResult res = hooks.Process(h => h());

            // Assert
            Assert.AreEqual(QueryEventResult.Deny, res);
            Assert.AreEqual(0, canary);
        }

        [Test]
        public void Process_PostEventResult_Continue_Continues()
        {
            // Arrange
            HookPoint<Func<PostEventResult>> hooks = new HookPoint<Func<PostEventResult>>();
            hooks.InsertHook(() => PostEventResult.Continue, 0);
            int canary = 0;
            hooks.InsertHook(() => {
                canary++;
                return PostEventResult.Continue;
            }, 1);

            // Act
            PostEventResult res = hooks.Process(h => h());

            // Assert
            Assert.AreEqual(PostEventResult.Continue, res);
            Assert.AreEqual(1, canary);
        }

        [Test]
        public void Process_PostEventResult_Retry_Interrupts()
        {
            // Arrange
            HookPoint<Func<PostEventResult>> hooks = new HookPoint<Func<PostEventResult>>();
            hooks.InsertHook(() => PostEventResult.Retry, 0);
            int canary = 0;
            hooks.InsertHook(() => {
                canary++;
                return PostEventResult.Continue;
            }, 1);

            // Act
            PostEventResult res = hooks.Process(h => h());

            // Assert
            Assert.AreEqual(PostEventResult.Retry, res);
            Assert.AreEqual(0, canary);
        }
    }

}