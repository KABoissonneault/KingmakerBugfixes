using HarmonyLib;
using Kingmaker.RuleSystem.Rules.Damage;
using System.Reflection;

namespace kmbf.Patch.KM.RuleSystem.Rules.Damage
{
    // When creating a Physical Damage type description from an existing damage bundle, the Weapon Enhancement and Enhancement Total
    // are lost. This can prevent things like Sneak Attacks bypassing Damage Reduction that the weapon can
    [HarmonyPatch(typeof(PhysicalDamage), nameof(PhysicalDamage.CreateTypeDescription))]
    internal class PhysicalDamage_CreateTypeDescription_Postfix
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPreparePatch("PhysicalDamage Enhancement Propagation", original);
        }

        [HarmonyPostfix]
        static void CreateTypeDescriptionPostfix(ref DamageTypeDescription __result, PhysicalDamage __instance)
        {
            __result.Physical.Enhancement = __instance.Enchantment;
            __result.Physical.EnhancementTotal = __instance.EnchantmentTotal;
        }
    }
}
