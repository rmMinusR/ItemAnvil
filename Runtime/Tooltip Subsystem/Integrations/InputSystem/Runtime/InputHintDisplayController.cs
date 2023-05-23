using System.Collections;
using System.Collections.Generic;
using TMPro;
using rmMinusR.Tooltips;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public sealed class InputHintDisplayController : ContentPart
{
    [SerializeField] private LayoutGroup root;
    [SerializeField] private List<InputHintDisplay> instances;
    [SerializeField] private InputHintDisplay prefab;

    protected override void UpdateTarget(Tooltippable newTarget)
    {
        if (newTarget.TryGetComponent(out InputHint hint))
        {
            List<InputHint.Action> actions = new List<InputHint.Action>(hint.GetControls());

            //Match count
            while (instances.Count > actions.Count) { Destroy(instances[instances.Count-1]); instances.RemoveAt(instances.Count-1); }
            while (instances.Count < actions.Count) { instances.Add(Instantiate(prefab, root.transform)); }

            //Write data
            InputActionAsset controlMap = PlayerInput.all.Count > 0
                                                    ? PlayerInput.all[0].actions //Try to get from PlayerInput. TODO fix for multiplayer
                                                    : EventSystem.current.GetComponent<InputSystemUIInputModule>().actionsAsset; //Fallback: grab from UI
            for (int i = 0; i < actions.Count; ++i) instances[i].Write(actions[i], controlMap);
        }
        else root.gameObject.SetActive(false);
    }
}
