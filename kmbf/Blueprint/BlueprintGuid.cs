//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom.Artisans;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace kmbf.Blueprint;

public class BlueprintObjectGuid
{
    public string guid;

    protected BlueprintScriptableObject cachedObject;
    protected bool cached = false;

    public BlueprintObjectGuid()
    {

    }

    public BlueprintObjectGuid(string guid)
    {
        this.guid = guid;
    }

    public virtual string BlueprintTypeName => "";

    public string GetDebugName()
    {
        var TypeName = BlueprintTypeName;
        if (!string.IsNullOrEmpty(TypeName))
        {
            if (cachedObject != null)
                return $"{TypeName}:{guid}:{cachedObject.name}";
            else
                return $"{TypeName}:{guid}";
        }
        else
        {
            if (cachedObject != null)
                return $"{guid}:{cachedObject.name}";
            else
                return $"{guid}";
        }
    }

    public bool GetBlueprint(out BlueprintScriptableObject bp)
    {
        if (!TryFetchBP(out bp))
        {
            if (!string.IsNullOrEmpty(BlueprintTypeName))
                Main.Log.Error($"Could not find {BlueprintTypeName} blueprint with GUID '{guid}'");
            else
                Main.Log.Error($"Could not find blueprint with GUID '{guid}'");
            return false;
        }

        return true;
    }

    protected bool GetBlueprintAs<C>(out C derivedBp)
        where C : BlueprintScriptableObject
    {
        if(GetBlueprint(out BlueprintScriptableObject bp))
        {
            if(bp is C c)
            {
                derivedBp = c;
                return true;
            }
            else
            {
                Main.Log.Error($"Blueprint with GUID '{guid}' did not have expected type {BlueprintTypeName}");
            }
        }

        derivedBp = null;
        return false;
    }

    protected bool TryFetchBP(out BlueprintScriptableObject bp)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid);
            cached = true;
        }

        bp = cachedObject;
        return bp != null;
    }

    public void AssignNewInstance(BlueprintScriptableObject bp)
    {
        cachedObject = bp;
        cached = true;
    }

    public static bool GetBlueprintArray<GuidType, BPType>(IEnumerable<GuidType> guids, out BPType[] bps)
        where GuidType : BlueprintObjectGuid
        where BPType : BlueprintScriptableObject
    {
        List<BPType> result = new List<BPType>();
        foreach (var guid in guids)
        {
            if (!guid.GetBlueprintAs(out BPType value))
            {
                bps = null;
                return false;
            }

            result.Add(value);
        }

        bps = result.ToArray();
        return true;
    }
}

public class BlueprintComponentListGuid : BlueprintObjectGuid
{
    public BlueprintComponentListGuid() { }
    public BlueprintComponentListGuid(string guid) 
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Component List";
    public bool GetBlueprint(out BlueprintComponentList componentList) => GetBlueprintAs(out componentList);
}

public class BlueprintFactGuid : BlueprintObjectGuid
{
    public BlueprintFactGuid() { }
    public BlueprintFactGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Fact";
    public bool GetBlueprint(out BlueprintFact componentList) => GetBlueprintAs(out componentList);
}

public class BlueprintUnitFactGuid : BlueprintFactGuid
{
    public BlueprintUnitFactGuid() { }
    public BlueprintUnitFactGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Unit Fact";
    public bool GetBlueprint(out BlueprintUnitFact bp) => GetBlueprintAs(out bp);
}

public class BlueprintAbilityGuid : BlueprintUnitFactGuid
{
    public BlueprintAbilityGuid() { }
    public BlueprintAbilityGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Ability";
    public bool GetBlueprint(out BlueprintAbility bp) => GetBlueprintAs(out bp);
}

public class BlueprintActivatableAbilityGuid : BlueprintUnitFactGuid
{
    public BlueprintActivatableAbilityGuid() { }
    public BlueprintActivatableAbilityGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Activatable Ability";
    public bool GetBlueprint(out BlueprintActivatableAbility bp) => GetBlueprintAs(out bp);
}

    public class BlueprintAbilityAreaEffectGuid : BlueprintObjectGuid
{
    public BlueprintAbilityAreaEffectGuid() { }
    public BlueprintAbilityAreaEffectGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Ability Area Effect";
}

public class BlueprintCharacterClassGuid : BlueprintObjectGuid
{
    public BlueprintCharacterClassGuid() { }
    public BlueprintCharacterClassGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Character Class";
    public bool GetBlueprint(out BlueprintCharacterClass bp) => GetBlueprintAs(out bp);
}

public class BlueprintCueGuid : BlueprintObjectGuid
{
    public BlueprintCueGuid() { }
    public BlueprintCueGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Cue";
    public bool GetBlueprint(out BlueprintCue bp) => GetBlueprintAs(out bp);
}

