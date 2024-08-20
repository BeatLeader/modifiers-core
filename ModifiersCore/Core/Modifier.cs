using System.Collections.Generic;
using UnityEngine;

namespace ModifiersCore;

internal record Modifier(
    string Id,
    string Name,
    string Description,
    Sprite Icon,
    float Multiplier,
    IEnumerable<string>? MutuallyExclusives = null,
    IEnumerable<string>? Requires = null,
    IEnumerable<string>? RequiredBy = null
) : IModifier;