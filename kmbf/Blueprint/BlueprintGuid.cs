using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom.Artisans;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.UnitLogic.Abilities.Blueprints;
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

    public bool TryFetchBP(out BlueprintScriptableObject bp)
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

    public static BlueprintComponentListGuid CapitalThroneRoomActions = new("d528c9a3bfea3ba4fb69ae5811c15499");
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

    public static readonly BlueprintAbilityGuid ProtectionFromArrowsCommunal = new("96c9d98b6a9a7c249b6c4572e4977157");
    public static readonly BlueprintAbilityGuid RaiseDead = new("a0fc99f0933d01643b2b8fe570caa4c5");
    public static readonly BlueprintAbilityGuid BreathOfLifeTouch = new("cbd03c874e39e6c4795fe0093544f2a2");
    public static readonly BlueprintAbilityGuid JoyfulRapture = new("15a04c40f84545949abeedef7279751a");

    public static readonly BlueprintAbilityGuid DarknessDomainGreaterAbility = new("31acd268039966940872c916782ae018");
    public static readonly BlueprintAbilityGuid DeadlyEarthMudBlast = new("0be97d0e752060f468bbf62ce032b9f5");
    public static readonly BlueprintAbilityGuid DeadlyEarthEarthBlast = new("e29cf5372f89c40489227edc9ffc52be");
    public static readonly BlueprintAbilityGuid DeadlyEarthMagmaBlast = new("c0704daaf6e4c5840a94e7db6d7dbe0e");
    public static readonly BlueprintAbilityGuid DeadlyEarthMetalBlast = new("44804ca6ba7d495439cc9d5ad6d6cfcf");

    public static readonly BlueprintAbilityGuid SummonMonsterVSingle = new("0964bf88b582bed41b74e79596c4f6d9");
    public static readonly BlueprintAbilityGuid SummonNaturesAllyVSingle = new("28ea1b2e0c4a9094da208b4c186f5e4f");

    public static readonly BlueprintAbilityGuid MimicOozeSpit = new("3ea0add618aab444bb5a4e2701a3ee4b");
}

public class BlueprintAbilityAreaEffectGuid : BlueprintObjectGuid
{
    public BlueprintAbilityAreaEffectGuid() { }
    public BlueprintAbilityAreaEffectGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Ability Area Effect";

    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMetalBlast = new BlueprintAbilityAreaEffectGuid("38a2979db34ad0f45a449e5eb174729f");
    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthRareMetalBlast = new BlueprintAbilityAreaEffectGuid("267f19ba174b21e4d9baf30afd589068");
    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMudBlast = new BlueprintAbilityAreaEffectGuid("0af604484b5fcbb41b328750797e3948");

    public static readonly BlueprintAbilityAreaEffectGuid BalefulGaze = new BlueprintAbilityAreaEffectGuid("1b6dc09a66357e14ab51b7db86e9a29d");
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

    public static readonly BlueprintCharacterClassGuid ArcaneTrickster = new BlueprintCharacterClassGuid("9c935a076d4fe4d4999fd48d853e3cf3");
    public static readonly BlueprintCharacterClassGuid Druid = new BlueprintCharacterClassGuid("610d836f3a3a9ed42a4349b62f002e96");
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

    public static readonly BlueprintCueGuid IrleneGiftCue3 = new("03807d3897f73e44b84b476ae63a62f1");

    public static readonly BlueprintCueGuid CandlemereRismelDelayedStartFight = new("f099ec070656d0f45981e29aeac9d190");

    public static readonly BlueprintCueGuid Act2KestenTourToThroneRoom_Cue01 = new("61fd0dbd69f5c354995a559f79888c6f");
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

    public static readonly BlueprintKingdomArtisanGuid Woodmaster = new BlueprintKingdomArtisanGuid("670c334ec3ecd1640b70024ea93d9229");
    public static readonly BlueprintKingdomArtisanGuid ShadyTrader = new BlueprintKingdomArtisanGuid("42efca2aecce9ff43ad3ed2d4d516124");
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

public class BlueprintWeaponEnchantmentGuid : BlueprintItemEnchantmentGuid
{
    public BlueprintWeaponEnchantmentGuid() { }
    public BlueprintWeaponEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Weapon Enchantment";
    public bool GetBlueprint(out BlueprintWeaponEnchantment bp) => GetBlueprintAs(out bp);

    public static readonly BlueprintWeaponEnchantmentGuid Soporiferous = new BlueprintWeaponEnchantmentGuid("da0a0c76266c96b45aacc34dc6635b28");
    public static readonly BlueprintWeaponEnchantmentGuid BaneLiving = new BlueprintWeaponEnchantmentGuid("e1d6f5e3cd3855b43a0cb42f6c747e1c");
    public static readonly BlueprintWeaponEnchantmentGuid NaturesWrath = new BlueprintWeaponEnchantmentGuid("afa5d47f05724ac43a4dc19e5ecbd150");
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

