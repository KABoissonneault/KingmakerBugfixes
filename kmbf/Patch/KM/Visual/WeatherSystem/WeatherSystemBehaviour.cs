using HarmonyLib;
using Kingmaker.Visual.WeatherSystem;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.Visual.WeatherSystem
{
    // Add missing null check in WeatherSystemBehaviour.Update
    [HarmonyPatch(typeof(WeatherSystemBehaviour), nameof(WeatherSystemBehaviour.Update))]
    static class WeatherSystemBehaviour_Update_Transpiler
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase originalMethod)
        {
            return PatchUtility.StartPreparePatch("WeatherSystemBehaviour Null Reference", originalMethod);
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Update_Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo cameraRig = AccessTools.Field(typeof(WeatherSystemBehaviour), nameof(WeatherSystemBehaviour.m_CameraRig));
            MethodInfo GameInstanceGetter = AccessTools.PropertyGetter(typeof(Kingmaker.Game), nameof(Kingmaker.Game.Instance));

            CodeInstruction[] currentInstructions = instructions.ToArray();
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            for(int i = 0; i < currentInstructions.Length; ++i)
            {
                CodeInstruction currentInstruction = currentInstructions[i];
                if(currentInstruction.Calls(GameInstanceGetter))
                {
                    Label notNullLabel = generator.DefineLabel();
                    CodeInstruction notNullInstruction = currentInstructions[i + 3].Clone();
                    notNullInstruction.labels.Add(notNullLabel);

                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0) { labels = currentInstruction.labels.ToList() });
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldfld, cameraRig));
                    newInstructions.AddRange(TranspilerUtility.UnityObjectNullCheckReturn(ifNotNull: notNullLabel));
                    newInstructions.Add(notNullInstruction);

                    i += 3; // Skip the original "Game.Instance.UI.GetCameraRig()"
                }
                else
                {
                    newInstructions.Add(currentInstruction);
                }
            }

            return newInstructions;

        }
    }
}
