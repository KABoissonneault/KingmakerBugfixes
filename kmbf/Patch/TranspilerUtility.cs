
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch
{
    static class TranspilerUtility
    {
        static readonly MethodInfo UnityObjectEquals = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Equals));

        // Expects a Unity.Object in the stack. If the reference is null, the function returns. If not, jumps to label
        public static IEnumerable<CodeInstruction> UnityObjectNullCheckReturn(Label ifNotNull)
        {
            return [
                new CodeInstruction(OpCodes.Dup)
                , new CodeInstruction(OpCodes.Ldnull)
                , new CodeInstruction(OpCodes.Call, UnityObjectEquals)
                , new CodeInstruction(OpCodes.Brfalse_S, ifNotNull)
                , new CodeInstruction(OpCodes.Pop)
                , new CodeInstruction(OpCodes.Ret)
                ];
        }
    }
}
