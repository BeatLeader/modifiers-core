using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public static class ModifiersManager {
    /// <summary>
    /// Represents a collection of registered modifiers.
    /// </summary>
    public static IReadOnlyCollection<ICustomModifier> Modifiers => InternalModifiers.Values;

    public static event Action<ICustomModifier>? ModifierAddedEvent;
    public static event Action<ICustomModifier>? ModifierRemovedEvent;

    internal static readonly Dictionary<string, CustomModifierParamsSO> ModifierParams = new();
    internal static readonly Dictionary<string, ICustomModifier> InternalModifiers = new();

    /// <param name="id">The modifier identifier.</param>
    /// <returns>The modifier if it is represented in the collection and null if not.</returns>
    public static ICustomModifier? GetModifierWithId(string id) {
        InternalModifiers.TryGetValue(id, out var value);
        return value;
    }

    /// <param name="id">The modifier identifier.</param>
    /// <returns>True if modifier is represented in the collection and False if not.</returns>
    public static bool HasModifierWithId(string id) {
        return InternalModifiers.ContainsKey(id);
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
        InternalModifiers[modifier.Id] = modifier;
        UpdateModifierParams(modifier);
        ModifierAddedEvent?.Invoke(modifier);
    }

    private static GameplayModifierParamsSO UpdateModifierParams(ICustomModifier modifier) {
        var modifierParams = GetModifierParams(modifier.Id);
        modifierParams.customModifier = modifier;
        modifierParams._modifierNameLocalizationKey = modifier.Name;
        modifierParams._descriptionLocalizationKey = modifier.Description;
        modifierParams._icon = modifier.Icon;
        modifierParams._multiplier = modifier.Multiplier;
        modifierParams._mutuallyExclusives = MakeModifiersArray(modifier.MutuallyExclusives);
        modifierParams._requires = MakeModifiersArray(modifier.Requires);
        modifierParams._requiredBy = MakeModifiersArray(modifier.RequiredBy);
        return modifierParams;

        static GameplayModifierParamsSO[] MakeModifiersArray(IEnumerable<string>? ids) {
            return ids?.Select(GetModifierParams).OfType<GameplayModifierParamsSO>().ToArray() ?? [];
        }
    }

    private static CustomModifierParamsSO GetModifierParams(string id) {
        if (!ModifierParams.TryGetValue(id, out var modifierParams)) {
            modifierParams = ScriptableObject.CreateInstance<CustomModifierParamsSO>();
            ModifierParams[id] = modifierParams;
        }
        return modifierParams;
    }
}