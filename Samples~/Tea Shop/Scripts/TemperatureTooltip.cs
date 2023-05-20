using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TemperatureTooltip
#if UNITY_EDITOR
    : Tooltips.ContentPart
#endif
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private string format = "{0} °F";

    Temperature temperature;


#if UNITY_EDITOR
    protected override void UpdateTarget(Tooltips.Tooltippable newTarget)
    {
        if (newTarget.TryGetComponent(out ViewItemStack view))
        {
            temperature = view.mostRecentStack.GetProperty<Temperature>();
        }
        else temperature = null;

        gameObject.SetActive(temperature != null);
    }
#else
    private void Start()
    {
        gameObject.SetActive(false);
    }
#endif

    private void Update()
    {
        if (temperature != null) text.text = string.Format(format, temperature.temperature);
    }
}
