using System;
using System.Collections.Generic;

namespace rmMinusR.ItemAnvil.Hooks
{
    /// <summary>
    /// Helper class to manage hooks
    /// </summary>
    /// <typeparam name="THook">Hook type</typeparam>
    public sealed class HookPoint<THook> where THook : class
    {
        private struct HookContainer<T> where T : class
        {
            public T hook;
            public int priority;
        }

        private List<HookContainer<THook>> hooks = new List<HookContainer<THook>>();

        public void InsertHook(THook hook, int priority)
        {
            HookContainer<THook> container = new HookContainer<THook>()
            {
                hook = hook,
                priority = priority
            };

            if (hooks.Count == 0) hooks.Add(container);
            else
            {
                int insertIndex = hooks.FindIndex(x => x.priority >= priority);
                if (insertIndex != -1) hooks.Insert(insertIndex, container); //There exist hooks that should execute after this one. Insert before them.
                else hooks.Add(container); //This hooks should execute after all others. Append.
            }
        }

        public void RemoveHook(THook hook)
        {
            hooks.RemoveAll(i => i.hook == hook);
        }

        #region Hook processing

        // Func is a lambda passing variables to individual hooks (thunk). Can be thought of like a visitor, except the ones that return EventResults are allwoed to halt early.

        public void Process(Action<THook> func)
        {
            foreach (HookContainer<THook> c in hooks) func(c.hook);
        }

        public QueryEventResult Process(Func<THook, QueryEventResult> func)
        {
            QueryEventResult res = QueryEventResult.Allow;
            foreach (HookContainer<THook> c in hooks)
            {
                res = func(c.hook);
                if (res != QueryEventResult.Allow) break;
            }
            return res;
        }

        public PostEventResult Process(Func<THook, PostEventResult> func)
        {
            PostEventResult res = PostEventResult.Continue;
            foreach (HookContainer<THook> c in hooks)
            {
                res = func(c.hook);
                if (res != PostEventResult.Continue) break;
            }
            return res;
        }

        #endregion
    }
}
