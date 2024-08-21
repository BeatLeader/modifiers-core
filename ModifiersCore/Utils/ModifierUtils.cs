using System.Collections.Generic;

namespace ModifiersCore;

internal static class ModifierUtils {
    private delegate void ArrayActionCallback<T>(ref T[] array, T item);

    public static void LinkModifiers(IModifier modifier, IDictionary<string, GameplayModifierParamsSO> dict) {
        HandleModifiers(modifier, dict, ArrayUtils.ExpandArray);
    }

    public static void UnlinkModifiers(IModifier modifier, IDictionary<string, GameplayModifierParamsSO> dict) {
        HandleModifiers(modifier, dict, ArrayUtils.ShrinkArray);
    }

    private static void HandleModifiers(
        IModifier modifier,
        IDictionary<string, GameplayModifierParamsSO> dict,
        ArrayActionCallback<GameplayModifierParamsSO> action
    ) {
        var gameplayModifier = dict[modifier.Id];
        if (modifier.MutuallyExclusives is { } exclusives) {
            foreach (var exclusive in exclusives) {
                var mod = dict[exclusive];
                action(ref mod._mutuallyExclusives, gameplayModifier);
            }
        }
        if (modifier.Requires is { } requires) {
            foreach (var require in requires) {
                var mod = dict[require];
                action(ref mod._requires, gameplayModifier);
            }
        }
        if (modifier.RequiredBy is { } requiredBy) {
            foreach (var required in requiredBy) {
                var mod = dict[required];
                action(ref mod._requiredBy, gameplayModifier);
            }
        }
    }
}