public class BlueprintCheckGuid : BlueprintObjectGuid
{
    public BlueprintCheckGuid() { }
    public BlueprintCheckGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Check";
    public bool GetBlueprint(out BlueprintCheck bp) => GetBlueprintAs(out bp);
}

public class BlueprintAnswerGuid : BlueprintObjectGuid
{
    public BlueprintAnswerGuid() { }
    public BlueprintAnswerGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Answer";
    public bool GetBlueprint(out BlueprintAnswer bp) => GetBlueprintAs(out bp);
}

public class BlueprintKingdomArtisanGuid : BlueprintObjectGuid
{
    public BlueprintKingdomArtisanGuid() { }
    public BlueprintKingdomArtisanGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Kingdom Artisan";
    public bool GetBlueprint(out BlueprintKingdomArtisan bp) => GetBlueprintAs(out bp);
}

public class BlueprintItemEnchantmentGuid : BlueprintFactGuid
{
    public BlueprintItemEnchantmentGuid() { }
    public BlueprintItemEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Enchantment";
}

public class BlueprintEquipmentEnchantmentGuid : BlueprintItemEnchantmentGuid
{
    public BlueprintEquipmentEnchantmentGuid() { }
    public BlueprintEquipmentEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Equipment Enchantment";
    public bool GetBlueprint(out BlueprintEquipmentEnchantment bp) => GetBlueprintAs(out bp);
}

public class BlueprintWeaponEnchantmentGuid : BlueprintItemEnchantmentGuid
{
    public BlueprintWeaponEnchantmentGuid() { }
    public BlueprintWeaponEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Weapon Enchantment";
    public bool GetBlueprint(out BlueprintWeaponEnchantment bp) => GetBlueprintAs(out bp);
}

public class BlueprintBuffGuid : BlueprintUnitFactGuid
{
    public BlueprintBuffGuid() { }
    public BlueprintBuffGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Buff";
    public bool GetBlueprint(out BlueprintBuff bp) => GetBlueprintAs(out bp);
}

public class BlueprintItemGuid : BlueprintObjectGuid
{
    public BlueprintItemGuid() { }
    public BlueprintItemGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item";
}

public class BlueprintItemEquipmentGuid : BlueprintItemGuid
{
    public BlueprintItemEquipmentGuid() { }
    public BlueprintItemEquipmentGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment";
}

public class BlueprintItemEquipmentSimpleGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemEquipmentSimpleGuid() { }
    public BlueprintItemEquipmentSimpleGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment Simple";
}

public class BlueprintItemEquipmentHandGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemEquipmentHandGuid() { }
    public BlueprintItemEquipmentHandGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment Hand";
    public bool GetBlueprint(out BlueprintItemEquipmentHand bp) => GetBlueprintAs(out bp);
}

public class BlueprintItemWeaponGuid : BlueprintItemEquipmentHandGuid
{
    public BlueprintItemWeaponGuid() { }
    public BlueprintItemWeaponGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Weapon";
    public bool GetBlueprint(out BlueprintItemWeapon bp) => GetBlueprintAs(out bp);
}

public class BlueprintItemEquipmentRingGuid : BlueprintItemEquipmentSimpleGuid
{
    public BlueprintItemEquipmentRingGuid() { }
    public BlueprintItemEquipmentRingGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment Ring";
    public bool GetBlueprint(out BlueprintItemEquipmentRing bp) => GetBlueprintAs(out bp);
}

public class BlueprintItemEquipmentUsableGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemEquipmentUsableGuid() { }
    public BlueprintItemEquipmentUsableGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment Usable";
    public bool GetBlueprint(out BlueprintItemEquipmentUsable bp) => GetBlueprintAs(out bp);
}

public class BlueprintWeaponTypeGuid : BlueprintObjectGuid
{
    public BlueprintWeaponTypeGuid() { }
    public BlueprintWeaponTypeGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Weapon Type";
    public bool GetBlueprint(out BlueprintWeaponType bp) => GetBlueprintAs(out bp);
}

public class BlueprintFeatureGuid : BlueprintUnitFactGuid
{
    public BlueprintFeatureGuid() { }
    public BlueprintFeatureGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Feature";
    public bool GetBlueprint(out BlueprintFeature bp) => GetBlueprintAs(out bp);
}

public class BlueprintRaceGuid : BlueprintFeatureGuid
{
    public BlueprintRaceGuid() { }
    public BlueprintRaceGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Race";
    public bool GetBlueprint(out BlueprintRace bp) => GetBlueprintAs(out bp);
}

