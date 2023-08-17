using System.Collections;
using TMPro;
using rmMinusR.Tooltips;
using UnityEditor;
using UnityEngine;
using rmMinusR.ItemAnvil.UI;

namespace rmMinusR.ItemAnvil.Tooltips
{

    public sealed class TooltipMarketable : TooltipPart
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TMP_Text text;
        [SerializeField] private string buyFormat = "{0}: Buy {1}";
        [SerializeField] private string sellFormat = "{0}: Sell {1}";
        [SerializeField] [Min(0)] private int padding = 8;

        private Marketable market;
        private MarketTransactionController interaction;

        protected override void UpdateTarget(Tooltippable newTarget)
        {
            //Try to grab necessary resources
            interaction = newTarget.GetComponent<MarketTransactionController>();
            if (newTarget.TryGetComponent(out ViewInventorySlot view) && !view.slot.IsEmpty) market = view.slot.Contents.itemType.Properties.Get<Marketable>();
            else market = null;

            //Determine whether we should be active
            if (market != null && interaction != null)
            {
                interaction.context.Assess(interaction.itemType, out bool operationAllowed, out _);
                root.SetActive(market != null && operationAllowed);
            }
            else root.SetActive(false);

            //Render text if active
            if (root.activeSelf) Render();
        }

    #if UNITY_EDITOR
        private void OnValidate()
        {
            Render();
        }
    #endif

        void Render()
        {
            if (!interaction) return; //Silence nominal error

            string formatter = interaction.context.PlayerAction switch
            {
                Marketable.Mode.Buying => buyFormat,
                Marketable.Mode.Selling => sellFormat,
                _ => throw new System.NotImplementedException()
            };

            text.text = string.Format(formatter, interaction?.MainControlName ?? "LMB", 1) + new string(' ', padding) + string.Format(formatter, interaction?.AltControlName ?? "RMB", interaction?.BatchQuantity ?? 5);
        }
    }

}