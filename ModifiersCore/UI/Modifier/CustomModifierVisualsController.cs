using HMUI;
using TMPro;
using UnityEngine;

namespace ModifiersCore;

public class CustomModifierVisualsController : MonoBehaviour {
    #region Setup

    private ImageView _background = null!;
    private ImageView _icon = null!;
    private TMP_Text _nameText = null!;
    private TMP_Text _multiplierText = null!;
    private ToggleWithCallbacks _toggle = null!;

    private Color _backgroundColor;
    private Color _multiplierColor;

    private void Awake() {
        _toggle = GetComponent<ToggleWithCallbacks>();
        _background = transform.Find("BG").GetComponent<ImageView>();
        _icon = transform.Find("Icon").GetComponent<ImageView>();
        _nameText = transform.Find("Name").GetComponent<TMP_Text>();
        _multiplierText = transform.Find("Multiplier").GetComponent<TMP_Text>();
        //
        _toggle.onValueChanged.AddListener(HandleToggleStateChanged);
        _toggle.stateDidChangeEvent += HandleToggleSelectionStateChanged;
    }

    private void OnDestroy() {
        _toggle.onValueChanged.RemoveListener(HandleToggleStateChanged);
        _toggle.stateDidChangeEvent -= HandleToggleSelectionStateChanged;
    }

    public void SetVisuals(Color backgroundColor, Color multiplierColor) {
        _backgroundColor = backgroundColor;
        _multiplierColor = multiplierColor;
        RefreshVisuals();
    }

    #endregion

    #region Colors

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
        _icon.color = elementsColor;
        _nameText.color = elementsColor;
        _multiplierText.color = on ? Color.white : _multiplierColor;
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