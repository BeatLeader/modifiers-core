using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace ModifiersCore;

internal class ConfigFileData {
    #region Serialization

    private const string ConfigPath = $"UserData\\{Plugin.PluginName}.json";

    public static void Initialize() {
        if (File.Exists(ConfigPath)) {
            var text = File.ReadAllText(ConfigPath);
            try {
                Instance = JsonConvert.DeserializeObject<ConfigFileData>(text)!;
                Plugin.Log.Debug("Config loaded.");
                return;
            } catch (Exception ex) {
                Plugin.Log.Error($"Failed to load config\n{ex}");
            }
        }
        Instance = new();
    }

    public static void Save() {
        try {
            var text = JsonConvert.SerializeObject(Instance, Formatting.Indented);
            File.WriteAllText(ConfigPath, text);
            Plugin.Log.Debug("Config saved.");
        } catch (Exception ex) {
            Plugin.Log.Error($"Failed to save config\n{ex}");
        }
    }

    public static ConfigFileData Instance { get; private set; } = null!;

    #endregion

    #region Config

    public Dictionary<string, bool> ModifierStates { get; set; } = new();

    #endregion
}