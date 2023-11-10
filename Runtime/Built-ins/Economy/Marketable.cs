using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Represents that a commodity can be bought and sold on the market.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: "ItemAnvil", sourceClassName: "Marketable")]
    public class Marketable : ItemProperty
    {
        public Item currency;

        public bool isBuyable;
        [Min(0)] public int buyPrice;

        public bool isSellable;
        [Min(0)] public int sellPrice;

        public enum Mode
        {
            Buying,
            Selling
        }

        //Data-carrier property: No active logic, no hooks to install or uninstall
        protected internal override void InstallHooks(InventorySlot context) { }
        protected internal override void UninstallHooks(InventorySlot context) { }
    }

}