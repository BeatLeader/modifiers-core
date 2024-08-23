namespace ModifiersCore;

public static class ModifierUtils {
    public static string ModifierLocalizationKeyToId(string modifierLocalizationKey) {
        if (string.IsNullOrEmpty(modifierLocalizationKey)) return modifierLocalizationKey;

        var idx1 = modifierLocalizationKey.IndexOf('_') + 1;
        var char1 = modifierLocalizationKey[idx1];

        var idx2 = modifierLocalizationKey.IndexOf('_', idx1) + 1;
        var char2 = modifierLocalizationKey[idx2];

        return $"{char.ToUpper(char1)}{char.ToUpper(char2)}";
    }
}