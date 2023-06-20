using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// Limit item stacking to the given amount. If not present, items will stack infinitely.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "", sourceAssembly: "ItemAnvil", sourceClassName: "MaxStackSize")]
    public class MaxStackSize : ItemProperty
    {
        [Min(1)] public int size = 10;
    }

}