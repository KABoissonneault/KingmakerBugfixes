using HarmonyLib;
using Kingmaker.TurnBasedMode;
using System.Reflection;
using System.Reflection.Emit;
using TurnBased.Controllers;

namespace kmbf.Patch.TB
{
    [HarmonyPatch(typeof(PathVisualizer), nameof(PathVisualizer.Update))]
    internal class PathVisualizer_Transpiler
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase originalMethod)
        {
            return PatchUtility.StartPreparePatch("PathVisualizer Null Reference", originalMethod);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var clearMethod = AccessTools.Method(typeof(PathVisualizer), nameof(PathVisualizer.Clear));
            var get_CurrentTurn = AccessTools.PropertyGetter(typeof(CombatController), nameof(CombatController.CurrentTurn));

            Label clearLabel = new Label();
            List<CodeInstruction> current = instructions.ToList();
            for (int index = 0; index < current.Count; index++)
            {
                CodeInstruction instruction = current[index];
                if (instruction.Calls(clearMethod))
                {
                    clearLabel = current[index - 1].labels[0];
                    break;
                }
            }

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();
            bool firstCurrentTurn = true;
            for (int index = 0; index < current.Count; index++)
            {
                CodeInstruction instruction = current[index];
                if (firstCurrentTurn && instruction.Calls(get_CurrentTurn))
                {
                    // Add a null check before accessing Game.Instance.TurnBasedCombatController.CurrentTurn
                    // If null, clear the path visualizer
                    newInstructions.Add(instruction);
                    newInstructions.AddRange(TranspilerUtility.UnityObjectConsumeNullCheckJump(clearLabel));
                    // Reload the "current turn" (get Game, get CombatController, get Current Turn)
                    newInstructions.Add(current[index - 2]);
                    newInstructions.Add(current[index - 1]);
                    newInstructions.Add(current[index]);

                    firstCurrentTurn = false;
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
