using System;
using HMUI;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal class CustomModifierPanel : MonoBehaviour {
    #region Setup

    public GameplayModifierParamsSO Modifier => _modifierToggle._gameplayModifier;
    public Toggle Toggle => _toggle;

    private CustomModifierVisualsController _visualsController = null!;
    private ToggleWithCallbacks _toggle = null!;
    private GameplayModifierToggle _modifierToggle = null!;
    private ModifiersCorePanel _panel = null!;

    private void Awake() {
        _visualsController = gameObject.AddComponent<CustomModifierVisualsController>();
        _modifierToggle = GetComponent<GameplayModifierToggle>();
        _toggle = GetComponent<ToggleWithCallbacks>();
        //
        name = "CustomModifier";
        _modifierToggle.enabled = false;
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
        DestroyImmediate(GetComponent<SwitchView>());
    }

    public void Setup(ModifiersCorePanel panel) {
        _panel = panel;
    }

    #endregion

    #region Visuals

    private static readonly Color negativeColor = new(1f, 0.35f, 0f);
    private static readonly Color positiveColor = new(0f, 0.75f, 1f);

    private CustomModifierParamsSO? _modifier;
    
    public void SetModifier(CustomModifierParamsSO gameplayModifier) {
        var customModifier = gameplayModifier.customModifier;
        _modifier = gameplayModifier;
        _modifierToggle._gameplayModifier = gameplayModifier;
        _modifierToggle.Start();
        var color = customModifier.Multiplier > 0f ? positiveColor : negativeColor;
        var backgroundColor = customModifier.Color ?? color;
        var multiplierColor = customModifier.MultiplierColor ?? color;
        _visualsController.SetVisuals(backgroundColor, multiplierColor);
    }
    
    #endregion

    #region Callbacks

    private void HandleToggleStateChanged(bool state) {
        if (_panel == null || _modifier == null) {
            throw new InvalidOperationException("The component was not initialized");
        }
        _panel.SetModifierActive(_modifier, state);
    }

    #endregion
}