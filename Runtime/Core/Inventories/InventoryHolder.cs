using System.Collections;
using UnityEngine;

namespace rmMinusR.ItemAnvil
{

    /// <summary>
    /// An inventory in component form. Most operations need just an inventory, but the provided components will typically bind to one of these.
    /// </summary>
    public class InventoryHolder : MonoBehaviour
    {
        [Header("Ticking")]
        [SerializeField] private bool doTick = true;
        [SerializeField] [Min(0)] private float tickInterval = 0.2f;
        private float timeSinceLastTick = 0;

        [Space]
        [TypeSwitcher(keepData = true, order = 2)]
        [SerializeReference] public Inventory inventory;

        private void Update()
        {
            if (doTick)
            {
                timeSinceLastTick += Time.deltaTime;
                if (timeSinceLastTick > tickInterval)
                {
                    inventory.Tick(this);
                    timeSinceLastTick = 0;
                }
            }
        }

        private void OnValidate()
        {
            if (inventory != null) inventory.Validate();
        }
    }

}