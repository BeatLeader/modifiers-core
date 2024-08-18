using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public interface ICustomModifier {
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
    /// Represents a color of the modifier container.
    /// </summary>
    Color? Color { get; }
    
    /// <summary>
    /// Represents a color of the modifier multiplier text.
    /// </summary>
    Color? MultiplierColor { get; }

    /// <summary>
    /// Represents a multiplier value.
    /// </summary>
    float Multiplier { get; }

    /// <summary>
    /// Represents a collection of modifiers that cannot be used with this modifier.
    /// </summary>
    IEnumerable<string>? MutuallyExclusives { get; }

    /// <summary>
    /// Represents a collection of modifiers that are required for this one to work.
    /// </summary>
    IEnumerable<string>? Requires { get; }

    /// <summary>
    /// Represents a collection of modifiers that require this modifier to work.
    /// </summary>
    IEnumerable<string>? RequiredBy { get; }
}