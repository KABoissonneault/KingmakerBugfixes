using HarmonyLib;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.FactLogic;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.UnitLogic.FactLogic
{
    // In KM, "EnhancementTotal" is a field used only for the purpose of checking whether the attack can bypass Damage Reduction with a "Enhancement" requirement
    // In both KM and WotR, the "enchantment cost" of the enchantments on the weapon are added to the normal enhancement value
    // This means that a +3 corrosive flaming sword gets a +1 from both corrosive and flaming for a total of +5, for bypassing DR
    // This creates issues
    // 1) This does not match tabletop
    // 2) Enchantments are easily counted twice (they add to enhancement, and also have an enchantment cost to add to the total enhancement)
    // 3) Random enchantments like "Composite" or "Thrown" add +1 (but not masterwork), when they have nothing to do with how magical the item is
    //
    // For this reason, this patch replaces the use of EnchantmentTotal with the regular Enchantment
    // This is a different approach than the one used in WotR by the DragonFixes mod
    [HarmonyPatch]
    internal class AddDamageResistancePhysical_OnTrigger_Transpiler
    {
        static readonly MethodInfo Enchantment = AccessTools.PropertyGetter(typeof(PhysicalDamage), nameof(PhysicalDamage.Enchantment));
        static readonly MethodInfo EnchantmentTotal = AccessTools.PropertyGetter(typeof(PhysicalDamage), nameof(PhysicalDamage.EnchantmentTotal));

        static CodeInstruction PatchOperand(CodeInstruction i)
        {
            i.operand = Enchantment;
            return i;
        }

        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return Main.UMMSettings.BalanceSettings.FixWeaponEnhancementDamageReduction;
        }

        [HarmonyPatch(typeof(AddDamageResistancePhysical), nameof(AddDamageResistancePhysical.Bypassed))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddDamageResistancePhysical_Bypassed_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Select(i => i.Calls(EnchantmentTotal) ? PatchOperand(i) : i);
        }

        [HarmonyPatch(typeof(AddDamageResistancePhysical), nameof(AddDamageResistancePhysical.CheckBypassedByAlignment))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddDamageResistancePhysical_CheckBypassedByAlignment_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Select(i => i.Calls(EnchantmentTotal) ? PatchOperand(i) : i);
        }

        [HarmonyPatch(typeof(AddEffectRegeneration), nameof(AddEffectRegeneration.OnEventDidTrigger))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddEffectRegeneration_OnEventDidTrigger_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Select(i => i.Calls(EnchantmentTotal) ? PatchOperand(i) : i);
        }

        [HarmonyPatch(typeof(AddIncorporealDamageDivisor), nameof(AddIncorporealDamageDivisor.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AddIncorporealDamageDivisor_OnEventAboutToTrigger_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Select(i => i.Calls(EnchantmentTotal) ? PatchOperand(i) : i);
        }

        [HarmonyPatch(typeof(GhostCriticalAndPrecisionImmunity), nameof(GhostCriticalAndPrecisionImmunity.OnEventAboutToTrigger))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GhostCriticalAndPrecisionImmunity_OnEventAboutToTrigger_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return instructions.Select(i => i.Calls(EnchantmentTotal) ? PatchOperand(i) : i);
        }
    }
}
