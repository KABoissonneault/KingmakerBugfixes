using HarmonyLib;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Conditions;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.Kingdom.Conditions
{
    [HarmonyPatch]
    static class KingdomStatCheck_CheckCondition_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPreparePatch("Kingdom Stat Check Condition", original);
        }

        [HarmonyPatch(typeof(KingdomStatCheck), nameof(KingdomStatCheck.CheckCondition)), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> CheckCondition_Transpile(IEnumerable<CodeInstruction> instructions)
        {
            var get_EffectiveValue = AccessTools.PropertyGetter(typeof(KingdomStats.Stat), nameof(KingdomStats.Stat.EffectiveValue));
            var value = AccessTools.Field(typeof(KingdomStats.Stat), nameof(KingdomStats.Stat.Value));

            List<CodeInstruction> inst = instructions.Select(i => i.Calls(get_EffectiveValue) ? TranspilerUtility.NewInstructionFrom(OpCodes.Ldfld, value, i) : i).ToList();
            return inst;
        }
    }
}
