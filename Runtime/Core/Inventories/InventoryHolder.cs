using System.Collections;
using UnityEngine;

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
                inventory.Tick();
                timeSinceLastTick = 0;
            }
        }
    }
}