public class BlueprintFeatureSelectionGuid : BlueprintFeatureGuid
{
    public BlueprintFeatureSelectionGuid() { }
    public BlueprintFeatureSelectionGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Feature Selection";
    public bool GetBlueprint(out BlueprintFeatureSelection bp) => GetBlueprintAs(out bp);
}

public class BlueprintProgressionGuid : BlueprintFeatureGuid
{
    public BlueprintProgressionGuid() { }
    public BlueprintProgressionGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Progression";
    public bool GetBlueprint(out BlueprintProgression bp) => GetBlueprintAs(out bp);

}

public class BlueprintKingdomUpgradeGuid : BlueprintObjectGuid
{
    public BlueprintKingdomUpgradeGuid() { }
    public BlueprintKingdomUpgradeGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Kingdom Upgrade";
    public bool GetBlueprint(out BlueprintKingdomUpgrade bp) => GetBlueprintAs(out bp);
}

public class BlueprintKingdomRootGuid : BlueprintObjectGuid
{
    public BlueprintKingdomRootGuid() { }
    public BlueprintKingdomRootGuid(string guid)
        : base(guid)
    {

    }
    
    public override string BlueprintTypeName => "Kingdom Root";
    public bool GetBlueprint(out KingdomRoot bp) => GetBlueprintAs(out bp);
}

public class BlueprintSettlementBuildingGuid : BlueprintFactGuid
{
    public BlueprintSettlementBuildingGuid() { }
    public BlueprintSettlementBuildingGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Settlement Building";
    public bool GetBlueprint(out BlueprintSettlementBuilding bp) => GetBlueprintAs(out bp);
}

public class BlueprintKingdomBuffGuid : BlueprintFactGuid
{
    public BlueprintKingdomBuffGuid() { }
    public BlueprintKingdomBuffGuid(string guid) 
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Kingdom Buff";
    public bool GetBlueprint(out BlueprintKingdomBuff bp) => GetBlueprintAs(out bp);
}

public class BlueprintRegionGuid : BlueprintObjectGuid
{
    public BlueprintRegionGuid() { }
    public BlueprintRegionGuid(string guid) 
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Region";
    public bool GetBlueprint(out BlueprintRegion bp) => GetBlueprintAs(out bp);
}

public class BlueprintKingdomEventGuid : BlueprintObjectGuid
{    
    public BlueprintKingdomEventGuid() { }
    public BlueprintKingdomEventGuid(string guid) 
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Kingdom Event";
    public bool GetBlueprint(out BlueprintKingdomEvent bp) => GetBlueprintAs(out bp);
}

public class BlueprintQuestObjectiveGuid : BlueprintFactGuid
{
    public BlueprintQuestObjectiveGuid() { }
    public BlueprintQuestObjectiveGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Quest Objective";
    public bool GetBlueprint(out BlueprintQuestObjective bp) => GetBlueprintAs(out bp);
}

public class BlueprintRandomEncounterGuid : BlueprintObjectGuid
{
    public BlueprintRandomEncounterGuid() { }
    public BlueprintRandomEncounterGuid(string guid) 
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Random Encounter";
    public bool GetBlueprint(out BlueprintRandomEncounter bp) => GetBlueprintAs(out bp);
}

public class BlueprintUnlockableFlagGuid : BlueprintObjectGuid
{
    public BlueprintUnlockableFlagGuid() { }
    public BlueprintUnlockableFlagGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Unlockable Flag";
    public bool GetBlueprint(out BlueprintUnlockableFlag bp) => GetBlueprintAs(out bp);
}

public class BlueprintCategoryDefaultsGuid : BlueprintObjectGuid
{
    public BlueprintCategoryDefaultsGuid() { }
    public BlueprintCategoryDefaultsGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Category Defaults";
    public bool GetBlueprint(out BlueprintCategoryDefaults bp) => GetBlueprintAs(out bp);
}

public class BlueprintTrashLootSettingsGuid : BlueprintObjectGuid
{
    public BlueprintTrashLootSettingsGuid() { }
    public BlueprintTrashLootSettingsGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Trash Loot Settings";
    public bool GetBlueprint(out TrashLootSettings bp) => GetBlueprintAs(out bp);
}

public class BlueprintUnitLootGuid : BlueprintObjectGuid
{
    public BlueprintUnitLootGuid() { }
    public BlueprintUnitLootGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Unit Loot";
    public bool GetBlueprint(out BlueprintUnitLoot bp) => GetBlueprintAs(out bp);
}

public class BlueprintSharedVendorTableGuid : BlueprintUnitLootGuid
{
    public BlueprintSharedVendorTableGuid() { }
    public BlueprintSharedVendorTableGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Shared Vendor Table";
    public bool GetBlueprint(out BlueprintSharedVendorTable bp) => GetBlueprintAs(out bp);
}
