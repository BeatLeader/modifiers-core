using System;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal class ModifierPanel : MonoBehaviour {
    public Toggle Toggle { get; private set; } = null!;
    public GameplayModifierToggle ModifierToggle { get; private set; } = null!;

    public event Action<ModifierPanel, bool>? ModifierStateChangedEvent;

    protected virtual void Awake() {
        ModifierToggle = GetComponent<GameplayModifierToggle>();
        Toggle = GetComponent<Toggle>();
        Toggle.onValueChanged.AddListener(HandleToggleStateChanged);
    }

    protected virtual void HandleToggleStateChanged(bool state) {
        ModifierStateChangedEvent?.Invoke(this, state);
    }
}