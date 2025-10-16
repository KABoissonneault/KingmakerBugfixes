using HarmonyLib;
using Kingmaker;
using Kingmaker.Utility;
using Kingmaker.Visual.Critters;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.Visual.Critters
{
    [HarmonyPatch(typeof(Familiar))]
    internal class Familiar_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPreparePatch("Familiar.Update Null Reference Exception", original);
        }

        [HarmonyPatch(nameof(Familiar.Update)), HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Update(IEnumerable<CodeInstruction> instructions)
        {
            var master_get = AccessTools.PropertyGetter(typeof(Familiar), nameof(Familiar.Master));
            var hideInCapital_get = AccessTools.PropertyGetter(typeof(Familiar), nameof(Familiar.HideInCapital));
            var gameInstance_get = AccessTools.PropertyGetter(typeof(Game), nameof(Game.Instance));
            var gameCurrentlyLoadedArea_get = AccessTools.PropertyGetter(typeof(Game), nameof(Game.CurrentlyLoadedArea));

            var foundMaster = false;
            var brTrueInstructionNext = false;

            List<CodeInstruction> result = new List<CodeInstruction>();
            foreach(var instruction in instructions)
            {
                if(!foundMaster)
                {
                    if(instruction.Calls(master_get))
                    {
                        foundMaster = true;
                        brTrueInstructionNext = true;
                    }
                }
                else if(brTrueInstructionNext)
                {
                    var skipRetLabel = (Label)instruction.operand;
                    brTrueInstructionNext = false;

                    result.Add(instruction);
                    // if (this.Master == null) => if (this.Master == null || (HideInCapital && Game.Instance.CurrentlyLoadedArea == null))
                    result.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    result.Add(new CodeInstruction(OpCodes.Call, hideInCapital_get));
                    result.Add(new CodeInstruction(OpCodes.Brfalse_S, skipRetLabel));
                    result.Add(new CodeInstruction(OpCodes.Call, gameInstance_get));
                    result.Add(new CodeInstruction(OpCodes.Callvirt, gameCurrentlyLoadedArea_get));
                    result.AddRange(TranspilerUtility.UnityObjectConsumeNotNullCheckJump(skipRetLabel));

                    continue;
                }

                result.Add(instruction);
            }

            return result;
        }
    }
}
