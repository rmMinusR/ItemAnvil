using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(InputHintDisplay))]
public class InputHintDisplayHeld : MonoBehaviour
{
    [Space]
    [SerializeField] protected GameObject holdOverlay;
    [SerializeField] [Tooltip("Optional, but strongly recommended")] protected Image holdSlider;

    protected virtual void Awake()     => GetComponent<InputHintDisplay>().OnWrite += Write;
    protected virtual void OnDestroy() => GetComponent<InputHintDisplay>().OnWrite -= Write;

    protected (InputAction control, InputHint.Action action, bool isBound) binding;

    protected virtual void Write(InputHint.Action action, InputAction bound)
    {
        holdOverlay.SetActive(action.useHold);
        if (holdSlider != null) holdSlider.fillAmount = 0;

        binding.isBound = action.useHold;
        if (action.useHold)
        {
            binding.control = bound;
            binding.action = action;
        }
    }

    protected void Update()
    {
        if (binding.isBound && holdSlider != null)
        {
            //Update fill amount
            float change = binding.control.activeControl.IsPressed() ? binding.action.holdTime : -binding.action.releaseTime; //TODO use events
            holdSlider.fillAmount = Mathf.Clamp01(holdSlider.fillAmount + Time.deltaTime / change);
        }
    }
}