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
            if (Main.UMMSettings.BalanceSettings.FixCandlemereTowerResearch)
            {
                List<KingdomBuff> buffsToRemove = KingdomState.Instance.ActiveBuffs.Enumerable.Where(b =>
                {
                    return b.Blueprint.AssetGuid == BlueprintKingdomBuffGuid.CandlemereTowerResearch.guid
                    && b.Region.Blueprint.AssetGuid != BlueprintKingdomRegionGuid.SouthNarlmarches.guid;
                }).ToList();
                buffsToRemove.ForEach(KingdomState.Instance.ActiveBuffs.RemoveFact);
            }
        }
    }
}
