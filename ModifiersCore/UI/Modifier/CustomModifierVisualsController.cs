using System;
using System.Globalization;
using HMUI;
using TMPro;
using UnityEngine;

namespace ModifiersCore;

public class CustomModifierVisualsController : MonoBehaviour {
    #region Setup

    private ImageView _background = null!;
    private ToggleWithCallbacks _toggle = null!;
    private GameplayModifierToggle _modifierToggle = null!;

    private Color _backgroundColor;
    private Color _multiplierColor;

    private void Awake() {
        _toggle = GetComponent<ToggleWithCallbacks>();
        _modifierToggle = GetComponent<GameplayModifierToggle>();
        _background = transform.Find("BG").GetComponent<ImageView>();
        //
        DestroyImmediate(GetComponent<SwitchView>());
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
        _toggle.stateDidChangeEvent += HandleToggleSelectionStateChanged;
    }

    private void OnDestroy() {
        _toggle.onValueChanged.RemoveListener(HandleToggleStateChanged);
        _toggle.stateDidChangeEvent -= HandleToggleSelectionStateChanged;
    }

    public void SetModifier(ICustomModifier modifier) {
        var color = modifier.Multiplier > 0f ? positiveColor : negativeColor;
        _backgroundColor = modifier.Color ?? color;
        _multiplierColor = modifier.MultiplierColor ?? color;
        RefreshVisuals();
        RefreshText(modifier);
    }

    #endregion

    #region Text

    private void RefreshText(ICustomModifier modifier) {
        var multiplier = modifier.Multiplier;
        var positiveMul = multiplier > 0.0;
        var multiplierStr = positiveMul ? $"+{multiplier:P0}" : $"{multiplier:P0}";
        var multiplierText = _modifierToggle._multiplierText;
        //
        multiplierText.gameObject.SetActive(!Mathf.Approximately(multiplier, 0.0f));
        multiplierText.text = multiplierStr;
        multiplierText.color = positiveMul ? _modifierToggle._positiveColor : multiplierText.color;
        //
        _modifierToggle._nameText.text = modifier.Name;
        _modifierToggle._hoverTextSetter.text = modifier.Description;
        _modifierToggle._icon.sprite = modifier.Icon;
    }

    #endregion

    #region Colors

    private static readonly Color negativeColor = new(1f, 0.35f, 0f);
    private static readonly Color positiveColor = new(0f, 0.75f, 1f);

    private static readonly Color highlightedColor = new(0.61f, 0.59f, 0.67f);
    private static readonly float highlightedAlpha = 0.5f;
    private static readonly float secondaryAlpha = 0.75f;

    private void RefreshColors(bool on, bool highlighted) {
        //first color
        Color firstColor;
        if (on) {
            firstColor = _backgroundColor;
        } else if (highlighted) {
            firstColor = highlightedColor;
        } else {
            firstColor = Color.black;
        }
        //second color alpha
        float alpha;
        if (on) {
            alpha = highlighted ? 1f : highlightedAlpha;
        } else {
            alpha = highlighted ? highlightedAlpha : 1f;
        }
        //applying elements
        var elementsAlpha = on || highlighted ? 1f : secondaryAlpha;
        var elementsColor = Color.white.ColorWithAlpha(elementsAlpha);
        _modifierToggle._icon.color = elementsColor;
        _modifierToggle._nameText.color = elementsColor;
        _modifierToggle._multiplierText.color = on ? Color.white : _multiplierColor;
        //applying background
        _background.color0 = firstColor;
        _background.color1 = firstColor.ColorWithAlpha(alpha);
    }

    private void RefreshVisuals() {
        RefreshColors(_toggle.isOn, _toggle.selectionState is ToggleWithCallbacks.SelectionState.Highlighted);
    }

    #endregion

    #region Callbacks

    private void HandleToggleStateChanged(bool state) {
        RefreshVisuals();
    }

    private void HandleToggleSelectionStateChanged(ToggleWithCallbacks.SelectionState state) {
        RefreshVisuals();
    }

    #endregion
}