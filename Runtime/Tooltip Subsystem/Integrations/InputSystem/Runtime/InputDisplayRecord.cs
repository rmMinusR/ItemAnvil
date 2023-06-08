using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct InputDisplayRecord
{
    public InputControl binding;
    public Sprite icon;

    public bool showText;
    public Vector2 textMargins;
}
