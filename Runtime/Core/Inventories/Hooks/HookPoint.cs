using System;
using System.Collections.Generic;
using UnityEditor;
using static UnityEngine.UI.Image;
using UnityEditor.PackageManager.UI;
using System.Linq;

namespace rmMinusR.ItemAnvil.Hooks
{
    #region Helper classes

    public abstract class IExecuteOnlyHookPoint<THook> where THook : class
    {
        public struct OrderedHook
        {
            public THook hook;
            public int execOrder;
            public static implicit operator THook(OrderedHook h) => h.hook;
        }
        public abstract IEnumerable<OrderedHook> GetHooks();

        #region Hook processing

        // Func is a lambda passing variables to individual hooks (cross between a thunk and bind-expression).
        // For example: CanAddItem.Process(hook => hook(final, original, cause));
        // Can be thought of like a visitor, except the ones that return EventResults are allowed to halt early.
        public virtual void Process(Action<THook> func)
        {
            foreach (THook h in GetHooks()) func(h);
        }

        public virtual QueryEventResult Process(Func<THook, QueryEventResult> func)
        {
            QueryEventResult res = QueryEventResult.Allow;
            foreach (THook h in GetHooks())
            {
                res = func(h);
                if (res != QueryEventResult.Allow) break;
            }
            return res;
        }

        public virtual PostEventResult Process(Func<THook, PostEventResult> func)
        {
            PostEventResult res = PostEventResult.Continue;
            foreach (THook h in GetHooks())
            {
                res = func(h);
                if (res != PostEventResult.Continue) break;
            }
            return res;
        }

        #endregion
    }

    public abstract class IHookPoint<THook> : IExecuteOnlyHookPoint<THook> where THook : class
    {
        public abstract void InsertHook(THook hook, int execOrder);
        public abstract void InsertFinalizer(THook hook);
        public abstract void RemoveHook(THook hook);


        //Utility shorthand
        public static IExecuteOnlyHookPoint<THook> Aggregate(params IHookPoint<THook>[] hookPoints)
        {
            return new AggregateHookPoint<THook>(hookPoints);
        }
    }

    #endregion

    /// <summary>
    /// Helper class to manage hooks.
    /// </summary>
    /// <remarks>
    /// Hooks execute in ascending priority. For events that only listen
    /// for a final result without modifying behavior (such as UI),
    /// priority is int.MaxValue.
    /// </remarks>
    /// <typeparam name="THook">Hook type</typeparam>
    public sealed class HookPoint<THook> : IHookPoint<THook> where THook : class
    {
        private List<OrderedHook> hooks = new List<OrderedHook>();
        public override IEnumerable<OrderedHook> GetHooks() => hooks;

        public override void InsertFinalizer(THook hook) => InsertHook(hook, int.MaxValue);

        public override void InsertHook(THook hook, int execOrder)
        {
            OrderedHook container = new OrderedHook()
            {
                hook = hook,
                execOrder = execOrder
            };

            if (hooks.Count == 0) hooks.Add(container);
            else
            {
                int insertIndex = hooks.FindIndex(x => x.execOrder >= execOrder);
                if (insertIndex != -1) hooks.Insert(insertIndex, container); //There exist hooks that should execute after this one. Insert before them.
                else hooks.Add(container); //This hooks should execute after all others. Append.
            }
        }

        public override void RemoveHook(THook hook)
        {
            hooks.RemoveAll(i => i.hook == hook);
        }
    }

    /// <summary>
    /// A lightweight view onto multiple HookPoints, allowing mixing hook scopes (ie. inventory-level vs slot-level) while maintaining correct order by interleaving hooks
    /// </summary>
    /// <typeparam name="THook"></typeparam>
    public sealed class AggregateHookPoint<THook> : IExecuteOnlyHookPoint<THook> where THook : class
    {
        private IHookPoint<THook>[] hookPoints;
        public AggregateHookPoint(IHookPoint<THook>[] hookPoints)
        {
            this.hookPoints = new HashSet<IHookPoint<THook>>(hookPoints).ToArray(); //Deduplicate
        }

        public override IEnumerable<OrderedHook> GetHooks()
        {
            //Effectively a mergesort

            //Prepare merge sources
            IEnumerator<OrderedHook>[] wrapped = new IEnumerator<OrderedHook>[hookPoints.Length];
            for (int i = 0; i < wrapped.Length; i++)
            {
                wrapped[i] = hookPoints[i].GetHooks().GetEnumerator();
                if (!wrapped[i].MoveNext()) wrapped[i] = null;
            }

            while(true)
            {
                //Find next hook to execute
                int? idx = FindMin(wrapped, i => i?.Current.execOrder);
                if (!idx.HasValue) break;

                //Yield hook
                yield return wrapped[idx.Value].Current;

                //Advance iterator
                if (!wrapped[idx.Value].MoveNext()) wrapped[idx.Value] = null;
            }            
        }

        private static int? FindMin<T>(IEnumerable<T> src, Func<T, int?> selector)
        {
            int? outIndex = null;
            int minVal = int.MaxValue;

            int curIndex = 0;
            foreach (T v in src)
            {
                int? eval = selector(v);
                if (eval.HasValue && eval.Value <= minVal)
                {
                    minVal = eval.Value;
                    outIndex = curIndex;
                }
                ++curIndex;
            }

            return outIndex;
        }
    }
}
