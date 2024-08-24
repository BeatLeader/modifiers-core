using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ModifiersCore;

[PublicAPI]
public record CustomModifier(
    string Id,
    string Name,
    string Description,
    Sprite Icon,
    Color? Color,
    Color? MultiplierColor,
    float Multiplier,
    IEnumerable<string>? Categories = null,
    IEnumerable<string>? MutuallyExclusiveCategories = null,
    IEnumerable<string>? MutuallyExclusiveModifiers = null,
    IEnumerable<string>? RequiresModifiers = null,
    IEnumerable<string>? RequiredByModifiers = null
) : ICustomModifier;