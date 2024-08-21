using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal class ModifiersCorePanel : MonoBehaviour {
    #region Setup

    private GameplayModifiersPanelController Panel => _patcher.Panel;

    private ModifiersCoreUIPatcher _patcher = null!;
    private ModifierPanelSpawner _spawner = null!;
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

    private void SetModifiersActive(IEnumerable<GameplayModifierParamsSO> modifiers, bool state) {
        foreach (var modifier in modifiers) {
            Panel.SetToggleValueWithGameplayModifierParams(modifier, state);
        }
    }

    public void SetModifierActive(GameplayModifierParamsSO modifier, bool state) {
        if (Panel._changingGameplayModifierToggles) {
            return;
        }
        Panel._changingGameplayModifierToggles = true;
        if (state) {
            SetModifiersActive(modifier.mutuallyExclusives, false);
            SetModifiersActive(modifier.requires, true);
        } else {
            SetModifiersActive(modifier.requiredBy, false);
        }
        Panel._gameplayModifiers = Panel._gameplayModifiersModel.CreateGameplayModifiers(Panel.GetToggleValueWithGameplayModifierParams);
        Panel._changingGameplayModifierToggles = false;
        Panel.RefreshTotalMultiplierAndRankUI();
    }

    #endregion

    #region Callbacks

    private void HandleModifierSpawnedInternal(ModifierPanel panel) {
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

    private void HandleModifierStateChanged(ModifierPanel panel, bool state) {
        SetModifierActive(panel.ModifierToggle.gameplayModifier, state);
    }

    #endregion
}