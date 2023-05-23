using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace rmMinusR.Tooltips
{

    public sealed class TooltippableShownManually : Tooltippable
    {
        public new void Show(TooltipDisplay target = null) => base.Show(target);

        public new void Hide() => base.Hide();
    }

}
