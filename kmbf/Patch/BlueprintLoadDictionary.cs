using HarmonyLib;
using Kingmaker.Blueprints;

namespace kmbf.Patch
{
    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;
        static public bool Loaded { get => loaded; }

        [HarmonyPostfix]
        public static void BlueprintPatch()
        {
            if (loaded) return;
            loaded = true;

            AbilitiesFixes.Apply();
            ItemFixes.Apply();
            TextFixes.Apply();
            EventFixes.Apply();
            KingdomFixes.Apply();
            OptionalFixes.ApplyAllEnabledFixes();

            // Harmony patches applied manually after Blueprints are loaded
            PostBlueprintPatches.Apply();
        }
    }
}
