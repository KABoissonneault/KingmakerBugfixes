using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

namespace kmbf.Patch
{
    static class TextFixes
    {
        
        public static void FixItemNames()
        {
            // In the base game, AmethystEncrustedRing has the GarnetRing name, and GarnetEncrustedRing has the TopazRing name
            BlueprintItemConfigurator.From(BlueprintItemEquipmentRingGuid.AmethystEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.AmethystRing)
                .Configure();

            BlueprintItemConfigurator.From(BlueprintItemEquipmentRingGuid.GarnetEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.GarnetRing)
                .Configure();

            // In the base game, Cold Iron Rapier +3 is called Cold Iron Rapier +1 (no string for +3)
            BlueprintItemConfigurator.From(BlueprintItemWeaponGuid.ColdIronRapierPlus3)
                .SetDisplayName(KMBFLocalizedStrings.CreateString("cold-iron-rapier-plus3"))
                .Configure();
        }
        
        public static void FixUnnamedEnemyAbilities()
        {
            BlueprintUnitFactConfigurator.From(BlueprintAbilityGuid.MimicOozeSpit)
                .SetDisplayName(KMLocalizedStrings.Spit)
                .SetDescription(KMBFLocalizedStrings.CreateString("ooze-spit-description"))
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.GiantSlugTongue)
                .SetTypeName(KMBFLocalizedStrings.Tongue)
                .SetDefaultName(KMBFLocalizedStrings.Tongue)
                .Configure();
        }
    }
}
