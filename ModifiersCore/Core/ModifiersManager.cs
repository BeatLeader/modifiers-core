using System;
using System.Collections.Generic;
using System.Linq;
using BGLib.Polyglot;
using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public static class ModifiersManager {
    static ModifiersManager() => LoadBaseGameModifiers();

    /// <summary>
    /// Represents a collection of registered modifiers.
    /// </summary>
    public static IReadOnlyCollection<IModifier> Modifiers => AllModifiers.Values;

    public static IReadOnlyCollection<ICustomModifier> CustomModifiers => InternalCustomModifiers.Values;

    public static event Action<ICustomModifier>? ModifierAddedEvent;
    public static event Action<ICustomModifier>? ModifierRemovedEvent;

    internal static readonly Dictionary<string, GameplayModifierParamsSO> ModifierParams = new();
    internal static readonly Dictionary<string, ICustomModifier> InternalCustomModifiers = new();
    internal static readonly Dictionary<string, IModifier> BaseGameModifiers = new();
    internal static readonly Dictionary<string, IModifier> AllModifiers = new();

    #region API

    /// <param name="id">The modifier identifier.</param>
    /// <returns>The modifier if it is represented in the collection and null if not.</returns>
    public static IModifier? GetModifierWithId(string id) {
        AllModifiers.TryGetValue(id, out var value);
        return value;
    }

    /// <param name="id">The modifier identifier.</param>
    /// <returns>True if modifier is represented in the collection and False if not.</returns>
    public static bool HasModifierWithId(string id) {
        return AllModifiers.ContainsKey(id);
    }

    public static bool HasBaseGameModifierWithId(string id) {
        return BaseGameModifiers.ContainsKey(id);
    }

    /// <summary>Gets the modifier state.</summary>
    /// <param name="id">The modifier identifier.</param>
    /// <exception cref="InvalidOperationException">Throws when a modifier with the specified id does not exist.</exception>
    /// <returns>A modifier state.</returns>
    public static bool GetModifierState(string id) {
        if (!HasModifierWithId(id)) {
            throw new InvalidOperationException("A modifier with such identifier does not exist");
        }
        ConfigFileData.Instance.ModifierStates.TryGetValue(id, out var state);
        return state;
    }

    /// <summary>Sets the modifier state.</summary>
    /// <param name="id">The modifier identifier.</param>
    /// <param name="state">The modifier state.</param>
    /// <exception cref="InvalidOperationException">Throws when a modifier with the specified id does not exist.</exception>
    public static void SetModifierState(string id, bool state) {
        if (!HasModifierWithId(id)) {
            throw new InvalidOperationException("A modifier with such identifier does not exist");
        }
        ConfigFileData.Instance.ModifierStates[id] = state;
    }

    /// <summary>
    /// Adds a modifier to the modifiers panel.
    /// </summary>
    /// <param name="modifier">The modifier to add.</param>
    /// <exception cref="InvalidOperationException">
    /// Throws when the specified modifier cannot be added because
    /// a modifier with the same name is already added.
    /// </exception>
    public static void AddModifier(ICustomModifier modifier) {
        if (HasModifierWithId(modifier.Id)) {
            throw new InvalidOperationException("A modifier with the same key is already added");
        }
        InternalCustomModifiers[modifier.Id] = modifier;
        AllModifiers[modifier.Id] = modifier;
        UpdateModifierParams(modifier);
        ModifierUtils.LinkModifiers(modifier, ModifierParams);
        ModifierAddedEvent?.Invoke(modifier);
    }

    /// <summary>
    /// Removes a modifier from the modifiers panel.
    /// </summary>
    /// <param name="id">The identifier of a modifier to remove.</param>
    /// <exception cref="InvalidOperationException">Throws when the specified modifier does not exist.</exception>
    public static void RemoveModifier(string id) {
        if (!InternalCustomModifiers.TryGetValue(id, out var modifier)) {
            throw new InvalidOperationException("A modifier with such key does not exist");
        }
        InternalCustomModifiers.Remove(id);
        ModifierUtils.UnlinkModifiers(modifier, ModifierParams);
        //TODO: add proper handling for bound modifiers
        // (when one modifier is bound to another but another one does not exist)
        var modifierParams = GetModifierParams(id);
        modifierParams._multiplier = 0f;
        ModifierRemovedEvent?.Invoke(modifier);
    }

    #endregion

    #region Tools

    private static void LoadBaseGameModifiers() {
        var modifiers = Resources.FindObjectsOfTypeAll<GameplayModifierParamsSO>();
        foreach (var modifier in modifiers) {
            var mod = new Modifier(
                modifier.modifierNameLocalizationKey,
                Localization.Get(modifier.modifierNameLocalizationKey),
                Localization.Get(modifier.descriptionLocalizationKey),
                modifier.icon,
                modifier.multiplier,
                MakeIdsArray(modifier.mutuallyExclusives),
                MakeIdsArray(modifier.requires),
                MakeIdsArray(modifier.requiredBy)
            );
            AllModifiers[mod.Id] = mod;
            BaseGameModifiers[mod.Id] = mod;
            ModifierParams[mod.Id] = modifier;
        }

        static string[] MakeIdsArray(GameplayModifierParamsSO[] modifiers) {
            return modifiers.Select(x => x.modifierNameLocalizationKey).ToArray();
        }
    }

    private static GameplayModifierParamsSO UpdateModifierParams(ICustomModifier modifier) {
        var modifierParams = GetModifierParams(modifier.Id);
        modifierParams._modifierNameLocalizationKey = modifier.Name;
        modifierParams._descriptionLocalizationKey = modifier.Description;
        modifierParams._icon = modifier.Icon;
        modifierParams._multiplier = modifier.Multiplier;
        modifierParams._mutuallyExclusives = MakeModifiersArray(modifier.MutuallyExclusives);
        modifierParams._requires = MakeModifiersArray(modifier.Requires);
        modifierParams._requiredBy = MakeModifiersArray(modifier.RequiredBy);
        return modifierParams;

        static GameplayModifierParamsSO[] MakeModifiersArray(IEnumerable<string>? ids) {
            return ids?.Select(GetModifierParams).ToArray() ?? [];
        }
    }

    private static GameplayModifierParamsSO GetModifierParams(string id) {
        if (!ModifierParams.TryGetValue(id, out var modifierParams)) {
            modifierParams = ScriptableObject.CreateInstance<GameplayModifierParamsSO>();
            ModifierParams[id] = modifierParams;
        }
        return modifierParams;
    }

    #endregion
}