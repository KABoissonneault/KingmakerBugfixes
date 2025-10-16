using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.EntitySystem.Entities
{
    [HarmonyPatch(typeof(AreaEffectEntityData), MethodType.Constructor, new Type[]{
            typeof(AreaEffectView),
            typeof(MechanicsContext),
            typeof(BlueprintAbilityAreaEffect),
            typeof(TargetWrapper),
            typeof(TimeSpan),
            typeof(TimeSpan?),
            typeof(bool),
        })]
    static class AreaOfEffectsTick_Round_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPrepareBalancePatch("Area of Effect Double Trigger", original, nameof(BalanceSettings.FixAreaOfEffectDoubleTrigger));
        }

        // Prevents Area of Effect spells with both a UnitEnter event and a round event from triggering twice
        [HarmonyPostfix]
        static void Postfix(AreaEffectEntityData __instance)
        {
            if (__instance.Blueprint.GetComponent<AbilityAreaEffectRunAction>()?.UnitEnter?.HasActions ?? false)
            {
                __instance.m_TimeToNextRound = 6f;
            }
        }
    }

    [HarmonyPatch(typeof(AreaEffectEntityData), nameof(AreaEffectEntityData.ShouldUnitBeInside))]
    static class AreaOfEffectsTick_ShouldUnitBeInside_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return PatchUtility.StartPreparePatch("AreaEffectEntityData Null Reference", original);
        }

        // Prevents Null Reference Exception on null Unit.View
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var get_Shape = AccessTools.PropertyGetter(typeof(AreaEffectView), nameof(AreaEffectView.Shape));
            var get_View = AccessTools.FirstProperty(typeof(UnitEntityData), p => p.Name == nameof(UnitEntityData.View)).GetGetMethod();

            Label falseLabel = (Label)instructions.First(i => i.opcode == OpCodes.Brfalse).operand;
            bool insertNext = false;
            bool done = false;
            List<CodeInstruction> result = new();

            foreach(CodeInstruction i in instructions)
            {
                if(insertNext)
                {
                    result.Add(i);
                    result.Add(new CodeInstruction(OpCodes.Ldarg_1));
                    result.Add(new CodeInstruction(OpCodes.Callvirt, get_View));
                    result.AddRange(TranspilerUtility.UnityObjectConsumeNullCheckJump(falseLabel));
                    insertNext = false;
                    done = true;
                }
                else if(!done && i.opcode == OpCodes.Callvirt && i.operand.Equals(get_Shape))
                {
                    result.Add(i);
                    insertNext = true;
                }
                else
                {
                    result.Add(i);
                }
            }

            return result;
        }
    }
}
