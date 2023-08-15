using UnityEngine;

namespace rmMinusR.ItemAnvil.UI
{
    public static class ScrollRectExtensions
    {
        public static Vector2 GetAreaOutOfBounds(RectTransform bounds, RectTransform target)
        {
            //All coordinates local to bounds.parent
            Rect validPosRect = bounds.rect;
            validPosRect = Rect.MinMaxRect( //Account for target's extents
                validPosRect.xMin + target.rect.size.x / 2,
                validPosRect.yMin + target.rect.size.y / 2,
                validPosRect.xMax - target.rect.size.x / 2,
                validPosRect.yMax - target.rect.size.y / 2
            );

            Vector2 targetStartCoord = bounds.transform.InverseTransformPoint(target.position);
            Vector2 targetEndCoord = new Vector2(
                Mathf.Clamp(targetStartCoord.x, validPosRect.xMin, validPosRect.xMax),
                Mathf.Clamp(targetStartCoord.y, validPosRect.yMin, validPosRect.yMax)
            );

            return targetEndCoord - targetStartCoord;
        }
    }
}