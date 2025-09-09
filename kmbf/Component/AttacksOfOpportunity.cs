//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using System.Reflection;

namespace kmbf.Component
{
    public class UnitPartAooAgainstAllies : AdditiveUnitPart<UnitPartAooAgainstAllies>
    {

    }

    [AllowedOn(typeof(BlueprintUnitFact))]
    public class AooAgainstAllies : OwnedGameLogicComponent<UnitDescriptor>
    {
        public override void OnTurnOn()
        {
            Owner.Ensure<UnitPartAooAgainstAllies>().AddSourceFact(Fact);
        }

        public override void OnTurnOff()
        {
            Owner.Ensure<UnitPartAooAgainstAllies>().RemoveSourceFact(Fact);
        }
    }

    [HarmonyPatch(typeof(UnitCombatEngagementController), nameof(UnitCombatEngagementController.TickUnit))]
    static class UnitCombatEngagementController_TickUnit_Prefix
    {
        // Maybe we shouldn't strip out the patch in any case,
        // but so far we only use this patch for the Necklace of Double Crosses fix,
        // which does get disabled when the user runs Call of the Wild
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return !Main.RunsCallOfTheWild;
        }

        [HarmonyPrefix]
        static void OnTickUnit(UnitCombatEngagementController __instance, UnitEntityData unit)
        {
            if (unit.Get<UnitPartAooAgainstAllies>() != null)
            {
                foreach (UnitGroupMemory.UnitInfo otherUnit in unit.Memory.UnitsList)
                {
                    UnitEntityData otherUnitData = otherUnit.Unit;
                    if (unit.IsAlly(otherUnitData) && otherUnitData.Descriptor.State.IsConscious && unit.IsEngage(otherUnitData) && unit != otherUnitData)
                        unit.CombatState.Engage(otherUnitData);
                }
            }
        }
    }

    // For all external purposes, a unit is not engaged by allies. We only keep the "engagement" for internal purposes for attacks of opportunities on disengagement
    [HarmonyPatch(typeof(UnitCombatState), nameof(UnitCombatState.EngagedBy), MethodType.Getter)]
    static class UnitCombatState_EngagedBy_Prefix
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return !Main.RunsCallOfTheWild;
        }

        [HarmonyPrefix]
        static bool Get_EngagedBy(UnitCombatState __instance, Dictionary<UnitEntityData, TimeSpan> ___m_EngagedBy, ref Dictionary<UnitEntityData, TimeSpan>.KeyCollection __result)
        {
            __result = ___m_EngagedBy.Where(kv => !kv.Key.IsAlly(__instance.Unit)).ToDictionary(kv =>kv.Key, kv => kv.Value).Keys;
            return false;
        }
    }
}
