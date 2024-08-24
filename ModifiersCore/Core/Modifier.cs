using System.Collections.Generic;
using UnityEngine;

namespace ModifiersCore;

internal record Modifier(
    string Id,
    string Name,
    string Description,
    Sprite Icon,
    float Multiplier,
    IEnumerable<string>? Categories = null,
    IEnumerable<string>? MutuallyExclusiveCategories = null,
    IEnumerable<string>? MutuallyExclusiveModifiers = null,
    IEnumerable<string>? RequiresModifiers = null,
    IEnumerable<string>? RequiredByModifiers = null
) : IModifier;