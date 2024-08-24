using HarmonyLib;
using JetBrains.Annotations;

namespace ModifiersCore;

//TODO: remove after adding extended customization
//a little shenanigan for beatleader
[HarmonyPatch(typeof(GameplayModifierToggle), "Start")]
internal static class GameplayModifierTogglePatch {
    [UsedImplicitly]
    private static void Postfix(GameplayModifierToggle __instance) {
        if (__instance.GetComponent<CustomModifierPanel>() is { } panel) {
            panel.SetModifier((ICustomModifier)panel.Modifier);
        }
    }
}