using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;

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
        [HarmonyPrefix]
        static void OnTickUnit(UnitCombatEngagementController __instance, UnitEntityData unit)
        {
            if (Main.RunsCallOfTheWild)
                return;

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
        [HarmonyPrefix]
        static bool Get_EngagedBy(UnitCombatState __instance, Dictionary<UnitEntityData, TimeSpan> ___m_EngagedBy, ref Dictionary<UnitEntityData, TimeSpan>.KeyCollection __result)
        {
            if (Main.RunsCallOfTheWild) return true;

            __result = ___m_EngagedBy.Where(kv => !kv.Key.IsAlly(__instance.Unit)).ToDictionary(kv =>kv.Key, kv => kv.Value).Keys;
            return false;
        }
    }
}
