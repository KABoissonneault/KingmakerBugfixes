using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch
{
    static class TextFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Text patches");

            FixItemNames();
            FixUnnamedEnemyAbilities();
            FixTormentor();
        }
        
        static void FixItemNames()
        {
            if (!StartPatch("Item Names")) return;

            // In the base game, AmethystEncrustedRing has the GarnetRing name, and GarnetEncrustedRing has the TopazRing name
            BlueprintItemConfigurator.From(ItemEquipmentRingRefs.AmethystEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.AmethystRing)
                .Configure();

            BlueprintItemConfigurator.From(ItemEquipmentRingRefs.GarnetEncrustedRing)
                .SetDisplayName(KMLocalizedStrings.GarnetRing)
                .Configure();

            // In the base game, Cold Iron Rapier +3 is called Cold Iron Rapier +1 (no string for +3)
            BlueprintItemConfigurator.From(ItemWeaponRefs.ColdIronRapierPlus3)
                .SetDisplayName(KMBFLocalizedStrings.CreateString("cold-iron-rapier-plus3"))
                .Configure();
        }
        
        // Enemy attacks Hero with . Miss!
        static void FixUnnamedEnemyAbilities()
        {
            if (!StartPatch("Empty Enemy Weapons")) return;

            BlueprintUnitFactConfigurator.From(AbilityRefs.MimicOozeSpit)
                .SetDisplayName(KMLocalizedStrings.Spit)
                .SetDescription(KMBFLocalizedStrings.CreateString("ooze-spit-description"))
                .Configure();

            BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.GiantSlugTongue)
                .SetTypeName(KMBFLocalizedStrings.Tongue)
                .SetDefaultName(KMBFLocalizedStrings.Tongue)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.ShockerLizardTouch)
                .SetTypeName(KMLocalizedStrings.TouchTypeName)
                .SetDefaultName(KMLocalizedStrings.TouchDefaultName)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.BuletteBite)
                .SetTypeName(KMLocalizedStrings.BiteTypeName)
                .SetDefaultName(KMLocalizedStrings.BiteDefaultName)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.MiteStoneThrow)
                .SetTypeName(KMLocalizedStrings.RockTypeName)
                .SetDefaultName(KMLocalizedStrings.RockDefaultName)
                .Configure();
        }

        // Tormentor shows Comforter as the debuff
        static void FixTormentor()
        {
            if (!StartPatch("Tormentor Buff")) return;

            BlueprintBuffConfigurator.From(BuffRefs.Tormentor)
                .SetDisplayName(KMLocalizedStrings.TormentorDisplayName)
                .SetDescription(KMLocalizedStrings.TormentorDescription)
                .Configure();
        }
    }
}
