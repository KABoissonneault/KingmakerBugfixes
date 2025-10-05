using HarmonyLib;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UI.CombatText;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.UI.CombatText
{
    [HarmonyPatch(typeof(CombatTextManager), "OnEventDidTrigger", new[] { typeof(RuleSavingThrow) })]
    static class DisplayFix_CombatTextManager_SavingThrow_Patch
    {
        static readonly MethodInfo RuleSavingThrow_SuccessBonus = AccessTools.PropertyGetter(typeof(RuleSavingThrow), "SuccessBonus");
        static readonly MethodInfo RuleSavingThrow_StatValue = AccessTools.PropertyGetter(typeof(RuleSavingThrow), "StatValue");

        [HarmonyPrepare]
        static bool Prepare()
        {
            return PatchUtility.StartPatch("Saving Throw Display", logOnce: true);
        }

        // When showing a Saving Throw as an "overtip" (on-field widget), the game shows "Roll vs DC" by default
        // If an enemy rolls 9 on a Reflex Saving Throw with +3 Reflex against a DC of 12, the game will show "Saving Throw Success (9 vs 12)"
        // With this patch, we subtract the modifier from the target, showing "9 vs 9" instead, since that's the actual roll the target
        // needs to do
        // This matches how attack rolls work
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);
            int target = FindInsertionTarget(codes);
            codes.InsertRange(target + 4,
                new CodeInstruction[] {
                    codes[target].Clone(),
                    codes[target + 1].Clone(),
                    new CodeInstruction(OpCodes.Call, RuleSavingThrow_StatValue),
                    new CodeInstruction(OpCodes.Sub),
                });
            return codes.AsEnumerable();
        }

        private static int FindInsertionTarget(List<CodeInstruction> codes)
        {
            for (int i = 0; i < codes.Count; i++)
            {
                if (codes[i].opcode == OpCodes.Callvirt && codes[i].Calls(RuleSavingThrow_SuccessBonus))
                {
                    return i - 2;
                }
            }
            Main.Log.Error("DisplayFix_OvertipsVM_SavingThrow_Patch: COULD NOT FIND TARGET");
            return -1;
        }
    }
}
