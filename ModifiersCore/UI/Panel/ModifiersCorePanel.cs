using System;
using System.Collections.Generic;
using System.Linq;
using BGLib.Polyglot;
using HMUI;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace ModifiersCore;

internal class ModifiersCorePanel : MonoBehaviour {
    #region Setup

    private GameplayModifiersPanelController _panel = null!;
    private RectTransform _modifiersSection = null!;
    private GridLayoutGroup _modifiersSectionGroup = null!;
    private GameObject _modifierPrefab = null!;

    private void Awake() {
        _panel = GetComponent<GameplayModifiersPanelController>();
        _modifiersSection = (RectTransform)transform.Find("Modifiers");
        _modifiersSectionGroup = _modifiersSection.GetComponent<GridLayoutGroup>();
        _modifierPrefab = _modifiersSection.GetComponentInChildren<GameplayModifierToggle>().gameObject;
        _modifiersSectionGroup.enabled = true;
        PatchUI();
    }

    private void Start() {
        ModifiersManager.ModifierAddedEvent += HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent += HandleModifierRemoved;
        //handling already added modifiers
        foreach (var modifier in ModifiersManager.Modifiers) {
            HandleModifierAddedInternal(modifier);
        }
    }

    private void OnDestroy() {
        ModifiersManager.ModifierAddedEvent -= HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent -= HandleModifierRemoved;
    }

    #endregion

    #region PatchUI

    private ManualScrollView _scrollView = null!;
    private RectTransform _scrollbarRect = null!;

    private void PatchUI() {
        var context = FindObjectsByType<Context>(FindObjectsSortMode.InstanceID).Last();
        var container = context.Container;
        //creating scroll view
        var scrollContainer = new GameObject("ScrollContainer");
        scrollContainer.AddComponent<Touchable>();
        scrollContainer.AddComponent<RectMask2D>();
        var scrollRect = scrollContainer.GetComponent<RectTransform>();
        _scrollView = scrollContainer.AddComponent<ManualScrollView>();
        //
        scrollRect.SetParent(_modifiersSection.parent, false);
        scrollRect.sizeDelta = new Vector2(100f, 46f);
        scrollRect.pivot = new Vector2(0.5f, 1f);
        scrollRect.localPosition = new Vector2(0f, -16f);
        //setting up modifiers section
        _modifiersSection.SetParent(scrollRect, false);
        _modifiersSection.anchorMin = new Vector2(0f, 1f);
        _modifiersSection.anchorMax = Vector2.one;
        _modifiersSection.sizeDelta = Vector2.zero;
        _modifiersSection.pivot = new Vector2(0.5f, 1f);
        var contentUpdater = _modifiersSection.gameObject.AddComponent<ScrollContentSizeUpdater>();
        contentUpdater.scrollView = _scrollView;
        //
        var modifiersSizeFitter = _modifiersSection.gameObject.AddComponent<ContentSizeFitter>();
        modifiersSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        //setting up scroll view
        _scrollView._scrollViewDirection = ScrollView.ScrollViewDirection.Vertical;
        _scrollView._scrollType = ScrollView.ScrollType.FixedCellSize;
        _scrollView._fixedCellSize = 9.3f;
        _scrollView._viewport = scrollRect;
        _scrollView._contentRectTransform = _modifiersSection;
        container.Inject(_scrollView);
        //
        AddScrollbar();
        _scrollView.ManualAwake();
        SetupScrollbar();
    }

    private void AddScrollbar() {
        var scrollbarPrefab = FindObjectsOfType<RectTransform>().First(x => x.name == "ScrollBar");
        _scrollbarRect = Instantiate(scrollbarPrefab, _panel.transform, false);
        //setting up scroll view
        _scrollView._verticalScrollIndicator = _scrollbarRect.Find("VerticalScrollIndicator").GetComponent<VerticalScrollIndicator>();
        _scrollView._pageUpButton = _scrollbarRect.Find("UpButton").GetComponent<Button>();
        _scrollView._pageDownButton = _scrollbarRect.Find("DownButton").GetComponent<Button>();
    }

    private void SetupScrollbar() {
        //setting up scrollbar
        _scrollbarRect.pivot = Vector2.one * 0.5f;
        _scrollbarRect.sizeDelta = new Vector2(8f, -24f);
        _scrollbarRect.localPosition = new Vector2(50f, -39f);
        _scrollView.SetContentSize(46f);
    }

    #endregion

    #region Toggles

    private readonly Dictionary<string, CustomModifierPanel> _toggles = new();
    private readonly Stack<CustomModifierPanel> _pooledToggles = new();

    private void SpawnToggle(ICustomModifier modifier) {
        CustomModifierPanel toggle;
        if (_pooledToggles.Count > 0) {
            //
            toggle = _pooledToggles.Pop();
            toggle.gameObject.SetActive(true);
        } else {
            //
            var go = Instantiate(_modifierPrefab, _modifiersSection, false);
            toggle = go.AddComponent<CustomModifierPanel>();
            toggle.Setup(this);
            _toggles[modifier.Id] = toggle;
        }
        //setting up
        var modifierParams = ModifiersManager.ModifierParams[modifier.Id];
        var state = ModifiersManager.GetModifierState(modifier.Id);
        var actualToggle = toggle.Toggle;
        //
        toggle.SetModifier(modifierParams);
        actualToggle.isOn = state;
        _panel._toggleForGameplayModifierParam[modifierParams] = actualToggle;
    }

    private void DespawnToggle(string id) {
        var toggle = _toggles[id];
        toggle.gameObject.SetActive(false);
        _toggles.Remove(id);
        _pooledToggles.Push(toggle);
        _panel._toggleForGameplayModifierParam.Remove(toggle.Modifier);
    }

    private void RefreshTogglesOrder() {
        var index = 0;
        foreach (var modifier in ModifiersManager.Modifiers) {
            var toggle = _toggles[modifier.Id];
            toggle.transform.SetSiblingIndex(index);
            index++;
        }
    }

    #endregion

    #region SetModifierActive

    private void SetModifiersActive(IEnumerable<GameplayModifierParamsSO> modifiers, bool state) {
        foreach (var modifier in modifiers) {
            _panel.SetToggleValueWithGameplayModifierParams(modifier, state);
            //saving state if modifier is custom
            if (modifier is CustomModifierParamsSO customModifier) {
                var id = customModifier.customModifier.Id;
                ModifiersManager.SetModifierState(id, state);
            }
        }
    }

    public void SetModifierActive(CustomModifierParamsSO modifier, bool state) {
        if (_panel._changingGameplayModifierToggles) {
            return;
        }
        _panel._changingGameplayModifierToggles = true;
        if (state) {
            SetModifiersActive(modifier.mutuallyExclusives, false);
            SetModifiersActive(modifier.requires, true);
        } else {
            SetModifiersActive(modifier.requiredBy, false);
        }
        ModifiersManager.SetModifierState(modifier.customModifier.Id, state);
        _panel._changingGameplayModifierToggles = false;
        _panel.RefreshTotalMultiplierAndRankUI();
    }

    #endregion

    #region Callbacks

    private void HandleModifierAddedInternal(ICustomModifier modifier) {
        SpawnToggle(modifier);
    }

    private void HandleModifierAdded(ICustomModifier modifier) {
        HandleModifierAddedInternal(modifier);
        RefreshTogglesOrder();
    }

    private void HandleModifierRemoved(ICustomModifier modifier) {
        DespawnToggle(modifier.Id);
        RefreshTogglesOrder();
    }

    #endregion
}