using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Tasks;
using kmbf.Blueprint;

namespace kmbf.Patch
{
    static class SaveFixes
    {
        public static void Apply()
        {
            // Kingdom Upgrade buffs are permanent and not retroactive. They need to be added or removed here when fixing them

            if (Main.UMMSettings.BalanceSettings.FixCandlemereTowerResearch)
            {
                List<KingdomBuff> buffsToRemove = KingdomState.Instance.ActiveBuffs.Enumerable.Where(b =>
                {
                    return b.Blueprint.AssetGuid == BlueprintKingdomBuffGuid.CandlemereTowerResearch.guid
                    && !b.Region.Blueprint.Is(BlueprintRegionGuid.SouthNarlmarches);
                }).ToList();
                buffsToRemove.ForEach(KingdomState.Instance.ActiveBuffs.RemoveFact);
            }

            var kingdomShrikeHills = KingdomState.Instance.Regions.FirstOrDefault(r => r.Blueprint.Is(BlueprintRegionGuid.ShrikeHills));
            if (kingdomShrikeHills != null)
            {
                if(kingdomShrikeHills.IsUpgraded && kingdomShrikeHills.Upgrade.Project.Is(BlueprintKingdomUpgradeGuid.ItsAMagicalPlace))
                {
                    if(!KingdomState.Instance.ActiveBuffs.Enumerable.Any(b => b.Blueprint.Is(BlueprintKingdomBuffGuid.ItsAMagicalPlace)))
                    {
                        if (BlueprintKingdomBuffGuid.ItsAMagicalPlace.GetBlueprint(out BlueprintKingdomBuff bp))
                        {
                            KingdomState.Instance.ActiveBuffs.Add(bp, kingdomShrikeHills, null, 0);
                        }
                    }
                }
            }

            if (KingdomState.Instance != null)
            {
                if(KingdomState.Instance.CurrentRegion != null)
                {
                    // Adjacency bonus changes require Building/Upgrading/Selling. Add the ability to refresh here
                    KingdomState.Instance.CurrentRegion.Settlement?.Update();
                }
            }
        }
    }
}
