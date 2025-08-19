using HarmonyLib;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using kmbf.Blueprint;
using System.Reflection;

namespace kmbf.Patch.KM.RuleSystem.Rules
{
    // Changes the rules around Shatter Defenses to
    // 1. Not check for Shaken/Frightened (it's checked on hit)
    // 2. Check for the "Shatter Defenses Hit" buff
    // Patch should not replace the base logic if not using the fix
    [HarmonyPatch(typeof(RuleCheckTargetFlatFooted), nameof(RuleCheckTargetFlatFooted.OnTrigger))]
    static class RuleCheckFlatFooted_OnTrigger_Prefix
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return !Main.RunsCallOfTheWild && Main.UMMSettings.BalanceSettings.FixShatterDefenses;
        }

        [HarmonyPrefix]
        static bool OnTrigger(RuleCheckTargetFlatFooted __instance)
        {
            __instance.IsFlatFooted =
                __instance.ForceFlatFooted
                || (!__instance.Target.CombatState.CanActInCombat && !__instance.IgnoreVisibility)
                || __instance.Target.Descriptor.State.IsHelpless
                || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Stunned)
                || (!__instance.Target.Memory.Contains(__instance.Initiator) && !__instance.IgnoreVisibility)
                || (UnitPartConcealment.Calculate(__instance.Target, __instance.Initiator, attack: false) == Concealment.Total && !__instance.IgnoreConcealment)
                || __instance.Target.Descriptor.State.HasCondition(UnitCondition.LoseDexterityToAC)
                || ((bool)__instance.Initiator.Descriptor.State.Features.ShatterDefenses && TargetHasShatterDefensesBuffFromInitiator(__instance));

            return false;
        }

        static bool TargetHasShatterDefensesBuffFromInitiator(RuleCheckTargetFlatFooted rule)
        {
            if (rule.Target?.Buffs == null)
                return false;

            return rule.Target.Buffs.Enumerable.Any(b => b.Blueprint.AssetGuid == BlueprintBuffGuid.KMBF_ShatterDefensesHit.guid && b.MaybeContext?.MaybeCaster == rule.Initiator);
        }
    }
}
