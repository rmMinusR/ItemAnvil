using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace rmMinusR.Tooltips
{

    [RequireComponent(typeof(LayoutElement))]
    public abstract class TooltipPart : MonoBehaviour
    {
        protected TooltipSectionManager section { get; private set; }

        [SerializeField] protected int m_displayOrder = 0; //TODO is there a clean way to have this be optional?
        protected internal virtual int DisplayOrder => m_displayOrder;

        protected virtual void Awake()
        {
            section = GetComponentInParent<TooltipSectionManager>();
            Debug.Assert(section != null, "TooltipPart must be nested under a TooltipSectionManager", this);
            section.RegisterContentPart(this);
        }

        protected virtual void OnDestroy()
        {
            if (section != null) section.UnregisterContentPart(this);
        }

        protected internal abstract void UpdateTarget(Tooltippable newTarget);
    }

}