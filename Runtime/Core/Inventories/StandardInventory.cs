using rmMinusR.ItemAnvil.Hooks;
using rmMinusR.ItemAnvil.Hooks.Inventory;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// A generic inventory that can be customized via InventoryProperties and hooks.
    /// </summary>
    [Serializable]
    public class StandardInventory : Inventory
    {
        [SerializeField] protected List<InventorySlot> slots = new List<InventorySlot>();
        public override IEnumerable<InventorySlot> Slots => slots;
        public override InventorySlot GetSlot(int id) => slots[id];
        public override int SlotCount => slots.Count;

        #region Inventory-scoped properties

        [SerializeField] private PropertyBag<InventoryProperty> _properties = new PropertyBag<InventoryProperty>();
        public ReadOnlyPropertyBag<InventoryProperty> Properties => _properties;

        public T AddProperty<T>() where T : InventoryProperty, new()
        {
            T prop = _properties.Add<T>();
            prop._InstallHooks(this);
            return prop;
        }

        public void AddProperty(InventoryProperty prop)
        {
            _properties.Add(prop);
            prop._InstallHooks(this);
        }

        #endregion

        public StandardInventory() { }
        public StandardInventory(int size) : this()
        {
            for (int i = 0; i < size; ++i) AppendSlot();
        }

        protected internal virtual InventorySlot AppendSlot()
        {
            InventorySlot s = new InventorySlot(slots.Count, this);
            slots.Add(s);
            return s;
        }

        protected internal virtual void Condense()
        {
            //Remove empty slots
            for (int i = slots.Count-1; i >= 0; --i)
            {
                if (slots[i].IsEmpty) slots.RemoveAt(i);
            }

            //Fix IDs
            Validate();
        }

        /// <summary>
        /// Add an item using an ItemStack
        /// </summary>
        /// <param name="newStack">Stack to add</param>
        /// <param name="cause"></param>
        public override void AddItem(ItemStack newStack, object cause)
        {
            if (ItemStack.IsEmpty(newStack)) throw new ArgumentException("Cannot add nothing!");

            //Check hooks to see if we're allowed to add this item
            if (Hooks.CanAddItem.Process(h => h(newStack, newStack.Clone(), cause)) != QueryEventResult.Allow) return;

        retry: //NOTE: Not great performance, but re-run both for loops just in case

            //First try to merge with existing stacks
            foreach (InventorySlot slot in slots)
            {
                //Check hooks to see if slot can accept this stack
                if (!slot.IsEmpty) slot.TryAccept(newStack, cause);
                if (newStack.quantity == 0) goto done;
            }

            //Then try to merge into empty slots
            foreach (InventorySlot slot in slots)
            {
                //Check hooks to see if slot can accept this stack
                if (slot.IsEmpty) slot.TryAccept(newStack, cause);
                if (newStack.quantity == 0) goto done;
            }

        done:
            //Run post hook. Handles stuff like overflow.
            if (Hooks.PostAddItem.Process(h => h(newStack, cause)) == PostEventResult.Retry) goto retry;
        }

        /// <summary>
        /// Dump the contents of this inventory. Note that these the original instances.
        /// </summary>
        public override IEnumerable<ReadOnlyItemStack> GetContents()
        {
            return slots.Where(i => !i.IsEmpty).Select(i => i.Contents);
        }

        /// <summary>
        /// Make a deep clone of the contents of this inventory, which may be manipulated freely without affecting the inventory
        /// </summary>
        public override List<ItemStack> CloneContents()
        {
            return slots.Where(i => !i.IsEmpty).Select(i => i.Contents.Clone()).ToList();
        }

        /// <summary>
        /// Attempt to remove items. If not enough are available, no changes will be made.
        /// </summary>
        /// <param name="typeToRemove">Item type to be removed</param>
        /// <param name="totalToRemove">How many to be removed</param>
        /// <param name="cause">User-defined data to provide additional context</param>
        /// <returns>If enough items were present, an IEnumerable of those items. Otherwise null, and no changes were made.</returns>
        public override IEnumerable<ItemStack> TryRemove(Predicate<ItemStack> filter, int totalToRemove, object cause)
        {
            //Item removal routine
            List<(ItemStack, InventorySlot)> everythingRemoved = new List<(ItemStack, InventorySlot)>();
            for (int i = 0; i < slots.Count; ++i)
            {
                if (!slots[i].IsEmpty && filter(slots[i].Contents))
                {
                    //Create dummy removedStack
                    ItemStack removedStack = slots[i].Contents.Clone();
                    removedStack.quantity = Mathf.Min(totalToRemove, slots[i].Contents.quantity);

                    //Check hook to see if we can continue, then check to make sure no hooks are up to funny business
                    if (Hooks.TryRemoveItem.Process(h => h(slots[i], removedStack, removedStack.Clone(), cause)) == QueryEventResult.Allow && removedStack.quantity > 0)
                    {
                        //Make sure we aren't overcharging and leaving the slot with negative quantities
                        removedStack.quantity = Mathf.Min(removedStack.quantity, slots[i].Contents.quantity);

                        //Log it
                        everythingRemoved.Add((removedStack, slots[i]));

                        //Update slot
                        slots[i].Contents.quantity -= removedStack.quantity;
                        if (slots[i].Contents.quantity <= 0)
                        {
                            slots[i].Contents = null;
                        }
                        
                        //Update totalToRemove
                        totalToRemove -= removedStack.quantity;
                        if (totalToRemove <= 0) break;
                    }
                }
            }

            //If we haven't met our quota, revert
            if (totalToRemove > 0)
            {
                //Undo operation to prevent covariants
                foreach ((ItemStack stack, InventorySlot slot) in everythingRemoved)
                {
                    //Should not call add hook here, since we're reverting a failed operation
                    if (slot.Contents == null) slot.Contents = stack;
                    else slot.Contents.quantity += stack.quantity;
                }

                return null;
            }
            else
            {
                Hooks.PostRemove.Process(h => h(cause));
                return everythingRemoved.Select(i => i.Item1);
            }
        }

        /// <summary>
        /// Remove all items that match the given filter
        /// </summary>
        /// <returns>How many items were removed</returns>
        public override int RemoveAll(Predicate<ItemStack> filter, object cause)
        {
            int nRemoved = 0;

            for (int i = 0; i < slots.Count; ++i)
            {
                if (!slots[i].IsEmpty && filter(slots[i].Contents))
                {
                    //Create dummy removedStack
                    ItemStack removedStack = slots[i].Contents.Clone();
                    
                    //Check hook to see if we can continue, then check to make sure no hooks are up to funny business
                    if (Hooks.TryRemoveItem.Process(h => h(slots[i], removedStack, removedStack.Clone(), cause)) == QueryEventResult.Allow && removedStack.quantity > 0)
                    {
                        //Make sure we aren't overcharging and leaving the slot with negative quantities
                        removedStack.quantity = Mathf.Min(removedStack.quantity, slots[i].Contents.quantity);

                        //Log it
                        nRemoved += removedStack.quantity;

                        //Update slot
                        slots[i].Contents.quantity -= removedStack.quantity;
                        if (slots[i].Contents.quantity <= 0)
                        {
                            slots[i].Contents = null;
                            slots[i].UninstallHooks();
                        }
                    }
                }
            }

            Hooks.PostRemove.Process(h => h(cause));
            return nRemoved;
        }

        /// <summary>
        /// Check how many items match the given filter
        /// </summary>
        public override int Count(Predicate<ItemStack> filter) => slots.Where(i => !i.IsEmpty && filter(i.Contents)).Sum(i => i.Contents.quantity);

        /// <summary>
        /// Find all ItemStacks that match the filter
        /// </summary>
        public override IEnumerable<ItemStack> FindAll(Predicate<ItemStack> filter) => slots.Where(i => !i.IsEmpty && filter(i.Contents)).Select(i => i.Contents);

        public override void Sort(IComparer<ReadOnlyItemStack> comparer, object cause)
        {
        retry:

            //Find what slots can be sorted, and sort them
            List<InventorySlot> sortables = new List<InventorySlot>(slots);
            sortables.RemoveAll(slot => Hooks.TrySortSlot.Process(h => h(slot, cause)) != QueryEventResult.Allow);
            sortables.Sort(new ItemStackToSlotComparer(comparer));
            
            //Rearrange the sortable set in the original slots
            //This is all legal since it's just references
            int sortableIndex = 0;
            for (int slotIndex = 0; slotIndex < slots.Count; slotIndex++)
            {
                if (sortables.Contains(slots[slotIndex]))
                {
                    slots[slotIndex] = sortables[sortableIndex];
                    sortableIndex++;
                }
            }

            ValidateIDs();

            if (Hooks.PostSort.Process(h => h(cause)) == PostEventResult.Retry) goto retry;
        }

        public override void DoSetup()
        {
            foreach (InventoryProperty i in Properties) i._InstallHooks(this);

            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty) slots[i].InstallHooks();
            }
        }
        
        [SerializeField, HideInInspector] private InventoryHooksImplDetail _hooks;
        public override IInventoryHooks Hooks => _hooks != null ? _hooks : (_hooks = ScriptableObject.CreateInstance<InventoryHooksImplDetail>());


        #region Obsolete functions/variables, and upgrader

        protected void ValidateIDs()
        {
            //Ensure slots have correct IDs
            for (int i = 0; i < slots.Count; ++i) slots[i].ID = i;
        }

        public override void Validate()
        {
            foreach (InventoryProperty i in Properties) i._InstallHooks(this);

            ValidateIDs();
        }

        #endregion
    }

}
