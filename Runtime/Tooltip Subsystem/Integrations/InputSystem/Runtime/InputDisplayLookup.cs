using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class InputDisplayLookup : MonoBehaviour
{
    private static InputDisplayLookup __instance;
    private static InputDisplayLookup Instance => __instance != null ? __instance : (__instance = FindObjectOfType<InputDisplayLookup>());

    [SerializeField] private List<InputDisplayRecord> controlDisplays;
    [SerializeField] private InputDisplayRecord fallbackDisplay = new InputDisplayRecord { showText = true };

    public static InputDisplayRecord Fetch(InputControl control)
    {
        //Try to fetch from configurable source
        if (Instance != null) return Instance._InstFetch(control);
        
        //Fallback: No InputDisplayLookup exists
        else return new InputDisplayRecord
        {
            binding = control,
            icon = null,
            showText = true,
            textMargins = Vector2.zero
        };
    }

    private InputDisplayRecord _InstFetch(InputControl control)
    {
        //Try to fetch and display as config'd
        int ind = controlDisplays.FindIndex(x => x.binding.path == control.path); //TODO does this even work?
        if (ind != -1) return controlDisplays[ind];
        else
        {
            //Fallback: No matching record found
            InputDisplayRecord @out = fallbackDisplay;
            @out.binding = control;
            return @out;
        }
    }
}
