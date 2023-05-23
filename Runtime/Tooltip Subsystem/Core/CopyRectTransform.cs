using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
internal sealed class CopyRectTransform : MonoBehaviour
{
    [SerializeField] private RectTransform source;
    [SerializeField] private bool copyPosition;
    [SerializeField] private bool copyWidth;
    [SerializeField] private bool copyHeight;

    private LayoutElement layoutElement; //Optional
    private HorizontalOrVerticalLayoutGroup layoutGroup; //Optional
    private void Awake()
    {
        layoutElement = GetComponent<LayoutElement>();
        layoutGroup = transform.parent != null ? transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>() : null;
    }

    [ContextMenu("Update manually")]
    void Update()
    {
#if UNITY_EDITOR
        if (layoutElement == null) layoutElement = GetComponent<LayoutElement>();
        if (layoutGroup == null) layoutGroup = transform.parent.GetComponent<HorizontalOrVerticalLayoutGroup>();
#endif

        if (layoutElement != null && layoutGroup != null)
        {
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform) transform);

            //Write to LayoutElement values and let parent Layout do the rest
            if (copyWidth ) layoutElement.preferredWidth  = source.sizeDelta.x;
            if (copyHeight) layoutElement.preferredHeight = source.sizeDelta.y;

            //Write to transform
            RectTransform dst = (RectTransform) transform;
            if (!layoutGroup.childControlWidth || !layoutGroup.childControlHeight)
            {
                dst.sizeDelta = new Vector2(
                    (copyWidth  && layoutGroup.childControlWidth ) ? source.sizeDelta.x : dst.sizeDelta.x,
                    (copyHeight && layoutGroup.childControlHeight) ? source.sizeDelta.y : dst.sizeDelta.y
                );
            }
        }
        else
        {
            //Write to transform
            RectTransform dst = (RectTransform) transform;
            if(copyPosition) dst.position = source.position;
            if (copyWidth || copyHeight)
            {
                dst.sizeDelta = new Vector2(
                    copyWidth  ? source.sizeDelta.x : dst.sizeDelta.x,
                    copyHeight ? source.sizeDelta.y : dst.sizeDelta.y
                );
            }
        } 
    }
}