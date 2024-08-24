using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ModifiersCore;

public static partial class ModifiersManager {
    public static readonly Dictionary<string, string> defaultModifierIds = new() {
        { "MODIFIER_ONE_LIFE", "IF" },
        { "MODIFIER_FOUR_LIVES", "BE" },
        { "MODIFIER_NO_BOMBS", "NB" },
        { "MODIFIER_NO_OBSTACLES", "NO" },
        { "MODIFIER_NO_ARROWS", "NA" },
        { "MODIFIER_GHOST_NOTES", "GN" },
        { "MODIFIER_DISAPPEARING_ARROWS", "DA" },
        { "MODIFIER_SMALL_CUBES", "SC" },
        { "MODIFIER_PRO_MODE", "PM" },
        { "MODIFIER_STRICT_ANGLES", "SA" },
        { "MODIFIER_SLOWER_SONG", "SS" },
        { "MODIFIER_FASTER_SONG", "FS" },
        { "MODIFIER_SUPER_FAST_NOTES", "SF" },
        { "MODIFIER_SUPER_FAST_SONG", "SF" },
        { "MODIFIER_ZEN_MODE", "ZM" },
        { "MODIFIER_NO_FAIL_ON_0_ENERGY", "NF" },
    };

    public static readonly Dictionary<string, string> modifierCategories = new() {
        { "IF", DefaultCategory.ENERGY },
        { "BE", DefaultCategory.ENERGY },
        { "NB", DefaultCategory.BEATMAP },
        { "NO", DefaultCategory.BEATMAP },
        { "NA", DefaultCategory.BEATMAP },
        { "GN", DefaultCategory.VISUALS },
        { "DA", DefaultCategory.VISUALS },
        { "SC", DefaultCategory.BEATMAP },
        { "PM", DefaultCategory.SCORING },
        { "SA", DefaultCategory.SCORING },
        { "SS", DefaultCategory.SPEED },
        { "FS", DefaultCategory.SPEED },
        { "SF", DefaultCategory.SPEED },
    };

    private static readonly Dictionary<string, string[]> modifierExclusiveCategories = new() {
        { "IF", [DefaultCategory.ENERGY] },
        { "BE", [DefaultCategory.ENERGY] },
        { "GN", [DefaultCategory.VISUALS] },
        { "DA", [DefaultCategory.VISUALS] },
        { "SS", [DefaultCategory.SPEED] },
        { "FS", [DefaultCategory.SPEED] },
        { "SF", [DefaultCategory.SPEED] },
    };

    private static readonly Dictionary<string, Func<GameplayModifiers, bool>> modifierGetters = new() {
        { "IF", x => x.instaFail },
        { "BE", x => x.energyType is GameplayModifiers.EnergyType.Battery },
        { "NB", x => x.noBombs },
        { "NO", x => x.enabledObstacleType is GameplayModifiers.EnabledObstacleType.NoObstacles },
        { "NA", x => x.noArrows },
        { "GN", x => x.ghostNotes },
        { "DA", x => x.disappearingArrows },
        { "SC", x => x.smallCubes },
        { "PM", x => x.proMode },
        { "SA", x => x.strictAngles },
        { "SS", x => x.songSpeed is GameplayModifiers.SongSpeed.Slower },
        { "FS", x => x.songSpeed is GameplayModifiers.SongSpeed.Faster },
        { "SF", x => x.songSpeed is GameplayModifiers.SongSpeed.SuperFast },
        { "ZM", x => x.zenMode },
        { "NF", x => x.noFailOn0Energy },
    };

    internal static Dictionary<string, GameplayModifierParamsSO> GameplayModifierParams = new();

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
            var id = defaultModifierIds[modifier.modifierNameLocalizationKey];
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
            return modifiers.Select(x => defaultModifierIds[x.modifierNameLocalizationKey]).ToArray();
        }
    }
}