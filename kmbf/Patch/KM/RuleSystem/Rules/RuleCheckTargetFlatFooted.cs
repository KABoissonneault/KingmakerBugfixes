using HarmonyLib;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Parts;
using kmbf.Blueprint;

namespace kmbf.Patch.KM.RuleSystem.Rules
{
    [HarmonyPatch(typeof(RuleCheckTargetFlatFooted), nameof(RuleCheckTargetFlatFooted.OnTrigger))]
    static class RuleCheckFlatFooted_OnTrigger_Prefix
    {
        [HarmonyPrefix]
        static bool OnTrigger(RuleCheckTargetFlatFooted __instance)
        {
            if(Main.RunsCallOfTheWild)
                return true;

            __instance.IsFlatFooted =
                __instance.ForceFlatFooted
                || (!__instance.Target.CombatState.CanActInCombat && !__instance.IgnoreVisibility)
                || __instance.Target.Descriptor.State.IsHelpless
                || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Stunned)
                || (!__instance.Target.Memory.Contains(__instance.Initiator) && !__instance.IgnoreVisibility)
                || (UnitPartConcealment.Calculate(__instance.Target, __instance.Initiator, attack: false) == Concealment.Total && !__instance.IgnoreConcealment)
                || __instance.Target.Descriptor.State.HasCondition(UnitCondition.LoseDexterityToAC)
                || ((__instance.Target.Descriptor.State.HasCondition(UnitCondition.Shaken) || __instance.Target.Descriptor.State.HasCondition(UnitCondition.Frightened)) 
                    && (bool)__instance.Initiator.Descriptor.State.Features.ShatterDefenses && TargetHasShatterDefensesBuffFromInitiator(__instance));

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
