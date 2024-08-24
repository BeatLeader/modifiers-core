using System;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal abstract class ModifierPanelBase : MonoBehaviour {
    public abstract IModifier Modifier { get; }

    public event Action<ModifierPanelBase, bool>? ModifierStateChangedEvent;

    private Toggle _toggle = null!;
    public GameplayModifierToggle _modifierToggle = null!;

    public void SetModifierActive(bool active) {
        _toggle.isOn = active;
    }

    protected virtual void Awake() {
        _toggle = GetComponent<Toggle>();
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
    }

    protected virtual void HandleToggleStateChanged(bool state) {
        ModifierStateChangedEvent?.Invoke(this, state);
    }
}