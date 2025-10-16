
using HarmonyLib;
using Kingmaker;
using Kingmaker.RuleSystem.Rules;
using System.Reflection;

namespace kmbf.Patch.KM.RuleSystem.Rules
{
    // This patch allows spells to bypass spell resistance if used on an ally out of combat
    // Tabletop always allows to remove Spell Resistance on demand, but it requires a standard action
    // It makes sense to always freely allow this to happen implicitly out of combat
    [HarmonyPatch(typeof(RuleSpellResistanceCheck), nameof(RuleSpellResistanceCheck.HasResistanceRoll), MethodType.Getter)]
    static class RuleSpellResistanceCheck_HasResistanceRoll_Postfix
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPrepareQualityOfLifePatch("Spell Resistance on Buffs", original, nameof(QualityOfLifeSettings.BypassSpellResistanceForOutOfCombatBuffs));
        }

        [HarmonyPostfix]
        static void Postfix(RuleSpellResistanceCheck __instance, ref bool __result)
        {
            if (__result && !Game.Instance.Player.IsInCombat && !__instance.Initiator.Group.IsEnemy(__instance.Target.Group))
                __result = false;
        }
    }
}
