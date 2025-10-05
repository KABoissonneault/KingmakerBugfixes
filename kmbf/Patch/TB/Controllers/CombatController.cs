using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Controllers;

namespace kmbf.Patch.TB.Controllers
{
    [HarmonyPatch]
    static class CombatController_HandleCombatStart_Postfix
    {
        static readonly MethodInfo timeSpanSeconds = AccessTools.PropertyGetter(typeof(TimeSpan), nameof(TimeSpan.Seconds));
        static readonly MethodInfo timeSpanTotalSeconds = AccessTools.PropertyGetter(typeof(TimeSpan), nameof(TimeSpan.TotalSeconds));

        [HarmonyPrepare]
        static bool Prepare()
        {
            return PatchUtility.StartPatch("Turn-based Surprise Round", logOnce: true);
        }

        // The code checks if the timespan between now and the last surprise action is less than 6 seconds ago
        // If the current time is 560.12:49:03 and the protagonist's last surprise action is 00:00:00 (never this run),
        // the difference has a Seconds of 3, and a TotalSeconds of 48,430,143
        // 3 is less than 6, so the protagonist gets the surprise action
        // This is obviously wrong
        [HarmonyPatch(typeof(CombatController), nameof(CombatController.HandleCombatStart))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HandleCombatStart_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return DoPatch(instructions);
        }

        [HarmonyPatch("TurnBased.Controllers.CombatController+<>c__DisplayClass79_0", "<HandleCombatStart>b__3")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> HandleCombatStart_Lambda_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return DoPatch(instructions);
        }

        static IEnumerable<CodeInstruction> DoPatch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(timeSpanSeconds))
                {
                    var newInstruction = new CodeInstruction(OpCodes.Call, timeSpanTotalSeconds);
                    newInstruction.labels = instruction.labels;
                    newInstruction.blocks = instruction.blocks;
                    newInstructions.Add(newInstruction);
                }
                else
                {
                    newInstructions.Add(instruction);
                }
            }

            return newInstructions;
        }
    }
}
