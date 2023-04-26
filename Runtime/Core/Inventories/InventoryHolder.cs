using System.Collections;
using UnityEngine;

public class InventoryHolder : MonoBehaviour
{
#if USING_SUBCLASS_SELECTOR
    [SubclassSelector]
#endif
    [SerializeReference] public Inventory inventory;
}
