using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public interface ICustomModifier : IModifier {
    /// <summary>
    /// Represents a color of the modifier container.
    /// </summary>
    Color? Color { get; }
    
    /// <summary>
    /// Represents a color of the modifier multiplier text.
    /// </summary>
    Color? MultiplierColor { get; }
}