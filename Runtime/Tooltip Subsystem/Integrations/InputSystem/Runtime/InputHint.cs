using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHint : MonoBehaviour
{
    [Serializable]
    public struct Action
    {
        public string controlName;
        public string effect;
        public bool enabled;

        [Space]
        public bool useHold;
        [Min(0)] public float holdTime;
        [Min(0)] public float releaseTime;
    }

    [SerializeField] private List<Action> fixedControls;

    protected virtual IEnumerable<Action> EvalAdditionalControls()
    {
        yield break;
    }

    public IEnumerable<Action> GetControls()
    {
        foreach (Action i in fixedControls) yield return i;
        foreach (Action i in EvalAdditionalControls()) yield return i;
    }
}
