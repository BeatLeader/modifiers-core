using HarmonyLib;
using IPA;
using JetBrains.Annotations;
using Logger = IPA.Logging.Logger;

namespace ModifiersCore;

[Plugin(RuntimeOptions.SingleStartInit), UsedImplicitly]
public class Plugin {
    internal const string PluginName = "ModifiersCore";

    internal static Logger Log { get; private set; } = null!;

    private static Harmony _harmony = null!;

    [Init]
    public Plugin(Logger logger) {
        Log = logger;
        _harmony = new("ModifiersCore");
    }

    [OnStart, UsedImplicitly]
    public void OnApplicationStart() {
        ConfigFileData.Initialize();
        _harmony.PatchAll();
    }

    [OnExit, UsedImplicitly]
    public void OnApplicationExit() {
        _harmony.UnpatchSelf();
        ConfigFileData.Save();
    }
}