using System;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Decorator to show a "Switch type..." dropdown. For use by polymorphic types that serialize inline and don't derive from UnityEngine.Object.
    /// </summary>
    public class TypeSwitcherAttribute : PropertyAttribute
    {
        /// <summary>
        /// When switching type, should we attempt to write previous values onto the new type's members?
        /// </summary>
        public bool keepData = false;
    }

}