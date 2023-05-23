using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace rmMinusR.Tooltips
{
    public sealed class TooltipSectionManager : MonoBehaviour
    {
        [SerializeField] private TooltipDisplay root;

        private void Awake()
        {
            root = GetComponentInParent<TooltipDisplay>();
            Debug.Assert(root != null, "TooltipSectionManager must be nested under a TooltipDisplay", this);
            root.RegisterSection(this);
        }
        private void OnDestroy() => root.UnregisterSection(this);

        internal void UpdateTarget(Tooltippable newTarget)
        {
            foreach (TooltipPart c in contentParts) c.UpdateTarget(newTarget);
        }

#if USING_INSPECTORSUGAR
        [InspectorReadOnly(editing = AccessMode.ReadOnly, playing = AccessMode.ReadWrite)]
#endif
        [SerializeField] private List<TooltipPart> contentParts = new List<TooltipPart>();
        internal void RegisterContentPart(TooltipPart c)
        {
            Debug.Assert(!contentParts.Contains(c));
            Debug.Assert(c.transform.IsChildOf(this.transform));

            if (contentParts.Count > 0)
            {
                //Complex case: Insert at correct index

                int insIndex = contentParts.FindLastIndex(i => i.DisplayOrder <= c.DisplayOrder) + 1;
                contentParts.Insert(insIndex, c);

                //Try to position right after the previous element
                if (insIndex != 0) c.transform.SetSiblingIndex(contentParts[insIndex-1].transform.GetSiblingIndex()+1);
                //Otherwise, position right before the next element
                else
                {
                    Transform next = contentParts[insIndex+1].transform;
                    c.transform.SetSiblingIndex(next.GetSiblingIndex());
                    next.SetSiblingIndex(c.transform.GetSiblingIndex()+1); //TODO test if this works on lists with more items
                }
            }
            else
            {
                //Trivial case: just put it there, assume it has right order in hierarchy
                contentParts.Add(c);
            }
        }
        internal void UnregisterContentPart(TooltipPart c) => contentParts.Remove(c);
    }
}
