using System.Collections.Generic;
using BGLib.Polyglot;
using HarmonyLib;
using IPA.Utilities;
using UnityEngine;
using UnityEngine.Pool;

namespace ModifiersCore;

[HarmonyPatch]
internal static class GameplayModifiersPanelPatch {
    internal static ModifiersCorePanel CorePanel;
    [HarmonyPatch(typeof(GameplayModifiersPanelController), "Awake"), HarmonyPostfix]
    private static void AwakePostfix(GameplayModifiersPanelController __instance) {
        CorePanel = __instance.gameObject.AddComponent<ModifiersCorePanel>();
    }

    [HarmonyPatch(typeof(GameplayModifiersPanelController), "RefreshTotalMultiplierAndRankUI"), HarmonyPrefix]
    private static bool RefreshRankAndMultiplierPrefix(GameplayModifiersPanelController __instance) {
        var modifiers = ListPool<IModifier>.Get();
        AddEnabledModifiers(__instance, modifiers);
        var totalMultiplier = GetTotalMultiplier(__instance._gameplayModifiersModel, modifiers, 1f);
        var color = totalMultiplier >= 1.0 ? __instance._positiveColor : __instance._negativeColor;
        //multiplier
        __instance._totalMultiplierValueText.text = string.Format(Localization.Instance.SelectedCultureInfo, "{0:P0}", totalMultiplier);
        __instance._totalMultiplierValueText.color = color;
        //rank
        var maxRank = MaxRankForModifiers(__instance._gameplayModifiersModel, modifiers, 1f);
        var rankName = RankModel.GetRankName(maxRank);
        __instance._maxRankValueText.text = rankName;
        __instance._maxRankValueText.color = color;
        ListPool<IModifier>.Release(modifiers);
        return false;
    }

    private static void AddEnabledModifiers(GameplayModifiersPanelController panel, IList<IModifier> modifiers) {
        foreach (var modifier in ModifiersManager.Modifiers) {
            //if base game modifier
            if (ModifiersManager.GameplayModifierParams.TryGetValue(modifier.Id, out var paramsSO)) {
                //acquiring getter
                panel._gameplayModifiersModel._gameplayModifierGetters.TryGetValue(paramsSO, out var getter);
                //adding if enabled
                if (getter?.Invoke(panel._gameplayModifiers) ?? false) {
                    modifiers.Add(modifier);
                }
            } else if (ModifiersManager.GetModifierState(modifier.Id)) {
                modifiers.Add(modifier);
            }
        }
    }

    private static float GetTotalMultiplier(
        GameplayModifiersModelSO model,
        IEnumerable<IModifier> modifiers,
        float energy
    ) {
        var totalMultiplier = 1f;
        foreach (var modifier in modifiers) {
            var name = model._noFailOn0Energy.modifierNameLocalizationKey;
            if (modifier.Name == name) {
                if (energy <= 9.999999747378752E-06) {
                    totalMultiplier += modifier.Multiplier;
                }
            } else {
                totalMultiplier += modifier.Multiplier;
            }
        }
        if (totalMultiplier < 0f) {
            totalMultiplier = 0f;
        }
        return totalMultiplier;
    }

    private static RankModel.Rank MaxRankForModifiers(
        GameplayModifiersModelSO gameplayModifiersModel,
        IEnumerable<IModifier> modifiers,
        float energy
    ) {
        var totalMultiplier = GetTotalMultiplier(gameplayModifiersModel, modifiers, energy);
        var num = ScoreModel.GetModifiedScoreForGameplayModifiersScoreMultiplier(1000000, totalMultiplier);
        return RankModel.GetRankForScore(0, num, 1000000, num);
    }
}