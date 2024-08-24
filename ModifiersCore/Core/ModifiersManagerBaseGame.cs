using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifiersCore;

public static partial class ModifiersManager {
    internal static Dictionary<string, GameplayModifierParamsSO> GameplayModifierParams = new();

    private static IEnumerable<string>? GetCategories(string id) {
        var category = ModifierUtils.GetBaseModifierCategory(id);
        return category != null ? [category] : null;
    }

    private static IEnumerable<string>? GetExclusiveCategories(string id) {
        var category = ModifierUtils.GetBaseModifierExclusiveCategory(id);
        return category != null ? [category] : null;
    }

    internal static void LoadBaseGameModifiers() {
        var modifiers = Resources.FindObjectsOfTypeAll<GameplayModifierParamsSO>();
        foreach (var modifier in modifiers) {
            var id = ModifierUtils.GetBaseModifierIdBySerializedName(modifier.modifierNameLocalizationKey);
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
            return modifiers.Select(x => ModifierUtils.GetBaseModifierIdBySerializedName(x.modifierNameLocalizationKey)).ToArray();
        }
    }
}