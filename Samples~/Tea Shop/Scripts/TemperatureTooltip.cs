using rmMinusR.ItemAnvil.UI;
using rmMinusR.Tooltips;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TemperatureTooltip : TooltipPart
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private string format = "{0} °F";

    Temperature temperature;

    protected override void UpdateTarget(Tooltippable newTarget)
    {
        if (newTarget.TryGetComponent(out ViewInventorySlot view) && !view.slot.IsEmpty)
        {
            temperature = view.slot.Contents.instanceProperties.Get<Temperature>();
        }
        else temperature = null;

        gameObject.SetActive(temperature != null);
    }

    private void Update()
    {
        if (temperature != null) text.text = string.Format(format, temperature.temperature);
    }
}
