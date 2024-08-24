using System.Collections.Generic;
using UnityEngine;

namespace ModifiersCore;

internal class ModifierPanelSpawner : MonoBehaviour {
    #region Setup

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
            var id = ModifiersManager.defaultModifierIds[toggle.gameplayModifier.modifierNameLocalizationKey];
            var modifier = ModifiersManager.AllModifiers[id];
            panel.SetModifier(modifier);
            panel._modifierToggle = toggle;
            _baseGamePanels.Add(id, panel);
            _spawnedPanels.Add(id, panel);
        }
    }

    #endregion

    #region Spawner

    public ICollection<ModifierPanelBase> Panels => _spawnedPanels.Values;

    private readonly Dictionary<string, ModifierPanelBase> _spawnedPanels = new();
    private readonly Dictionary<string, ModifierPanelBase> _baseGamePanels = new();
    private readonly Stack<CustomModifierPanel> _pooledPanels = new();

    public ModifierPanelBase GetSpawnedPanel(string id) {
        return _spawnedPanels[id];
    }

    public ModifierPanelBase SpawnPanel(ICustomModifier modifier) {
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
        panel.SetModifier(modifier);
        _spawnedPanels[modifier.Id] = panel;
        return panel;
    }

    public void DespawnPanel(string id) {
        if (_baseGamePanels.ContainsKey(id)) return;
        var panel = (CustomModifierPanel)_spawnedPanels[id];
        //
        panel.gameObject.SetActive(false);
        _spawnedPanels.Remove(id);
        _pooledPanels.Push(panel);
    }

    #endregion
}