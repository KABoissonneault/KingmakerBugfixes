using HarmonyLib;
using Kingmaker;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Globalmap.State;
using Kingmaker.Utility;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace kmbf.Patch.KM.Controllers.GlobalMap
{
    // RandomEncountersController.RollTravelEncounter has an issue: its chance of rolling *any* encounter can fall
    // to 0% with the right region upgrades.
    // This prevents getting some "special" random encounters, such as the Skeletal Merchant or quest encounters
    // This patch makes it so that if you roll above the "modified" encounter chance but below the unmodified encounter chance,
    // you can still roll a special (non-random) encounter.
    // This allows quests like Honor and Duty to always be completable, without necessarily guaranteeing it immediately
    [HarmonyPatch(typeof(RandomEncountersController), nameof(RandomEncountersController.RollTravelEncounter))]
    static class RandomEncountersController_RollTravelEncounter_Transpile
    {
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> Run(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var get_Settings = AccessTools.PropertyGetter(typeof(RandomEncountersController), nameof(RandomEncountersController.Settings));
            var rollSpecialEncounter = AccessTools.Method(typeof(RandomEncountersController_RollTravelEncounter_Transpile), nameof(RollSpecialEncounter));

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            Label label = generator.DefineLabel();
            bool shouldAttachLabel = false;
            bool foundFlag2 = false;

            CodeInstruction loadValue = instructions.Where(i => i.opcode == OpCodes.Ldloc_S).First(i => i.operand is LocalVariableInfo l && l.LocalIndex == 11);
            CodeInstruction loadNum7 = instructions.Where(i => i.opcode == OpCodes.Ldloc_S).First(i => i.operand is LocalVariableInfo l && l.LocalIndex == 13);
            foreach (var instr in instructions)
            {
                if(foundFlag2)
                {
                    if(instr.opcode != OpCodes.Brfalse_S)
                    {
                        newInstructions.Add(instr);
                        foundFlag2 = false;
                        continue;
                    }

                    newInstructions.Add(instr); // brfalse.s IL_02C2

                    // We want to:
                    // 1. Load num7 (encounter roll)
                    // 2. Compare it to Settings.ChanceOnGlobalMap (encounter rate unmodified by Region Claims / Upgrades)
                    // 3. If below, roll a special encounter with the instance and the position value (index 11)
                    // 4. Else, keep going as usual (stops the function)
                    newInstructions.Add(loadNum7);
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, get_Settings));
                    newInstructions.Add(CodeInstruction.LoadField(typeof(RandomEncountersRoot), nameof(RandomEncountersRoot.ChanceOnGlobalMap)));
                    newInstructions.Add(new CodeInstruction(OpCodes.Cgt));
                    newInstructions.Add(new CodeInstruction(OpCodes.Brtrue_S, label));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ldarg_0));
                    newInstructions.Add(loadValue);
                    newInstructions.Add(new CodeInstruction(OpCodes.Call, rollSpecialEncounter));
                    newInstructions.Add(new CodeInstruction(OpCodes.Ret));

                    foundFlag2 = false;
                    shouldAttachLabel = true;
                }
                else
                {
                    if (shouldAttachLabel)
                    {
                        instr.labels.Add(label);
                        shouldAttachLabel = false;
                    }

                    if (instr.opcode == OpCodes.Ldloc_S)
                    {
                        var localVar = instr.operand as LocalVariableInfo;
                        if (localVar.LocalIndex == 15)
                            foundFlag2 = true;
                    }

                    newInstructions.Add(instr);
                }
            }

            return newInstructions;
        }

        static bool RollSpecialEncounter(RandomEncountersController instance, Vector3 position)
        {
            BlueprintRandomEncounter randomEncounter = RandomEncounterSelector_SelectSpecialEncounter();
            if(randomEncounter != null)
            {
                return instance.StartEncounter(randomEncounter, 0, position, isHard: false, isCamp: false);
            }
            return false;
        }

        static BlueprintRandomEncounter RandomEncounterSelector_SelectSpecialEncounter()
        {
            GlobalMapState state = Game.Instance.Player.GlobalMap;
            Dictionary<EncounterPool, IGrouping<EncounterPool, BlueprintRandomEncounter>> source = (from e in BlueprintRoot.Instance.RE.RandomEncounters
                                                                                                    where CheckEncounter(e)
                                                                                                    group e by e.Pool).ToDictionary((IGrouping<EncounterPool, BlueprintRandomEncounter> i) => i.Key);
            List<BlueprintRandomEncounter> list = source.Get(EncounterPool.Special)?.ToList();
            List<BlueprintRandomEncounter> list2 = ((list != null && list.Count > 0) ? list : (source.SelectMany((KeyValuePair<EncounterPool, IGrouping<EncounterPool, BlueprintRandomEncounter>> i) => i.Value).ToList()));
            List<BlueprintRandomEncounter> list3 = ((state.LastEncounter != null) ? list2.Where((BlueprintRandomEncounter e) => e.Pool != state.LastEncounter.Pool).ToList() : null);
            List<BlueprintRandomEncounter> list4 = ((!list3.Empty()) ? list3 : list2);
            return list4.Random();
        }

        static bool CheckEncounter(BlueprintRandomEncounter e)
        {
            GlobalMapState globalMap = Game.Instance.Player.GlobalMap;
            if (e.CampingEncounter)
            {
                return false;
            }
            if (e.IsRandomizedCombat)
            {
                return false;
            }
            if (e.EncountersLimit > 0 && globalMap.GetEncountersCount(e) >= e.EncountersLimit)
            {
                return false;
            }
            if (!e.Conditions.Check())
            {
                return false;
            }
            return true;
        }
    }
}
