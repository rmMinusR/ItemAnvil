using rmMinusR.Tooltips;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TemperatureTooltip : ContentPart
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private string format = "{0} °F";

    Temperature temperature;


    protected override void UpdateTarget(Tooltippable newTarget)
    {
        if (newTarget.TryGetComponent(out ViewItemStack view))
        {
            temperature = view.mostRecentStack.GetProperty<Temperature>();
        }
        else temperature = null;

    }

    private void Update()
    {
        gameObject.SetActive(temperature != null);
        if (temperature != null) text.text = string.Format(format, temperature.temperature);
    }
}
