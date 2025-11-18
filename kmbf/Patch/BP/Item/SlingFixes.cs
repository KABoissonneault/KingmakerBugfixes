using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.FactLogic;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using UnityEngine;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch.BP.Item
{
    public class WeaponUndersized : WeaponEnchantmentLogic, IInitiatorRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IRulebookHandler<RuleCalculateAttackBonusWithoutTarget>, IInitiatorRulebookSubscriber
    {
        public void OnEventAboutToTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
            if (evt.Weapon == base.Owner && (evt.Initiator?.Descriptor?.OriginalSize ?? Size.Medium) > Size.Small)
            {
                evt.AddBonus(-2, base.Fact);
            }
        }

        public void OnEventDidTrigger(RuleCalculateAttackBonusWithoutTarget evt)
        {
        }
    }

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

            var rockThrow = BlueprintWeaponEnchantmentConfigurator.From(WeaponEnchantmentRefs.RockThrowStrength)
                .SetEnchantName(KMLocalizedStrings.RockThrow)
                .SetDescription(KMBFLocalizedStrings.CreateString("rock-throw-strength-description"))
                .Configure();

            #endregion

            if (!StartAddedContentPatch("Slings - Added Content", nameof(AddedContentSettings.SlingsExperimental)))
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
                .SetIcon(slingMoreMagicIcon)
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

            var slingCorrosivePlus1 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingCorrosivePlus1)
                .SetCR(7)
                .SetIcon(slingMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus2.m_VisualParameters);
                })
                .Configure();

            var slingCorrosivePlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingCorrosivePlus2)
                .SetCR(10)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var slingFlamingPlus1 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingFlamingPlus1)
                .SetCR(7)
                .SetIcon(slingMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus2.m_VisualParameters);
                })
                .Configure();

            var slingFlamingPlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingFlamingPlus2)
                .SetCR(10)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var slingFrostPlus1 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingFrostPlus1)
                .SetCR(7)
                .SetIcon(slingMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus2.m_VisualParameters);
                })
                .Configure();

            var slingFrostPlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingFrostPlus2)
                .SetCR(10)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var slingShockPlus1 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingShockPlus1)
                .SetCR(7)
                .SetIcon(slingMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus2.m_VisualParameters);
                })
                .Configure();

            var slingShockPlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingShockPlus2)
                .SetCR(10)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var slingSonicPlus2 = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SlingSonicPlus2)
                .SetCR(10)
                .SetIcon(slingMoreMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                .Configure();

            var undersized = BlueprintWeaponEnchantmentConfigurator.New(FixWeaponEnchantmentRefs.Undersized, "Undersized")
                .AddComponent<WeaponUndersized>()
                .SetEnchantName(KMBFLocalizedStrings.CreateString("undersized-name"))
                .SetDescription(KMBFLocalizedStrings.CreateString("undersized-description"))
                .Configure();

            var koboldSling = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.KoboldSling)
                .AddEnchantment(WeaponEnchantmentRefs.Enhancement1)
                .AddEnchantment(WeaponEnchantmentRefs.Speed)
                .AddEnchantment(FixWeaponEnchantmentRefs.Undersized)
                .SetCR(11)
                .SetDescription(KMBFLocalizedStrings.CreateString("kobold-sling-description"))
                .SetFlavorText(KMBFLocalizedStrings.CreateString("kobold-sling-flavor"))
                .SetIcon(slingVeryMagicIcon)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                })
                // "+3" cost
                // Enchantment: +1
                // Speed: +3
                // Small: -1
                // Same as Devourer of Metal
                .SetCost(18500)
                .Configure();            

            var trollSling = BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.LargeStandardSling)
                .SetSize(Size.Medium) // Counter-intuitive, but this is how KM does large weapons
                .SetOverrideDamageDice(DiceFormulaFactory.Value(DiceType.D6))
                .AddEnchantment(WeaponEnchantmentRefs.Enhancement4)
                .AddEnchantment(WeaponEnchantmentRefs.RockThrowStrength)
                .AddEnchantment(WeaponEnchantmentRefs.Oversized)
                .EditVisualParameters(v =>
                {
                    v.CopyFrom(slingStaffPlus3.m_VisualParameters);
                    v.m_Projectiles = [null];
                    ProjectileRefs.RockThrow.GetBlueprint(out v.m_Projectiles[0]);
                })
                .SetCR(16)
                .SetDisplayName(KMBFLocalizedStrings.CreateString("troll-sling-display-name"))
                .SetDescription(KMBFLocalizedStrings.CreateString("troll-sling-description"))
                .SetFlavorText(KMBFLocalizedStrings.CreateString("troll-sling-flavor"))
                .SetIcon(slingMostMagicIcon)
                .SetCost(50000)
                .Configure();

            #endregion

            #region Vendors
            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.Oleg)
                    .AddItem("KMBF1", standardSlingStaff)
                    .AddItem("KMBF2", standardSling, 10)
                    .AddItem("KMBF3", masterworkSling)
                    .Configure();

            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.DLC2QuartermasterBase)
                    .AddItem("KMBF1", standardSling, 10)
                    .AddItem("KMBF2", masterworkSling, 6)
                    .AddItem("KMBF3", slingPlus1, 6)
                    .Configure();

            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.DLC2QuartermasterImproved)
                    .AddItem("KMBF1", slingStaffPlus2, 2)
                    .AddItem("KMBF2", slingCorrosivePlus1, 2)
                    .AddItem("KMBF3", slingPlus2, 3)
                    .Configure();

            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.DireNarlmarchesVillageTrader)
                    .AddItem("KMBF1", slingPlus1, 1)
                    .AddItem("KMBF2", slingPlus2, 1)
                    .AddItem("KMBF3", slingPlus3, 1)
                    .AddItem("KMBF4", slingPlus4, 1)
                    .AddItem("KMBF5", slingPlus5, 1)
                    .AddItem("KMBF6", slingCorrosivePlus1, 1)
                    .AddItem("KMBF7", slingCorrosivePlus2, 1)
                    .AddItem("KMBF8", slingFlamingPlus1, 1)
                    .AddItem("KMBF9", slingFlamingPlus2, 1)
                    .AddItem("KMBF10", slingFrostPlus1, 1)
                    .AddItem("KMBF11", slingFrostPlus2, 1)
                    .AddItem("KMBF12", slingShockPlus1, 1)
                    .AddItem("KMBF13", slingShockPlus2, 1)
                    .AddItem("KMBF14", slingSonicPlus2, 1)
                    .AddItem("KMBF15", trollSling, 1)
                    .Configure();

            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.DLC3_HonestGuy)
                .AddItem("KMBF1", masterworkSling, 8)
                .AddItem("KMBF2", slingPlus1, 7)
                .AddItem("KMBF3", standardSling, 10)
                .Configure();

            BlueprintSharedVendorTableConfigurator.From(SharedVendorTableRefs.TrollLairVendor)
                .AddItem("KMBF1", masterworkSling, 3)
                .AddItem("KMBF2", koboldSling)
                .Configure();

            // TODO, more

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

            // Helmet is supposed to protect against rocks
            BlueprintFeatureConfigurator.From(FeatureRefs.TrailblazerHelmet)
                .AddComponent<DamageReductionAgainstRangedWeapons>(c =>
                {
                    c.Type = slingType;
                    c.ReductionTrue = 10;
                    c.ReductionFalse = 2;
                })
                .Configure();
            #endregion
        }
    }
}
