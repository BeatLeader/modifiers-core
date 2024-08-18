using System.Collections.Generic;
using BGLib.Polyglot;
using HarmonyLib;
using IPA.Utilities;

namespace ModifiersCore;

[HarmonyPatch]
internal static class GameplayModifiersPanelPatch {
    [HarmonyPatch(typeof(GameplayModifiersPanelController), "Awake"), HarmonyPostfix]
    private static void AwakePostfix(GameplayModifiersPanelController __instance) {
        __instance.gameObject.AddComponent<ModifiersCorePanel>();
    }

    [HarmonyPatch(typeof(GameplayModifiersPanelController), "RefreshTotalMultiplierAndRankUI"), HarmonyPrefix]
    private static bool RefreshRankAndMultiplierPrefix(GameplayModifiersPanelController __instance) {
        var modifiers = CreateModifiersList(__instance);
        var totalMultiplier = __instance._gameplayModifiersModel.GetTotalMultiplier(modifiers, 1f);
        var color = totalMultiplier >= 1.0 ? __instance._positiveColor : __instance._negativeColor;
        //multiplier
        __instance._totalMultiplierValueText.text = string.Format(Localization.Instance.SelectedCultureInfo, "{0:P0}", totalMultiplier);
        __instance._totalMultiplierValueText.color = color;
        //rank
        var maxRank = MaxRankForModifiers(modifiers, __instance._gameplayModifiersModel, 1f);
        var rankName = RankModel.GetRankName(maxRank);
        __instance._maxRankValueText.text = rankName;
        __instance._maxRankValueText.color = color;
        return false;
    }

    private static List<GameplayModifierParamsSO> CreateModifiersList(GameplayModifiersPanelController panel) {
        var modifiersList = panel._gameplayModifiersModel.CreateModifierParamsList(panel._gameplayModifiers);
        foreach (var (id, modifier) in ModifiersManager.ModifierParams) {
            var state = ModifiersManager.GetModifierState(id);
            if (state) modifiersList.Add(modifier);
        }
        return modifiersList;
    }

    private static RankModel.Rank MaxRankForModifiers(
        List<GameplayModifierParamsSO> modifiers,
        GameplayModifiersModelSO gameplayModifiersModel,
        float energy
    ) {
        var num = gameplayModifiersModel.MaxModifiedScoreForMaxMultipliedScore(1000000, modifiers, energy);
        return RankModel.GetRankForScore(0, num, 1000000, num);
    }
}