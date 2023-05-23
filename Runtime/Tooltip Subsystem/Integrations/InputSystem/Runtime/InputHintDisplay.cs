using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InputHintDisplay : MonoBehaviour
{
    [SerializeField] protected Image controlIcon;
    [SerializeField] protected TMP_Text controlText;
    [SerializeField] protected TMP_Text actionEffectText;

    [Space]
    [SerializeField] protected Color controlEnabledColor = Color.black;
    [SerializeField] protected Color controlDisabledColor = Color.gray;
    [SerializeField] protected Color actionEffectEnabledColor = Color.black;
    [SerializeField] protected Color actionEffectDisabledColor = Color.gray;

    public event Action<InputHint.Action, InputAction> OnWrite;

    public virtual void Write(InputHint.Action action, InputActionAsset map)
    {
        InputAction binding = map.FindAction(action.controlName);
        Debug.Assert(binding != null, "Control "+action.controlName+" not found in "+map);
        InputControl control = binding.activeControl ?? binding.controls[0];

        InputDisplayRecord disp = InputDisplayLookup.Fetch(control);

        controlIcon.sprite = disp.icon;

        //Write to control text
        controlText.gameObject.SetActive(disp.showText);
        if (disp.showText)
        {
            controlText.text = control.shortDisplayName;
            controlText.color = action.enabled ? controlEnabledColor : controlDisabledColor;
        }

        //Write what will happen if the player presses this control
        if (actionEffectText != null)
        {
            actionEffectText.text = action.effect;
            actionEffectText.color = action.enabled ? actionEffectEnabledColor : actionEffectDisabledColor;
        }

        OnWrite(action, binding);
    }
}