    public static readonly BlueprintBuffGuid ProtectionFromArrows = new("241ee6bd8c8767343994bce5dc1a95e0");
    public static readonly BlueprintBuffGuid ProtectionFromArrowsCommunal = new("10014a817b0780c49a2d2d954f62fa55");
    public static readonly BlueprintBuffGuid MagicalVestmentArmor = new("9e265139cf6c07c4fb8298cb8b646de9");
    public static readonly BlueprintBuffGuid MagicalVestmentShield = new("2e8446f820936a44f951b50d70a82b16");

    public static readonly BlueprintBuffGuid Nauseated = new("956331dba5125ef48afe41875a00ca0e");

    public static readonly BlueprintBuffGuid DebilitatingInjuryBewilderedActive = new("116ee72b2149f4d44a330296a7e42d13");
    public static readonly BlueprintBuffGuid DebilitatingInjuryBewilderedEffect = new("22b1d98502050cb4cbdb3679ac53115e");
    public static readonly BlueprintBuffGuid DebilitatingInjuryDisorientedActive = new("6339eac5bdcef1747ac46885d2cf4e25");
    public static readonly BlueprintBuffGuid DebilitatingInjuryDisorientedEffect = new("1f1e42f8c06d7dc4bb70cc12c73dfb38");
    public static readonly BlueprintBuffGuid DebilitatingInjuryHamperedActive = new("cc9a43f5157309646b23a0a690fee84b");
    public static readonly BlueprintBuffGuid DebilitatingInjuryHamperedEffect = new("5bfefc22a68e736488b0c309d9c1c1d4");

    public static readonly BlueprintBuffGuid ItsAMagicalPlace = new("670ab5958ff9ab246966ecb968132f37");

    public static readonly BlueprintBuffGuid CraneStyleWingBuff = new("f78a249bacba9924b9595e52495cb02f");

    // GUIDs taken from CotW. Should not run together
    public static readonly BlueprintBuffGuid KMBF_ShatterDefensesHit = new("843741b85d8249b9acdcffb042015f06");
    public static readonly BlueprintBuffGuid KMBF_ShatterDefensesAppliedThisRound = new("cf3e721e93044a21b87692526b3c45e3");
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

public class BlueprintItemWeaponGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemWeaponGuid() { }
    public BlueprintItemWeaponGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Weapon";
    public bool GetBlueprint(out BlueprintItemWeapon bp) => GetBlueprintAs(out bp);

    public static readonly BlueprintItemWeaponGuid SoporiferousSecond = new BlueprintItemWeaponGuid("af87d71820e93364c81b1aff840344ed");
    public static readonly BlueprintItemWeaponGuid ColdIronRapierPlus3 = new BlueprintItemWeaponGuid("925d399524ae74748ac78b49f48785c0");
}

public class BlueprintItemEquipmentRingGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemEquipmentRingGuid() { }
    public BlueprintItemEquipmentRingGuid(string guid)
        : base(guid)
    {

    }

    public override string BlueprintTypeName => "Item Equipment Ring";
    public bool GetBlueprint(out BlueprintItemEquipmentRing bp) => GetBlueprintAs(out bp);

    public static BlueprintItemEquipmentRingGuid AmethystEncrustedRing = new("3cfeae23012c8b9488b5b36d51ee4a8d");
    public static BlueprintItemEquipmentRingGuid GarnetEncrustedRing = new("489a7b81151c40541b8de88b0eaa6a77");
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

    public static readonly BlueprintItemEquipmentUsableGuid ScrollSummonNaturesAllyVSingle = new BlueprintItemEquipmentUsableGuid("4e9e261a93c7aa144a7b29c9fcfb4986");
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

    public static readonly BlueprintCheckGuid ShrewishGulchLastStageTwoActions = new BlueprintCheckGuid("373d384d88b55a244b74009dc6628b0e");
    public static readonly BlueprintCheckGuid ShrewishGulchLastStageThreeActions = new BlueprintCheckGuid("e4f4fe6042b99cc4790f0103ae10345e");

    public static readonly BlueprintCheckGuid Unrest_AngryMob_FirstCheck_Diplomacy = new("f0a74d43a46cff44e9cefa07710bb1e6");
    public static readonly BlueprintCheckGuid Unrest_AngryMob_FirstCheck_Intimidate = new("46b410623d176764fbd57bdc0e5b921d");
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

    public static readonly BlueprintWeaponTypeGuid Dart = new BlueprintWeaponTypeGuid("f415ae950523a7843a74d7780dd551af");

    public static readonly BlueprintWeaponTypeGuid GiantSlugTongue = new BlueprintWeaponTypeGuid("4957290cee0b59542808c65c77bfbee3");
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

    public static readonly BlueprintFeatureGuid DoubleDebilitation = new("dd699394df0ef8847abba26038333f02");

