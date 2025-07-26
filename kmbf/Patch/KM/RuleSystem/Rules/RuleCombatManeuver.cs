using HarmonyLib;
using Kingmaker.RuleSystem.Rules;

namespace kmbf.Patch.KM.RuleSystem.Rules
{
    [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.IsSuccessRoll))]
    static class RuleCombatManeuver_IsSuccessRoll_Patch
    {
        // If a unit is immune to a Combat Maneuver (ex: Freedom of Movement's immunity to Grapple),
        // IsSuccess will be checked with a d20 roll of 0 and Target CMD of 0, which always succeeds,
        // rather than always fails
        // This fix makes a d20 roll of 0 always fail
        [HarmonyPostfix]
        static bool Fix(bool __result, int d20) => d20 != 0 && __result;
    }
}
