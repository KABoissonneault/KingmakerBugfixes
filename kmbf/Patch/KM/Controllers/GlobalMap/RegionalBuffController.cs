using HarmonyLib;
using Kingmaker;
using Kingmaker.Controllers.GlobalMap;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Buffs;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.UnitLogic.Buffs;

namespace kmbf.Patch.KM.Controllers.GlobalMap
{
    // RegionalBuffController only handles one RegionBasedPartyBuff per KingdomBuff
    // ErastilHolyPlaceKingdomBuff has two, so the +5 Lore(Nature) bonus gets missed
    [HarmonyPatch]
    static class RegionalBuffController_MultiRegionBuffs_Prefix
    {
        [HarmonyPatch(typeof(RegionalBuffController), nameof(RegionalBuffController.CheckBuffs))]
        [HarmonyPrefix]
        public static bool CheckBuffs(RegionId region)
        {
            if (KingdomState.Instance == null)
            {
                return false;
            }

            foreach (KingdomBuff item in KingdomState.Instance.ActiveBuffs.Enumerable)
            {
                if (!item.Active)
                {
                    continue;
                }

                foreach (var regionBasedPartyBuff in item.SelectComponents<RegionBasedPartyBuff>())
                {
                    bool flag = regionBasedPartyBuff.MatchedRegion(region);
                    foreach (UnitEntityData allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
                    {
                        Buff buff = allCrossSceneUnit.Buffs.GetBuff(regionBasedPartyBuff.Buff);
                        if (buff == null && flag)
                        {
                            allCrossSceneUnit.Buffs.AddBuff(regionBasedPartyBuff.Buff, Game.Instance.Player.MainCharacter.Value, null);
                        }
                        else if (buff != null && !flag)
                        {
                            allCrossSceneUnit.Buffs.RemoveFact(buff);
                        }
                    }
                }                
            }

            return false;
        }

        [HarmonyPatch(typeof(RegionalBuffController), nameof(RegionalBuffController.OnBuffAdded))]
        [HarmonyPrefix]
        static bool OnBuffAdded(KingdomBuff buff)
        {
            RegionId currentRegion = Game.Instance.Player.GlobalMap.CurrentRegion;
            
            foreach (var regionBasedPartyBuff in buff.SelectComponents<RegionBasedPartyBuff>())
            {
                bool flag = regionBasedPartyBuff.MatchedRegion(currentRegion);
                foreach (UnitEntityData allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
                {
                    Buff buff2 = allCrossSceneUnit.Buffs.GetBuff(regionBasedPartyBuff.Buff);
                    if (buff2 == null && flag)
                    {
                        allCrossSceneUnit.Buffs.AddBuff(regionBasedPartyBuff.Buff, Game.Instance.Player.MainCharacter.Value, null);
                    }
                    else if (buff2 != null && !flag)
                    {
                        allCrossSceneUnit.Buffs.RemoveFact(buff2);
                    }
                }
            }

            return false;
        }

        [HarmonyPatch(typeof(RegionalBuffController), nameof(RegionalBuffController.OnBuffRemoved))]
        [HarmonyPrefix]
        static bool OnBuffRemoved(KingdomBuff buff)
        {
            foreach (var regionBasedPartyBuff in buff.SelectComponents<RegionBasedPartyBuff>())
            {
                foreach (UnitEntityData allCrossSceneUnit in Game.Instance.Player.AllCrossSceneUnits)
                {
                    Buff buff2 = allCrossSceneUnit.Buffs.GetBuff(regionBasedPartyBuff.Buff);
                    if (buff2 != null)
                    {
                        allCrossSceneUnit.Buffs.RemoveFact(buff2);
                    }
                }
            }

            return false;
        }
    }
}
