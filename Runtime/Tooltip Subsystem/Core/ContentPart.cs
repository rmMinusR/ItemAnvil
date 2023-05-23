using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.Tooltips
{

    [RequireComponent(typeof(LayoutElement))]
    public abstract class ContentPart : MonoBehaviour
    {
        protected TooltipDisplay owner { get; private set; }
        protected internal int displayOrder { get; }

        protected virtual void Awake()
        {
            owner = GetComponentInParent<TooltipDisplay>();
            Debug.Assert(owner != null, "ContentPart must be nested under a TooltipDisplay", this);
            owner.RegisterContentPart(this);
        }

        protected virtual void OnDestroy()
        {
            if (owner != null) owner.UnregisterContentPart(this);
        }

        protected internal abstract void UpdateTarget(Tooltippable newTarget);
    }

}