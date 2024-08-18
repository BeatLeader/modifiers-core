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
    IEnumerable<string>? MutuallyExclusives = null,
    IEnumerable<string>? Requires = null,
    IEnumerable<string>? RequiredBy = null
) : ICustomModifier;