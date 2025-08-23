using System.Text;

namespace kmbf.Patch.KM.UI.Common
{
    // Class is handled in PostBlueprintPatches because of the dependency on BlueprintRoot in the static constructor
    static class UIUtilityTexts_Patch
    {
        public static bool TryAddWordSeparator(StringBuilder descrition, string conjunction)
        {
            var conjunctionStr = conjunction ?? string.Empty;
            if (!string.IsNullOrEmpty(conjunctionStr) && !(conjunctionStr.StartsWith(" ") || conjunctionStr.EndsWith(" ")))
            {
                conjunctionStr = $" {conjunctionStr} ";
            }

            if (!string.IsNullOrEmpty(descrition.ToString()))
            {
                descrition.Append(conjunctionStr);
            }

            return false;
        }
    }
}
