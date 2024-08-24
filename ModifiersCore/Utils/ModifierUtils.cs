using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ModifiersCore;

[PublicAPI]
public static class ModifierUtils {
    #region Maps

    private static readonly Dictionary<string, string> defaultModifierIds = new() {
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
        { "MODIFIER_NO_FAIL_ON_0_ENERGY", "NF" }
    };

    private static readonly Dictionary<string, string> modifierCategories = new() {
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
        { "SF", DefaultCategory.SPEED }
    };

    private static readonly Dictionary<string, string> modifierExclusiveCategory = new() {
        { "IF", DefaultCategory.ENERGY },
        { "BE", DefaultCategory.ENERGY },
        { "GN", DefaultCategory.VISUALS },
        { "DA", DefaultCategory.VISUALS },
        { "SS", DefaultCategory.SPEED },
        { "FS", DefaultCategory.SPEED },
        { "SF", DefaultCategory.SPEED }
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
        { "NF", x => x.noFailOn0Energy }
    };

    #endregion

    #region Methods

    public static string GetBaseModifierIdBySerializedName(string name) {
        if (!defaultModifierIds.TryGetValue(name, out name)) {
            throw new ArgumentException($"Unknown base game modifier name: {name}");
        }
        return name;
    }

    public static bool GetGameplayModifierState(this GameplayModifiers modifiers, string id) {
        if (!modifierGetters.TryGetValue(id, out var getter)) {
            throw new ArgumentException($"Unknown base game modifier id: {id}");
        }
        return getter(modifiers);
    }

    internal static string? GetBaseModifierCategory(string id) {
        modifierCategories.TryGetValue(id, out var category);
        return category;
    }

    internal static string? GetBaseModifierExclusiveCategory(string id) {
        modifierExclusiveCategory.TryGetValue(id, out var category);
        return category;
    }

    #endregion
}