using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using HMUI;
using UnityEngine;

namespace ModifiersCore;

internal class ModifiersCorePanel : MonoBehaviour {
    #region Setup

    private GameplayModifiersPanelController Panel => _patcher.Panel;

    private ModifiersCoreUIPatcher _patcher = null!;
    internal ModifierPanelSpawner _spawner = null!;

    private void Awake() {
        _patcher = gameObject.AddComponent<ModifiersCoreUIPatcher>();
        _spawner = gameObject.AddComponent<ModifierPanelSpawner>();
    }

    private void Start() {
        //handling already spawned
        foreach (var panel in _spawner.Panels) {
            HandleModifierSpawnedInternal(panel);
        }
        ModifiersManager.ModifierAddedEvent += HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent += HandleModifierRemoved;
        _patcher.TabSelector.didSelectCellEvent += HandleTabSelected;
        //handling already added modifiers
        foreach (var modifier in ModifiersManager.CustomModifiers) {
            HandleModifierAddedInternal(modifier);
        }
        RefreshTabs();
    }

    private void OnDestroy() {
        ModifiersManager.ModifierAddedEvent -= HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent -= HandleModifierRemoved;
        if (_patcher.TabSelector != null) {
            _patcher.TabSelector.didSelectCellEvent -= HandleTabSelected;
        }
    }

    #endregion

    #region Modifiers

    private void RefreshModifiersOrder() {
        //base game modifiers keep their places, only the custom section is laid out
        var index = 0;
        foreach (var modifier in ModifiersManager.CustomModifiers) {
            var toggle = _spawner.GetSpawnedPanel(modifier.Id);
            toggle.transform.SetSiblingIndex(index);
            index++;
        }
    }

    #endregion

    #region Tabs

    private void RefreshTabs() {
        //there is nothing to switch to without custom modifiers
        var hasCustomModifiers = ModifiersManager.CustomModifiers.Count > 0;
        _patcher.TabSelector.gameObject.SetActive(hasCustomModifiers);
        ShowTab(hasCustomModifiers ? ConfigFileData.Instance.SelectedTab : ModifiersTab.Vanilla);
    }

    private void ShowTab(ModifiersTab tab) {
        _patcher.TabSelector.SelectCellWithNumber((int)tab);
        _patcher.ModifiersSection.gameObject.SetActive(tab is ModifiersTab.Vanilla);
        _patcher.CustomModifiersContainer.gameObject.SetActive(tab is ModifiersTab.Custom);
    }

    private void HandleTabSelected(SegmentedControl segmentedControl, int cellIdx) {
        var tab = (ModifiersTab)cellIdx;
        ConfigFileData.Instance.SelectedTab = tab;
        ShowTab(tab);
    }

    #endregion

    #region SetModifierActive

    private void SetModifiersActive(IEnumerable<string>? modifiers, bool state, string? except = null) {
        if (modifiers == null) return;
        foreach (var modifier in modifiers) {
            if (modifier == except) continue;
            if (!_spawner.TryGetSpawnedPanel(modifier, out var panel)) continue;
            panel.SetModifierActive(state);
        }
    }

    private void SetCategoriesActive(
        IEnumerable<string>? categories,
        IDictionary<string, HashSet<string>> cache,
        bool state, 
        string? except = null
    ) {
        if (categories == null) return;
        foreach (var category in categories) {
            cache.TryGetValue(category, out var modifiers);
            SetModifiersActive(modifiers, state, except);
        }
    }

    public void SetModifierActive(IModifier modifier, bool state) {
        if (Panel._changingGameplayModifierToggles) {
            return;
        }
        Panel._changingGameplayModifierToggles = true;
        if (state) {
            //disabling categories that cannot be used with this modifier
            SetCategoriesActive(modifier.MutuallyExclusiveCategories, ModifiersManager.CategorizedModifiers, false, modifier.Id);
            //disabling modifiers that cannot be used with this category
            SetCategoriesActive(modifier.Categories, ModifiersManager.ExclusiveCategories, false, modifier.Id);
            //disabling modifiers that cannot be used with this one
            ModifiersManager.ExclusiveModifiers.TryGetValue(modifier.Id, out var exclusiveModifiers);
            SetModifiersActive(exclusiveModifiers, false);
            //enabling modifiers that required for this one to work
            SetModifiersActive(modifier.RequiresModifiers, true);
        } else {
            //disabling modifiers that rely on this one
            ModifiersManager.DependentModifiers.TryGetValue(modifier.Id, out var dependOnThis);
            SetModifiersActive(dependOnThis, false);
        }
        Panel._gameplayModifiers = Panel._gameplayModifiersModel.CreateGameplayModifiers(Panel.GetToggleValueWithGameplayModifierParams);
        Panel._changingGameplayModifierToggles = false;
        Panel.RefreshTotalMultiplierAndRankUI();
        (modifiersChangedEventField.GetValue(Panel) as Action)?.Invoke();
    }

    private static readonly FieldInfo modifiersChangedEventField = AccessTools.Field(
        typeof(GameplayModifiersPanelController),
        nameof(GameplayModifiersPanelController.didChangeGameplayModifiersEvent)
    );

    #endregion

    #region Callbacks

    private void HandleModifierSpawnedInternal(ModifierPanelBase panel) {
        panel.ModifierStateChangedEvent += HandleModifierStateChanged;
    }

    private void HandleModifierAddedInternal(ICustomModifier modifier) {
        var panel = _spawner.SpawnPanel(modifier);
        HandleModifierSpawnedInternal(panel);
    }

    private void HandleModifierAdded(ICustomModifier modifier) {
        HandleModifierAddedInternal(modifier);
        RefreshModifiersOrder();
        RefreshTabs();
    }

    private void HandleModifierRemoved(ICustomModifier modifier) {
        var panel = _spawner.GetSpawnedPanel(modifier.Id);
        panel.ModifierStateChangedEvent -= HandleModifierStateChanged;
        _spawner.DespawnPanel(modifier.Id);
        RefreshModifiersOrder();
        RefreshTabs();
    }

    private void HandleModifierStateChanged(ModifierPanelBase panel, bool state) {
        SetModifierActive(panel.Modifier, state);
    }

    #endregion
}