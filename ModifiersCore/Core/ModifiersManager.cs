using System;
using System.Collections.Generic;
using System.Linq;
using IPA.Utilities;
using JetBrains.Annotations;

namespace ModifiersCore;

[PublicAPI]
public static partial class ModifiersManager {
    static ModifiersManager() => LoadBaseGameModifiers();

    /// <summary>
    /// Represents a collection of registered modifiers.
    /// </summary>
    public static IReadOnlyCollection<IModifier> Modifiers => AllModifiers.Values;

    /// <summary>
    /// Represents a collection of modifiers that cannot be added yet because they rely on unknown modifiers.
    /// </summary>
    public static IReadOnlyCollection<ICustomModifier> PendingModifiers => InternalPendingModifiers.Values;

    public static IReadOnlyCollection<ICustomModifier> CustomModifiers => InternalCustomModifiers.Values;

    public static event Action<ICustomModifier>? ModifierAddedEvent;
    public static event Action<ICustomModifier>? ModifierRemovedEvent;

    internal static readonly Dictionary<string, ICustomModifier> InternalCustomModifiers = new();
    internal static readonly Dictionary<string, ICustomModifier> InternalPendingModifiers = new();
    internal static readonly Dictionary<string, IModifier> AllModifiers = new();

    internal static readonly Dictionary<string, HashSet<string>> DependentModifiers = new();
    internal static readonly Dictionary<string, HashSet<string>> ExclusiveModifiers = new();
    internal static readonly Dictionary<string, HashSet<string>> ExclusiveCategories = new();
    internal static readonly Dictionary<string, HashSet<string>> CategorizedModifiers = new();

    private static readonly List<string> buffer = new();

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
        AddCustomModifierInternal(modifier, true);
        ReviewPendingModifiers();
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
        //removing from categories cache
        RemoveFromCache(modifier.Id, CategorizedModifiers);
        RemoveFromCache(modifier.Id, ExclusiveCategories);
        //removing from dependencies cache
        RemoveFromCache(modifier.Id, DependentModifiers);
        RemoveFromCache(modifier.Id, ExclusiveModifiers);
        //
        ModifierRemovedEvent?.Invoke(modifier);
    }

    public static (GameplayModifierToggle, IModifier)[]? Toggles() {
        return GameplayModifiersPanelPatch.CorePanel?._spawner?.Panels?.Select(p => (p._modifierToggle, p.Modifier))?.ToArray();
    }

    private static void AddCustomModifierInternal(ICustomModifier modifier, bool checkDependencies) {
        if (HasModifierWithId(modifier.Id)) {
            throw new InvalidOperationException("A modifier with the same key is already added");
        }
        if (modifier.Id.Length < 2 || modifier.Id.Length > 3) {
            throw new InvalidOperationException("A modifier key should be 2 or 3 characters long (example: NF)");
        }
        if (checkDependencies && !EnsureDependenciesExist(modifier)) {
            InternalPendingModifiers[modifier.Id] = modifier;
            return;
        }
        InternalCustomModifiers[modifier.Id] = modifier;
        AddModifierInternal(modifier);
        ModifierAddedEvent?.Invoke(modifier);
    }

    private static void AddModifierInternal(IModifier modifier) {
        AllModifiers[modifier.Id] = modifier;
        //caching categories
        AddToCache(modifier.Id, modifier.Categories, CategorizedModifiers);
        AddToCache(modifier.Id, modifier.MutuallyExclusiveCategories, ExclusiveCategories);
        //caching dependencies
        AddToCache(modifier.Id, modifier.RequiresModifiers, DependentModifiers);
        AddToCache(modifier.Id, modifier.MutuallyExclusiveModifiers, ExclusiveModifiers, true);
    }

    private static void ReviewPendingModifiers() {
        foreach (var modifier in PendingModifiers) {
            if (!EnsureDependenciesExist(modifier)) continue;
            //adding modifier
            AddCustomModifierInternal(modifier, false);
            buffer.Add(modifier.Id);
        }
        foreach (var id in buffer) {
            InternalPendingModifiers.Remove(id);
        }
        buffer.Clear();
    }

    #endregion

    #region Cache

    private static void RemoveFromCache(string id, IDictionary<string, HashSet<string>> dict) {
        foreach (var (_, set) in dict) {
            set.Remove(id);
        }
    }

    private static void AddToCache(
        string id,
        IEnumerable<string>? collection,
        IDictionary<string, HashSet<string>> dict,
        bool addSelf = false
    ) {
        if (collection == null) return;
        //adding current modifier
        if (addSelf) {
            if (!dict.TryGetValue(id, out var set)) {
                set = new();
                dict[id] = set;
            }
            foreach (var item in collection) {
                set.Add(item);
            }
        }
        //adding bound modifiers
        foreach (var item in collection) {
            if (!dict.TryGetValue(item, out var set)) {
                set = new();
                dict[item] = set;
            }
            set.Add(id);
        }
    }

    private static bool EnsureDependenciesExist(IModifier modifier) {
        if (modifier.RequiresModifiers is not { } requires) return true;
        return requires.All(require => AllModifiers.ContainsKey(require));
    }

    #endregion
}