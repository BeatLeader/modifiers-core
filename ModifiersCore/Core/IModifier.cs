using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public interface IModifier {
    /// <summary>
    /// Represents the modifier identifier.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Represents the modifier name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Represents a description for the modifier.
    /// </summary>
    string Description { get; }

    /// <summary>
    /// Represents an icon that will be displayed within the modifier container.
    /// </summary>
    Sprite Icon { get; }

    /// <summary>
    /// Represents a multiplier value.
    /// </summary>
    float Multiplier { get; }

    /// <summary>
    /// Represents a collection of categories that cannot be used with this modifier.
    /// </summary>
    IEnumerable<string>? Categories { get; }

    /// <summary>
    /// Represents a collection of categories that cannot be used with this modifier.
    /// </summary>
    IEnumerable<string>? MutuallyExclusiveCategories { get; }

    /// <summary>
    /// Represents a collection of modifiers that cannot be used with this modifier.
    /// </summary>
    IEnumerable<string>? MutuallyExclusiveModifiers { get; }

    /// <summary>
    /// Represents a collection of modifiers that are required for this one to work.
    /// </summary>
    IEnumerable<string>? RequiresModifiers { get; }

    /// <summary>
    /// Represents a collection of modifiers that require this modifier to work.
    /// </summary>
    IEnumerable<string>? RequiredByModifiers { get; }
}

public static class DefaultCategory {
    public static string ENERGY = "ENERGY";
    public static string BEATMAP = "BEATMAP";
    public static string VISUALS = "VISUALS";
    public static string SCORING = "SCORING";
    public static string SPEED = "SPEED";
}