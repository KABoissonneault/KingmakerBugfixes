
using HarmonyLib;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch
{
    static class TranspilerUtility
    {
        static readonly MethodInfo UnityObjectEquals = AccessTools.Method(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Equals));

        // Expects a Unity.Object in the stack. If the reference is null, the function returns. If not, jumps to label, object still in stack
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

        // Expects and consumes a Unity.Object in the stack. If the reference is null, jump to label. If not, fallthrough to the next instruction
        public static IEnumerable<CodeInstruction> UnityObjectConsumeNullCheckJump(Label ifNull)
        {
            return [ new CodeInstruction(OpCodes.Ldnull)
                , new CodeInstruction(OpCodes.Call, UnityObjectEquals)
                , new CodeInstruction(OpCodes.Brtrue_S, ifNull)
                ];
        }

        // Expects and consumes a Unity.Object in the stack. If the reference is not null, jump to label. If null, fallthrough to the next instruction
        public static IEnumerable<CodeInstruction> UnityObjectConsumeNotNullCheckJump(Label ifNotNull)
        {
            return [ new CodeInstruction(OpCodes.Ldnull)
                , new CodeInstruction(OpCodes.Call, UnityObjectEquals)
                , new CodeInstruction(OpCodes.Brfalse_S, ifNotNull)
                ];
        }
    }
}
