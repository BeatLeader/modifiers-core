using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ModifiersCore;

internal class ModifierPanelSpawner : MonoBehaviour {
    #region Setup

    private Dictionary<GameplayModifierParamsSO, Toggle> TogglesForModifiers => _patcher.Panel._toggleForGameplayModifierParam;
    private RectTransform ModifiersSection => _patcher.ModifiersSection;

    private ModifiersCoreUIPatcher _patcher = null!;
    private GameObject _modifierPrefab = null!;

    private void Awake() {
        _patcher = GetComponent<ModifiersCoreUIPatcher>();
        _modifierPrefab = ModifiersSection.GetComponentInChildren<GameplayModifierToggle>().gameObject;
        LoadExistingModifiers();
    }

    private void LoadExistingModifiers() {
        var toggles = _patcher.ModifiersSection.GetComponentsInChildren<GameplayModifierToggle>();
        foreach (var toggle in toggles) {
            var panel = toggle.gameObject.AddComponent<ModifierPanel>();
            var id = toggle.gameplayModifier.modifierNameLocalizationKey;
            _baseGamePanels.Add(id, panel);
            _spawnedPanels.Add(id, panel);
        }
    }

    #endregion

    #region Spawner

    public ICollection<ModifierPanel> Panels => _spawnedPanels.Values;

    private readonly Dictionary<string, ModifierPanel> _spawnedPanels = new();
    private readonly Dictionary<string, ModifierPanel> _baseGamePanels = new();
    private readonly Stack<CustomModifierPanel> _pooledPanels = new();

    public ModifierPanel GetSpawnedPanel(string id) {
        return _spawnedPanels[id];
    }

    public ModifierPanel SpawnPanel(ICustomModifier modifier) {
        CustomModifierPanel panel;
        if (_pooledPanels.Count > 0) {
            //
            panel = _pooledPanels.Pop();
            panel.gameObject.SetActive(true);
        } else {
            //
            var go = Instantiate(_modifierPrefab, ModifiersSection, false);
            panel = go.AddComponent<CustomModifierPanel>();
        }
        //setting up
        var modifierParams = ModifiersManager.ModifierParams[modifier.Id];
        var actualToggle = panel.Toggle;
        panel.SetModifier(modifier, modifierParams);
        //saving
        TogglesForModifiers[modifierParams] = actualToggle;
        _spawnedPanels[modifier.Id] = panel;
        //returning
        return panel;
    }

    public void DespawnPanel(string id) {
        if (_baseGamePanels.ContainsKey(id)) return;
        //fetching
        var panel = (CustomModifierPanel)_spawnedPanels[id];
        var modifier = panel.ModifierToggle.gameplayModifier;
        //
        panel.gameObject.SetActive(false);
        _spawnedPanels.Remove(id);
        _pooledPanels.Push(panel);
        TogglesForModifiers.Remove(modifier);
    }

    #endregion
}