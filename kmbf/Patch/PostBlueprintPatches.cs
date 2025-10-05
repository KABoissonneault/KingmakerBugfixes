using HarmonyLib;
using Kingmaker.UI.Common;
using kmbf.Patch.KM.UI.Common;
using System.Text;

namespace kmbf.Patch
{
    static class PostBlueprintPatches
    {
        public static void Apply()
        {
            Main.Log.Log("Starting late patches");

            if (PatchUtility.StartPatch("Unit Inspector Word Separator"))
            {
                var tryAddWordSeparator = AccessTools.Method(typeof(UIUtilityTexts), nameof(UIUtilityTexts.TryAddWordSeparator), [typeof(StringBuilder), typeof(string)]);
                Main.HarmonyInstance.Patch(tryAddWordSeparator, prefix: new HarmonyMethod(typeof(UIUtilityTexts_Patch), nameof(UIUtilityTexts_Patch.TryAddWordSeparator)));
            }
        }
    }
}
