using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects;
using System.Reflection;

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
            return Main.UMMSettings.BalanceSettings.FixAreaOfEffectDoubleTrigger;
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
}
