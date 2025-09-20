using HarmonyLib;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;

namespace kmbf.Patch.KM.Designers.Mechanics.Facts
{
    [HarmonyPatch(typeof(AbilityScoreCheckBonus), nameof(AbilityScoreCheckBonus.OnD20AboutToTrigger))]
    internal class AbilityScoreCheckBonus_OnEventAboutToTrigger_Patch
    {
        // Ability Score Check Bonus has three issues:
        // 1) Uses Reason to get the RuleSkillCheck, which is not valid if the skill check comes from a fact (ex: Icy Prison - Entanglement)
        // 2) It adds a temporary modifier that is not actually registered to anything
        //
        // The solution here is to apply the modifier to the Skill Check's "Bonus"
        [HarmonyPrefix]
        static bool OnD20AboutToTrigger(RuleRollD20 evt, AbilityScoreCheckBonus __instance)
        {
            RuleSkillCheck ruleSkillCheck = Rulebook.CurrentContext.PreviousEvent as RuleSkillCheck;
            if (ruleSkillCheck?.StatType == __instance.Stat)
            {
                var stat = ruleSkillCheck.Initiator.Stats.GetStat(__instance.Stat);
                if (stat != null)
                {
                    ruleSkillCheck.AddTemporaryModifier(ruleSkillCheck.Bonus.AddModifier(__instance.Bonus.Calculate(__instance.Context), __instance, __instance.Descriptor));
                }
            }

            return false;
        }
    }
}
