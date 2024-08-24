using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifiersCore;

public static partial class ModifiersManager {
    private static readonly Dictionary<string, string> modifierCategories = new() {
        { "OL", "ENERGY" },
        { "FL", "ENERGY" },
        { "NB", "BEATMAP" },
        { "NW", "BEATMAP" },
        { "NA", "BEATMAP" },
        { "GN", "VISUALS" },
        { "DA", "VISUALS" },
        { "SN", "BEATMAP" },
        { "PM", "SCORING" },
        { "SA", "SCORING" },
        { "SS", "SPEED" },
        { "FS", "SPEED" },
        { "SF", "SPEED" },
    };

    private static readonly Dictionary<string, string[]> modifierExclusiveCategories = new() {
        { "OL", ["ENERGY"] },
        { "FL", ["ENERGY"] },
        { "GN", ["VISUALS"] },
        { "DA", ["VISUALS"] },
        { "SS", ["SPEED"] },
        { "FS", ["SPEED"] },
        { "SF", ["SPEED"] },
    };
    
    internal static Dictionary<string, GameplayModifierParamsSO> GameplayModifierParams = new();
    
    private static string ModifierLocalizationKeyToId(string modifierLocalizationKey) {
        if (string.IsNullOrEmpty(modifierLocalizationKey)) return modifierLocalizationKey;

        var idx1 = modifierLocalizationKey.IndexOf('_') + 1;
        var char1 = modifierLocalizationKey[idx1];

        var idx2 = modifierLocalizationKey.IndexOf('_', idx1) + 1;
        var char2 = modifierLocalizationKey[idx2];

        return $"{char.ToUpper(char1).ToString()}{char.ToUpper(char2).ToString()}";
    }

    private static IEnumerable<string>? GetCategories(string id) {
        modifierCategories.TryGetValue(id, out var category);
        return category != null ? [category] : null;
    }

    private static IEnumerable<string>? GetExclusiveCategories(string id) {
        modifierExclusiveCategories.TryGetValue(id, out var categories);
        return categories;
    }

    internal static void LoadBaseGameModifiers() {
        var modifiers = Resources.FindObjectsOfTypeAll<GameplayModifierParamsSO>();
        foreach (var modifier in modifiers) {
            var id = ModifierLocalizationKeyToId(modifier.modifierNameLocalizationKey);
            var mod = new Modifier(
                id,
                modifier.modifierNameLocalizationKey,
                modifier.descriptionLocalizationKey,
                modifier.icon,
                modifier.multiplier,
                Categories: GetCategories(id),
                MutuallyExclusiveCategories: GetExclusiveCategories(id),
                MutuallyExclusiveModifiers: MakeIdsArray(modifier.mutuallyExclusives),
                RequiresModifiers: MakeIdsArray(modifier.requires),
                RequiredByModifiers: MakeIdsArray(modifier.requiredBy)
            );
            GameplayModifierParams[id] = modifier;
            AddModifierInternal(mod);
        }

        static string[] MakeIdsArray(GameplayModifierParamsSO[] modifiers) {
            return modifiers.Select(x => ModifierLocalizationKeyToId(x.modifierNameLocalizationKey)).ToArray();
        }
    }
}