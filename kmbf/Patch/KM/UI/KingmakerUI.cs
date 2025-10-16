using HarmonyLib;
using Kingmaker.UI;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.UI
{
    // Fix a null reference access when entering the Main Menu
    [HarmonyPatch(typeof(BugReportCanvas), nameof(BugReportCanvas.OnEnable))]
    static class BugReportCanvas_OnEnable_Transpiler
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPreparePatch("BugReportCanvas Null Reference", original);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnEnable_Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            MethodInfo objectEquals = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Equals));
            FieldInfo bugReportButtonGet = AccessTools.Field(typeof(BugReportCanvas), nameof(BugReportCanvas.BugReportButton));

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            foreach(CodeInstruction instruction in instructions)
            {
                // Add a null check before loading the BugReportButton field
                if (instruction.opcode == OpCodes.Ldfld && instruction.operand.Equals(bugReportButtonGet))
                {
                    var jumpTarget = new CodeInstruction(OpCodes.Nop, null);
                    var jumpLabel = generator.DefineLabel();
                    jumpTarget.labels.Add(jumpLabel);

                    newInstructions.Add(instruction);
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldnull));
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, objectEquals));
                    newInstructions.Add(new CodeInstruction(OpCodes.Brfalse_S, jumpLabel));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ret));
                    newInstructions.Add(jumpTarget);
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0, null));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, instruction.operand));
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
