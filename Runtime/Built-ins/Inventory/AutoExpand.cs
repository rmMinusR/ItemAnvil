using rmMinusR.ItemAnvil.Hooks;
using System.Diagnostics;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Allow an inventory to expand or condense to fit its contents.
    /// Replaces legacy CondensingInventory. Only valid on StandardInventory.
    /// </summary>
    public sealed class AutoExpand : InventoryProperty
    {
        //On overflow: add more slots and try again
        private PostEventResult _HandleOverflows(ItemStack overflow, object cause)
        {
            if (overflow.quantity > 0)
            {
                ((StandardInventory)inventory).AppendSlot();
                return PostEventResult.Retry;
            }
            else return PostEventResult.Continue;
        }

        //After removal: Remove empty slots
        private void _Condense(object cause) => ((StandardInventory)inventory).Condense();

        protected override void InstallHooks()
        {
            Debug.Assert(inventory is StandardInventory, $"{nameof(AutoExpand)} is only valid on {nameof(StandardInventory)} or its children");
            inventory.HookPostAddItem(_HandleOverflows, 0);
            inventory.HookPostRemove(_Condense, 0);
        }
    }

}