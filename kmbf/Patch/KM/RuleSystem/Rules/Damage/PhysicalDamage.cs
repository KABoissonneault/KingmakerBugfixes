using HarmonyLib;
using Kingmaker.RuleSystem.Rules.Damage;

namespace kmbf.Patch.KM.RuleSystem.Rules.Damage
{
    [HarmonyPatch(typeof(PhysicalDamage), nameof(PhysicalDamage.CreateTypeDescription))]
    internal class PhysicalDamage_CreateTypeDescription_Postfix
    {
        [HarmonyPostfix]
        static void CreateTypeDescriptionPostfix(ref DamageTypeDescription __result, PhysicalDamage __instance)
        {
            __result.Physical.Enhancement = __instance.Enchantment;
            __result.Physical.EnhancementTotal = __instance.EnchantmentTotal;
        }
    }
}
