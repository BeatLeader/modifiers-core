using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal class ModifiersCorePanel : MonoBehaviour {
    #region Setup

    private GameplayModifiersPanelController Panel => _patcher.Panel;

    private ModifiersCoreUIPatcher _patcher = null!;
    internal ModifierPanelSpawner _spawner = null!;
    private GridLayoutGroup _modifiersSectionGroup = null!;

    private void Awake() {
        _patcher = gameObject.AddComponent<ModifiersCoreUIPatcher>();
        _spawner = gameObject.AddComponent<ModifierPanelSpawner>();
        _modifiersSectionGroup = _patcher.ModifiersSection.GetComponent<GridLayoutGroup>();
        _modifiersSectionGroup.enabled = true;
    }

    private void Start() {
        //handling already spawned
        foreach (var panel in _spawner.Panels) {
            HandleModifierSpawnedInternal(panel);
        }
        ModifiersManager.ModifierAddedEvent += HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent += HandleModifierRemoved;
        //handling already added modifiers
        foreach (var modifier in ModifiersManager.CustomModifiers) {
            HandleModifierAddedInternal(modifier);
        }
    }

    private void OnDestroy() {
        ModifiersManager.ModifierAddedEvent -= HandleModifierAdded;
        ModifiersManager.ModifierRemovedEvent -= HandleModifierRemoved;
    }

    #endregion

    #region Modifiers

    private void RefreshModifiersOrder() {
        var index = 0;
        foreach (var modifier in ModifiersManager.Modifiers) {
            var toggle = _spawner.GetSpawnedPanel(modifier.Id);
            toggle.transform.SetSiblingIndex(index);
            index++;
        }
    }

    #endregion

    #region SetModifierActive

    private void SetModifiersActive(IEnumerable<string>? modifiers, bool state, string? except = null) {
        if (modifiers == null) return;
        foreach (var modifier in modifiers) {
            if (modifier == except) continue;
            var panel = _spawner.GetSpawnedPanel(modifier);
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
    }

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
    }

    private void HandleModifierRemoved(ICustomModifier modifier) {
        var panel = _spawner.GetSpawnedPanel(modifier.Id);
        panel.ModifierStateChangedEvent -= HandleModifierStateChanged;
        _spawner.DespawnPanel(modifier.Id);
        RefreshModifiersOrder();
    }

    private void HandleModifierStateChanged(ModifierPanelBase panel, bool state) {
        SetModifierActive(panel.Modifier, state);
    }

    #endregion
}