using HarmonyLib;
using Kingmaker.Blueprints;

namespace kmbf.Patch.BP
{
    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;
        static public bool Loaded { get => loaded; }

        [HarmonyPostfix]
        public static void BlueprintPatch(LibraryScriptableObject __instance)
        {
            if (loaded) return;
            loaded = true;
                        
            AbilitiesFixes.Apply();
            ClassFixes.Apply();
            ItemFixes.Apply();
            TextFixes.Apply();
            EventFixes.Apply();
            KingdomFixes.Apply();

            // Harmony patches applied manually after Blueprints are loaded
            PostBlueprintPatches.Apply();
        }
    }
}
