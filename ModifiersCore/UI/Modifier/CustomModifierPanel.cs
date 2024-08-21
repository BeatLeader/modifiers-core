using UnityEngine;

namespace ModifiersCore;

internal class CustomModifierPanel : ModifierPanel {
    #region Setup

    private CustomModifierVisualsController _visualsController = null!;

    protected override void Awake() {
        base.Awake();
        name = "CustomModifier";
        ModifierToggle.enabled = false;
        _visualsController = gameObject.AddComponent<CustomModifierVisualsController>();
    }

    #endregion

    #region Visuals

    private static readonly Color negativeColor = new(1f, 0.35f, 0f);
    private static readonly Color positiveColor = new(0f, 0.75f, 1f);

    private string? _modifierId;

    public void SetModifier(ICustomModifier customModifier, GameplayModifierParamsSO gameplayModifier) {
        _modifierId = customModifier.Id;
        ModifierToggle._gameplayModifier = gameplayModifier;
        ModifierToggle.Start();
        var color = customModifier.Multiplier > 0f ? positiveColor : negativeColor;
        var backgroundColor = customModifier.Color ?? color;
        var multiplierColor = customModifier.MultiplierColor ?? color;
        _visualsController.SetVisuals(backgroundColor, multiplierColor);
        Toggle.isOn = ModifiersManager.GetModifierState(_modifierId);
    }

    #endregion

    #region Callbacks

    protected override void HandleToggleStateChanged(bool state) {
        ModifiersManager.SetModifierState(_modifierId!, state);
        base.HandleToggleStateChanged(state);
    }

    #endregion
}