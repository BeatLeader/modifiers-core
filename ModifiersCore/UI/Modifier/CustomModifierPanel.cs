namespace ModifiersCore;

internal class CustomModifierPanel : ModifierPanelBase {
    #region Setup

    public override IModifier Modifier => _modifier;

    private CustomModifierVisualsController _visualsController = null!;

    protected override void Awake() {
        base.Awake();
        name = "CustomModifier";
        _modifierToggle = gameObject.GetComponent<GameplayModifierToggle>();
        _modifierToggle.enabled = false;
        _visualsController = gameObject.AddComponent<CustomModifierVisualsController>();
    }

    #endregion

    #region Modifier

    private ICustomModifier _modifier = null!;

    public void SetModifier(ICustomModifier customModifier) {
        _modifier = customModifier;
        _visualsController.SetModifier(customModifier);
        var modifierActive = ModifiersManager.GetModifierState(customModifier.Id);
        SetModifierActive(modifierActive);
    }

    #endregion

    #region Callbacks

    protected override void HandleToggleStateChanged(bool state) {
        ModifiersManager.SetModifierState(_modifier.Id, state);
        base.HandleToggleStateChanged(state);
    }

    #endregion
}