using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

namespace kmbf
{
    static class ItemFixes
    {
        // In the base game, AmethystEncrustedRing has the GarnetRing name, and GarnetEncrustedRing has the TopazRing name
        public static void FixLootRingNames()
        {
            BlueprintItemConfigurator.From(BlueprintItemEquipmentRingGuid.AmethystEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.AmethystRing)
                .Configure();

            BlueprintItemConfigurator.From(BlueprintItemEquipmentRingGuid.GarnetEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.GarnetRing)
                .Configure();
        }
    }
}
