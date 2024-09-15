using HarmonyLib;
using JetBrains.Annotations;

namespace ModifiersCore;

[HarmonyPatch(typeof(PlayerDataModel), "Load")]
internal static class PlayerDataModelPatch {
    public static PlayerData? PlayerData => _playerDataModel?.playerData;

    private static PlayerDataModel? _playerDataModel;

    [UsedImplicitly]
    private static void Postfix(PlayerDataModel __instance) {
        _playerDataModel = __instance;
    }
}