    public static readonly BlueprintFeatureGuid EkunWolfOffensiveMaster = new("64f74b75ed0d1f8478de5245cf061bcc");
    public static readonly BlueprintFeatureGuid EkunWolfDefensiveMaster = new("915616ab61446694dbd73c7d269ea184");
    public static readonly BlueprintFeatureGuid EkunWolfOffensiveBuff = new("29b33987fae4f81448410007f8f9b902");
    public static readonly BlueprintFeatureGuid EkunWolfDefensiveBuff = new("b6cb208baaff10542a8230f1b9f6b26d");

    public static readonly BlueprintFeatureGuid DwarvenChampionEnchant = new("09de6da5469ab1943924139d5145835f");
    public static readonly BlueprintFeatureGuid NecklaceOfDoubleCrosses = new("64d5a59feeb292e49a6c459fe37c3953");

    public static readonly BlueprintFeatureGuid ShatterDefenses = new("61a17ccbbb3d79445b0926347ec07577");

    public static readonly BlueprintFeatureGuid AnimalCompanionUpgradeLeopard = new("b8c98af302ee334499d30a926306327d");
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

    public static readonly BlueprintKingdomUpgradeGuid ItsAMagicalPlace = new BlueprintKingdomUpgradeGuid("f9e28dd6f77a0b5468b2325b91c4195c");
    public static readonly BlueprintKingdomUpgradeGuid ResearchOftheCandlemere = new BlueprintKingdomUpgradeGuid("767fc35b0d514cd45a5aaf2ea8681cbb");
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

    public static readonly BlueprintKingdomRootGuid KingdomRoot = new("cdba36795a8ae944091f4b9f94e6d689");
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

    public static readonly BlueprintSettlementBuildingGuid AssassinsGuild = new("449a534c27f58ad47b36a32517f99bb5");
    public static readonly BlueprintSettlementBuildingGuid Aviary = new("cb8281c84a460704989e099eccacc7ca");
    public static readonly BlueprintSettlementBuildingGuid BlackMarket = new("3ea99e66901503e479cf6b58ba46265d");
    public static readonly BlueprintSettlementBuildingGuid EmbassyRow = new("f7e58e7f377c9fa46a10d2eecedd8c7c");
    public static readonly BlueprintSettlementBuildingGuid GamblingDen = new("13e6ce05f2aca6843b0cda6dff79e73b");
    public static readonly BlueprintSettlementBuildingGuid School = new("442cd4e83d39fb245853d23bfd3c158c");
    public static readonly BlueprintSettlementBuildingGuid TempleOfAbadar = new("7ccdcde2587eeb449b0b6cb6e2bfbda6");
    public static readonly BlueprintSettlementBuildingGuid Theater = new("74a922aaafbd23f4cb6f0b571d398cfa");
    public static readonly BlueprintSettlementBuildingGuid ThievesGuild = new("73526a963bbe73748af8a37299907dea");
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

    public static readonly BlueprintKingdomBuffGuid CandlemereTowerResearch = new("b92f2b91fe37f4f4e8000842069cf4a3");
    public static readonly BlueprintKingdomBuffGuid ItsAMagicalPlace = new("540f7dfba39b96c46bb76680e46077da");
    public static readonly BlueprintKingdomBuffGuid CulRank5_DiscountCulBuildings = new ("60e1176b4240bda409ce6a2ba7037f8c");
    public static readonly BlueprintKingdomBuffGuid StaRank10_WigmoldSystem = new("133818e83c8fb724fa9a59b547a7e0c6");
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

    public static readonly BlueprintRegionGuid ShrikeHills = new("caacbcf9f6d6561459f526e584ded703");
    public static readonly BlueprintRegionGuid SouthNarlmarches = new("cd92c3a23b092584a95eb39f64225923");
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

    public static readonly BlueprintKingdomEventGuid HonorAndDuty = new("acd37baba5a0d4343b7b184780bd68cf");
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

    public static readonly BlueprintQuestObjectiveGuid HonorAndDutyProtectOrKickOut = new("d45987ff311b94d40940167aae5356e9");
    public static readonly BlueprintQuestObjectiveGuid HonorAndDutyWaitForPeopleReaction = new("7368326029429d14286d5447e2dde37b");
    public static readonly BlueprintQuestObjectiveGuid HonorAndDutyFail = new("4adc57ec7cf0ea24b99acf1095eeefd9");
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

    public static readonly BlueprintRandomEncounterGuid HonorAndDuty = new("d46dea989708a6f4c84e800fdb999449");
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

    public static readonly BlueprintUnlockableFlagGuid AngryMob_FirstCheckModifier = new("8258f133257832541bf2b7bd9f99ed05");

    public static readonly BlueprintUnlockableFlagGuid SouthNarlmarches_MagicalUpgrade = new("a069e9ffe15aa214c830c8ef57a7bee0");
    
    public static readonly BlueprintUnlockableFlagGuid EzvankiDeal = new("00b40c4c7e679cb47be2eeb6cd857311");
}