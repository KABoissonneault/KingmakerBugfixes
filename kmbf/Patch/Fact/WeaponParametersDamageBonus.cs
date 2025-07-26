using HarmonyLib;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Abilities.Components;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch.Fact
{
    // Add null check in WeaponParametersDamageBonus.OnEventAboutToTrigger(RuleCalculateDamage) on the access to component.Weapon
    // This would trigger on ranged spells with an AbilityDeliverProjectile component  (ex: DragonsBreathSilver)
    [HarmonyPatch(typeof(WeaponParametersDamageBonus), nameof(WeaponParametersDamageBonus.OnEventAboutToTrigger), [typeof(RuleCalculateDamage)])]
    internal class WeaponParametersDamageBonus_OnEventAboutToTrigger_Transpile
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> OnEventAboutToTrigger_Transpile(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            FieldInfo weaponField = AccessTools.Field(typeof(AbilityDeliverProjectile), nameof(AbilityDeliverProjectile.Weapon));

            List<CodeInstruction> currentInstructions = instructions.ToList();
            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            for(int i = 0; i < currentInstructions.Count; ++i)
            {
                CodeInstruction currentInstruction = currentInstructions[i];
                if(currentInstruction.opcode == OpCodes.Ldfld && currentInstruction.operand.Equals(weaponField))
                {
                    int jumpOutIndex = currentInstructions.FindLastIndex(i, 
                        inst => inst.opcode == OpCodes.Br_S);
                    CodeInstruction jumpOutInstruction = currentInstructions[jumpOutIndex];

                    Label nextLabel = generator.DefineLabel();
                    CodeInstruction nextInstruction = currentInstructions[i + 1];
                    nextInstruction.labels.Add(nextLabel);

                    newInstructions.Add(currentInstruction);
                    newInstructions.Add(new CodeInstruction(OpCodes.Dup));
                    newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, nextLabel));
                    newInstructions.Add(new CodeInstruction(OpCodes.Pop));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldc_I4_1));
                    newInstructions.Add(new CodeInstruction(OpCodes.Br_S, jumpOutInstruction.operand));
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
