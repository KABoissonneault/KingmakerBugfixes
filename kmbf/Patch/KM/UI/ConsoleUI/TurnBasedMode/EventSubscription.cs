using HarmonyLib;
using Kingmaker.UI._ConsoleUI.TurnBasedMode;
using Kingmaker.Utility;

namespace kmbf.Patch.KM.UI.ConsoleUI.TurnBasedMode
{
    [HarmonyPatch]
    static class EventSubscription
    {
        // UnitBuffs in the initiative tracker are cleared and re-created every update, but not disposed
        [HarmonyPatch(typeof(InitiativeTrackerUnitVM), nameof(InitiativeTrackerUnitVM.UpdateBuffs))]
        [HarmonyPrefix]
        static void PatchUpdateBuffs(InitiativeTrackerUnitVM __instance) => __instance.UnitBuffs.ForEach(x => x.Dispose());
    }
}
