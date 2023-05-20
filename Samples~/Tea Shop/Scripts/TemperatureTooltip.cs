using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TemperatureTooltip
#if UNITY_EDITOR
    : Tooltips.ContentPart
#else
    : MonoBehaviour
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

    }
#endif

    private void Update()
    {
        gameObject.SetActive(temperature != null);
        if (temperature != null) text.text = string.Format(format, temperature.temperature);
    }
}
