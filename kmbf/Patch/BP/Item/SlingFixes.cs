using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.FactLogic;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch.BP.Item
{
    static class SlingFixes
    {
        // The fields that matter to slings
        private static void CopyFrom(this WeaponVisualParameters lhs, WeaponVisualParameters rhs)
        {
            lhs.m_Projectiles = (BlueprintProjectile[])rhs.m_Projectiles?.Clone();
            lhs.m_WeaponAnimationStyle = rhs.m_WeaponAnimationStyle;
            lhs.m_WeaponModel = rhs.m_WeaponModel;
            lhs.m_WeaponBeltModel = rhs.m_WeaponBeltModel;
            lhs.m_SoundSize = rhs.m_SoundSize;
            lhs.m_SoundType = rhs.m_SoundType;
            lhs.m_WhooshSound = rhs.m_WhooshSound;
            lhs.m_MissSoundType = rhs.m_MissSoundType;
            lhs.m_EquipSound = rhs.m_EquipSound;
            lhs.m_UnequipSound = rhs.m_UnequipSound;
            lhs.m_InventoryEquipSound = rhs.m_InventoryEquipSound;
            lhs.m_InventoryPutSound = rhs.m_InventoryPutSound;
            lhs.m_InventoryTakeSound = rhs.m_InventoryTakeSound;
        }

        public static void Apply()
        {
            if (!StartPatch("Slings")) return;

            if (!WeaponTypeRefs.SlingStaff.GetBlueprint(out BlueprintWeaponType slingStaffType))
                return;

            if (!ItemWeaponRefs.StandardSlingStaff.GetBlueprint(out BlueprintItemWeapon standardSlingStaff))
                return;

            if (!ItemWeaponRefs.SlingStaffPlus2.GetBlueprint(out BlueprintItemWeapon slingStaffPlus2))
                return;

            if (!ItemWeaponRefs.SlingStaffPlus3.GetBlueprint(out BlueprintItemWeapon slingStaffPlus3))
                return;

            if (!ItemWeaponRefs.SlingStaffPlus4.GetBlueprint(out BlueprintItemWeapon slingStaffPlus4))
                return;

            if (!ItemWeaponRefs.SlingStaffPlus5.GetBlueprint(out BlueprintItemWeapon slingStaffPlus5))
                return;

            #region Base Fixes
            // We don't have a model for Slings, so use all the visuals of the Sling Staff
            var slingType = BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Sling)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffType.m_VisualParameters);                    
                })
                .Configure();

            var standardSling = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.StandardSling)
                .AddTrashCategory(TrashLootType.Equipment)
                .Configure();

            BlueprintCategoryDefaultsConfigurator.From(CategoryDefaultsRefs.DefaultsForWeaponCategories)
                .AddEntry(new BlueprintCategoryDefaults.CategoryDefaultEntry
                {
                    Key = WeaponCategory.Sling,
                    DefaultWeapon = standardSling
                })
                .Configure();

            #endregion

            if (!StartAddedContentPatch("Slings - Added Content", nameof(AddedContentSettings.Slings)))
            {
                #region Dummy Feats for save compat
                BlueprintFeatureConfigurator.New(FixFeatureRefs.HalflingWeaponFamiliarity, "HalflingWeaponFamiliarity")
                    .AddGroup(FeatureGroup.Racial)
                    .SetRanks(1)
                    .SetIsClassFeature(true) // Apparently Racials can be lost in respec, and that's how you signal that
                    .SetHideInUI(true)
                    .Configure();
                #endregion
                return;
            }

            #region Fixed and New Slings

            var slingMasterworkIcon = BundleManager.GetKmbfResource<Sprite>("SlingMasterwork");
            var slingMagicIcon = BundleManager.GetKmbfResource<Sprite>("SlingMagic");
            var slingMoreMagicIcon = BundleManager.GetKmbfResource<Sprite>("SlingMoreMagic");
            var slingVeryMagicIcon = BundleManager.GetKmbfResource<Sprite>("SlingVeryMagic");
            var slingMostMagicIcon = BundleManager.GetKmbfResource<Sprite>("SlingMostMagic");

            var masterworkSling = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.MasterworkSling)
                    .AddTrashCategory(TrashLootType.Equipment)
                    .SetCR(1)
                    .SetIcon(slingMasterworkIcon)
                    .Configure();

            var slingPlus1 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingPlus1)
                .SetCR(4)
                .SetIcon(slingMasterworkIcon)
                .Configure();

            var slingPlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingPlus2)
                .SetCR(7)
                .SetIcon(slingMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus2.m_VisualParameters);
                })
                .Configure();

            var slingPlus3 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingPlus3)
                .SetCR(10)
                .SetIcon(BundleManager.MakeAssetId("SlingMoreMagic"))
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var slingPlus4 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingPlus4)
                .SetCR(13)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus4.m_VisualParameters);
                })
                .Configure();

            var slingPlus5 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingPlus5)
                .SetCR(16)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus5.m_VisualParameters);
                })
                .Configure();
            #endregion

            #region Vendors
            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.Oleg)
                    .AddItem("KMBF1", standardSlingStaff)
                    .AddItem("KMBF2", standardSling, 10)
                    .AddItem("KMBF3", masterworkSling)
                    .Configure();

            // TODO

            #endregion

            #region Feats
            var halflingWeaponFamiliarity = BlueprintFeatureConfigurator.New(FixFeatureRefs.HalflingWeaponFamiliarity, "HalflingWeaponFamiliarity")
                    .AddGroup(FeatureGroup.Racial)
                    .SetRanks(1)
                    .SetIsClassFeature(true) // Apparently Racials can be lost in respec, and that's how you signal that
                    .SetDisplayName(KMBFLocalizedStrings.CreateString("halfling-weapon-familiarity"))
                    .SetDescription(KMBFLocalizedStrings.CreateString("halfling-weapon-familiarity-description"))
                    .SetIcon(slingType.Icon)
                    .AddComponent<AddProficiencies>(p =>
                    {
                        p.ArmorProficiencies = [];
                        p.WeaponProficiencies = [WeaponCategory.Sling];
                    })
                    .Configure();
            #endregion

            #region Race and Class
            BlueprintRaceConfigurator.From(RaceRefs.Halfling)
                    .AddFeature(halflingWeaponFamiliarity)
                    .AddComponent<AddStartingEquipment>(s =>
                    {
                        s.CategoryItems = [WeaponCategory.Sling];
                    })
                    .Configure();

            BlueprintCharacterClassConfigurator.From(CharacterClassRefs.Druid)
                .AddStartingItem(standardSling)
                .Configure();
            #endregion

            #region Misc fixes
            // Pretty sure only the DLC3 trash loot settings are used, but might as well
            BlueprintTrashLootSettingsConfigurator.From(TrashLootSettingsRefs.TrashLootSettings)
                    .AddTypeEntry(TrashLootType.Equipment, standardSling, masterworkSling)
                    .Configure();

            BlueprintTrashLootSettingsConfigurator.From(TrashLootSettingsRefs.DLC3TrashLootSettings)
                .AddTypeEntry(TrashLootType.Equipment, standardSling, masterworkSling)
                .Configure();
            #endregion


        }

        static void FixSlings()
        {
        }
    }
}
