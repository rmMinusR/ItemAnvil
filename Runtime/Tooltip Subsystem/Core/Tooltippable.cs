using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rmMinusR.Tooltips
{

    public abstract class Tooltippable : MonoBehaviour
    {
#if USING_INSPECTORSUGAR
        [field: InspectorReadOnly]
#endif
        [field: SerializeField]
        protected TooltipDisplay tooltipRenderer { get; set; }

        [Tooltip("If no TooltipDisplay is provided to Show(), and none is found on the hierarchy, this will be used instead.")]
        [SerializeField] protected TooltipDisplay fallbackTooltipRenderer;

        protected void Show(TooltipDisplay target = null)
        {
            if (tooltipRenderer == null)
            {
                tooltipRenderer = target ?? TooltipDisplay.GetInstance(transform) ?? fallbackTooltipRenderer;
                if (tooltipRenderer == null) throw new InvalidOperationException("No TooltipDisplay present!");
                tooltipRenderer.SetTarget(this);
            }
        }

        protected void Hide()
        {
            if (tooltipRenderer != null)
            {
                if (tooltipRenderer.GetTarget() == this) tooltipRenderer.ClearTarget(this);
                tooltipRenderer = null; 
            }
        }

        protected virtual void OnDisable() => Hide();
    }

}
