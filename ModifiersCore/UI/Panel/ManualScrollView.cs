using HarmonyLib;
using HMUI;
using JetBrains.Annotations;

namespace ModifiersCore;

[HarmonyPatch(typeof(ScrollView), "Awake")]
internal class ManualScrollView : ScrollView {
    private static bool _suppress = true;
    
    [UsedImplicitly]
    private static bool Prefix(ScrollView __instance) {
        return __instance is not ManualScrollView || !_suppress;
    }

    public void ManualAwake() {
        _suppress = false;
        Awake();
        _suppress = true;
    }
}