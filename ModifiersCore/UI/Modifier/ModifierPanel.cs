namespace ModifiersCore;

internal class ModifierPanel : ModifierPanelBase {
    public override IModifier Modifier => _modifier;

    private IModifier _modifier = null!;

    public void SetModifier(IModifier modifier) {
        _modifier = modifier;
    }
}