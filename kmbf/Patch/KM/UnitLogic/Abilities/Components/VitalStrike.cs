using HarmonyLib;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities.Components;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.KM.UnitLogic.Abilities.Components
{
    // UnitEngagementExtension.GetThreatHand does not work with Ranged weapon, and therefore fails to resolve AbilityCustomMeleeAttack.Deliver for Vital Strike,
    // which should work on ranged weapons
    // Patch in a custom function that simply returns the primary hand weapon for Vital Strike
    // Taken from Call of the Wild
    [HarmonyPatch(typeof(AbilityCustomMeleeAttack), nameof(AbilityCustomMeleeAttack.Deliver), MethodType.Enumerator)]
    static class AbilityCustomMeleeAttack_Deliver_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return !Main.RunsCallOfTheWild;
        }

        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Deliver_Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            if(Main.RunsCallOfTheWild)
            {
                return instructions;
            }

            MethodInfo UnitEngagementExtension_GetThreatHand = AccessTools.Method(typeof(UnitEngagementExtension), nameof(UnitEngagementExtension.GetThreatHand));
            MethodInfo Patch_GetWeaponSlot = AccessTools.Method(typeof(AbilityCustomMeleeAttack_Deliver_Patch), nameof(GetWeaponSlot));

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            foreach(CodeInstruction instruction in instructions)
            {
                if (!instruction.Calls(UnitEngagementExtension_GetThreatHand))
                {
                    newInstructions.Add(instruction);
                }
                else
                {
                    // Load the AbilityCustomMeleeAttack, then call GetWeaponSlot
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldloc_1, null));
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, Patch_GetWeaponSlot));
                }
            }

            return newInstructions;
        }

        static WeaponSlot GetWeaponSlot(UnitEntityData attacker, AbilityCustomMeleeAttack customAbility)
        {
            return !customAbility.IsVitalStrike ? attacker.GetThreatHand() : attacker.Body.PrimaryHand;
        }
    }
}
