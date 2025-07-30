using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.DialogSystem.Blueprints;
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

    public BlueprintObjectGuid(string guid)
    {
        this.guid = guid;
    }

    public bool GetBlueprint(out BlueprintScriptableObject bp)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        bp = cachedObject;
        return bp != null;
    }
}

public class BlueprintFactGuid : BlueprintObjectGuid
{
    public BlueprintFactGuid(string guid)
        : base(guid)
    {

    }
}

public class BlueprintUnitFactGuid : BlueprintFactGuid
{
    public BlueprintUnitFactGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintUnitFact fact)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintUnitFact>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Unit Fact blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        fact = cachedObject as BlueprintUnitFact;

        return fact != null;
    }
}

public class BlueprintAbilityGuid : BlueprintUnitFactGuid
{
    public BlueprintAbilityGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintAbility ability)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Ability blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        ability = cachedObject as BlueprintAbility;

        return ability != null;
    }

    public static readonly BlueprintAbilityGuid RaiseDead = new("a0fc99f0933d01643b2b8fe570caa4c5");
    public static readonly BlueprintAbilityGuid BreathOfLifeTouch = new("cbd03c874e39e6c4795fe0093544f2a2");

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
    BlueprintAbilityAreaEffectGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintAbilityAreaEffect abilityArea)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintAbilityAreaEffect>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Ability Area Effect blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        abilityArea = cachedObject as BlueprintAbilityAreaEffect;

        return abilityArea != null;
    }

    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMetalBlast = new BlueprintAbilityAreaEffectGuid("38a2979db34ad0f45a449e5eb174729f");
    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthRareMetalBlast = new BlueprintAbilityAreaEffectGuid("267f19ba174b21e4d9baf30afd589068");
    public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMudBlast = new BlueprintAbilityAreaEffectGuid("0af604484b5fcbb41b328750797e3948");
}

public class BlueprintCharacterClassGuid : BlueprintObjectGuid
{
    public BlueprintCharacterClassGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintCharacterClass characterClass)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Character Class blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        characterClass = cachedObject as BlueprintCharacterClass;

        return characterClass != null;
    }

    public static readonly BlueprintCharacterClassGuid Druid = new BlueprintCharacterClassGuid("610d836f3a3a9ed42a4349b62f002e96");
}

public class BlueprintCueGuid : BlueprintObjectGuid
{
    public BlueprintCueGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintCue cue)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintCue>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Cue blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        cue = cachedObject as BlueprintCue;

        return cue != null;
    }

    public static readonly BlueprintCueGuid IrleneGiftCue3 = new BlueprintCueGuid("03807d3897f73e44b84b476ae63a62f1");
}

public class BlueprintKingdomArtisanGuid : BlueprintObjectGuid
{
    public BlueprintKingdomArtisanGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintKingdomArtisan artisan)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintKingdomArtisan>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Kingdom Artisan blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        artisan = cachedObject as BlueprintKingdomArtisan;

        return artisan != null;
    }

    public static readonly BlueprintKingdomArtisanGuid Woodmaster = new BlueprintKingdomArtisanGuid("670c334ec3ecd1640b70024ea93d9229");
    public static readonly BlueprintKingdomArtisanGuid ShadyTrader = new BlueprintKingdomArtisanGuid("42efca2aecce9ff43ad3ed2d4d516124");
}

public class BlueprintItemEnchantmentGuid : BlueprintFactGuid
{
    public BlueprintItemEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintItemEnchantment itemEnchantment)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintItemEnchantment>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Item Enchantment blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        itemEnchantment = cachedObject as BlueprintItemEnchantment;

        return itemEnchantment != null;
    }
}

public class BlueprintWeaponEnchantmentGuid : BlueprintItemEnchantmentGuid
{
    public BlueprintWeaponEnchantmentGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintWeaponEnchantment weaponEnchantment)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Weapon Enchantment blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        weaponEnchantment = cachedObject as BlueprintWeaponEnchantment;

        return weaponEnchantment != null;
    }

    public static readonly BlueprintWeaponEnchantmentGuid Soporiferous = new BlueprintWeaponEnchantmentGuid("da0a0c76266c96b45aacc34dc6635b28");
    public static readonly BlueprintWeaponEnchantmentGuid BaneLiving = new BlueprintWeaponEnchantmentGuid("e1d6f5e3cd3855b43a0cb42f6c747e1c");
    public static readonly BlueprintWeaponEnchantmentGuid NaturesWrath = new BlueprintWeaponEnchantmentGuid("afa5d47f05724ac43a4dc19e5ecbd150");
}

public class BlueprintBuffGuid : BlueprintFactGuid
{
    public BlueprintBuffGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintBuff buff)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Buff blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        buff = cachedObject as BlueprintBuff;

        return buff != null;
    }

    public static readonly BlueprintBuffGuid MagicalVestmentArmor = new BlueprintBuffGuid("9e265139cf6c07c4fb8298cb8b646de9");
    public static readonly BlueprintBuffGuid MagicalVestmentShield = new BlueprintBuffGuid("2e8446f820936a44f951b50d70a82b16");

    public static readonly BlueprintBuffGuid Nauseated = new BlueprintBuffGuid("956331dba5125ef48afe41875a00ca0e");

    public static readonly BlueprintBuffGuid DebilitatingInjuryBewilderedActive = new BlueprintBuffGuid("116ee72b2149f4d44a330296a7e42d13");
    public static readonly BlueprintBuffGuid DebilitatingInjuryBewilderedEffect = new BlueprintBuffGuid("22b1d98502050cb4cbdb3679ac53115e");
    public static readonly BlueprintBuffGuid DebilitatingInjuryDisorientedActive = new BlueprintBuffGuid("6339eac5bdcef1747ac46885d2cf4e25");
    public static readonly BlueprintBuffGuid DebilitatingInjuryDisorientedEffect = new BlueprintBuffGuid("1f1e42f8c06d7dc4bb70cc12c73dfb38");
    public static readonly BlueprintBuffGuid DebilitatingInjuryHamperedActive = new BlueprintBuffGuid("cc9a43f5157309646b23a0a690fee84b");
    public static readonly BlueprintBuffGuid DebilitatingInjuryHamperedEffect = new BlueprintBuffGuid("5bfefc22a68e736488b0c309d9c1c1d4");
}

public class BlueprintItemGuid : BlueprintObjectGuid
{
    public BlueprintItemGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintItem item)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintItem>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Item blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        item = cachedObject as BlueprintItem;

        return item != null;
    }
}

public class BlueprintItemEquipmentGuid : BlueprintItemGuid
{
    public BlueprintItemEquipmentGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintItemEquipment itemEquipment)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipment>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Item Equipment blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        itemEquipment = cachedObject as BlueprintItemEquipment;

        return itemEquipment != null;
    }
}

public class BlueprintItemWeaponGuid : BlueprintItemGuid
{
    public BlueprintItemWeaponGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintItemWeapon itemWeapon)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Item Weapon blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        itemWeapon = cachedObject as BlueprintItemWeapon;

        return itemWeapon != null;
    }

    public static readonly BlueprintItemWeaponGuid SoporiferousSecond = new BlueprintItemWeaponGuid("af87d71820e93364c81b1aff840344ed");
}

public class BlueprintItemEquipmentUsableGuid : BlueprintItemEquipmentGuid
{
    public BlueprintItemEquipmentUsableGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintItemEquipmentUsable itemUsable)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Item Equipment Usable blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        itemUsable = cachedObject as BlueprintItemEquipmentUsable;

        return itemUsable != null;
    }

    public static readonly BlueprintItemEquipmentUsableGuid ScrollSummonNaturesAllyVSingle = new BlueprintItemEquipmentUsableGuid("4e9e261a93c7aa144a7b29c9fcfb4986");
}

public class BlueprintCheckGuid : BlueprintObjectGuid
{
    public BlueprintCheckGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintCheck check)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintCheck>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Check blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        check = cachedObject as BlueprintCheck;

        return check != null;
    }

    public static readonly BlueprintCheckGuid ShrewishGulchLastStageTwoActions = new BlueprintCheckGuid("373d384d88b55a244b74009dc6628b0e");
    public static readonly BlueprintCheckGuid ShrewishGulchLastStageThreeActions = new BlueprintCheckGuid("e4f4fe6042b99cc4790f0103ae10345e");
}

public class BlueprintWeaponTypeGuid : BlueprintObjectGuid
{
    public BlueprintWeaponTypeGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintWeaponType weaponType)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Weapon Type blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        weaponType = cachedObject as BlueprintWeaponType;

        return weaponType != null;
    }

    public static readonly BlueprintWeaponTypeGuid Dart = new BlueprintWeaponTypeGuid("f415ae950523a7843a74d7780dd551af");

    public static readonly BlueprintWeaponTypeGuid GiantSlugTongue = new BlueprintWeaponTypeGuid("4957290cee0b59542808c65c77bfbee3");
}

public class BlueprintFeatureGuid : BlueprintUnitFactGuid
{
    public BlueprintFeatureGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintFeature feature)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintFeature>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Feature blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        feature = cachedObject as BlueprintFeature;

        return feature != null;
    }

    public static readonly BlueprintFeatureGuid DoubleDebilitation = new("dd699394df0ef8847abba26038333f02");

    public static readonly BlueprintFeatureGuid EkunWolfOffensiveMaster = new("64f74b75ed0d1f8478de5245cf061bcc");
    public static readonly BlueprintFeatureGuid EkunWolfDefensiveMaster = new("915616ab61446694dbd73c7d269ea184");
    public static readonly BlueprintFeatureGuid EkunWolfOffensiveBuff = new("29b33987fae4f81448410007f8f9b902");
    public static readonly BlueprintFeatureGuid EkunWolfDefensiveBuff = new("b6cb208baaff10542a8230f1b9f6b26d");

    public static readonly BlueprintFeatureGuid DwarvenChampionEnchant = new("09de6da5469ab1943924139d5145835f");
}

public class BlueprintKingdomUpgradeGuid : BlueprintObjectGuid
{
    public BlueprintKingdomUpgradeGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintKingdomUpgrade kingdomUpgrade)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintKingdomUpgrade>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Kingdom Upgrade blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        kingdomUpgrade = cachedObject as BlueprintKingdomUpgrade;

        return kingdomUpgrade != null;
    }

    public static readonly BlueprintKingdomUpgradeGuid ItsAMagicalPlace = new BlueprintKingdomUpgradeGuid("f9e28dd6f77a0b5468b2325b91c4195c");
    public static readonly BlueprintKingdomUpgradeGuid ResearchOftheCandlemere = new BlueprintKingdomUpgradeGuid("767fc35b0d514cd45a5aaf2ea8681cbb");
}

public class BlueprintKingdomRootGuid : BlueprintObjectGuid
{
    public BlueprintKingdomRootGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out KingdomRoot kingdomRoot)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<KingdomRoot>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Kingdom Root blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        kingdomRoot = cachedObject as KingdomRoot;

        return kingdomRoot != null;
    }

    public static readonly BlueprintKingdomRootGuid KingdomRoot = new("cdba36795a8ae944091f4b9f94e6d689");
}

public class BlueprintSettlementBuildingGuid : BlueprintFactGuid
{
    public BlueprintSettlementBuildingGuid(string guid)
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintSettlementBuilding building)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintSettlementBuilding>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Settlement Building blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        building = cachedObject as BlueprintSettlementBuilding;

        return building != null;
    }

    public static readonly BlueprintSettlementBuildingGuid TempleOfAbadar = new("7ccdcde2587eeb449b0b6cb6e2bfbda6");
}

public class BlueprintKingdomBuffGuid : BlueprintFactGuid
{
    public BlueprintKingdomBuffGuid(string guid) 
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintKingdomBuff buff)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintKingdomBuff>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Kingdom Buff blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        buff = cachedObject as BlueprintKingdomBuff;

        return buff != null;
    }

    public static readonly BlueprintKingdomBuffGuid CandlemereTowerResearch = new("b92f2b91fe37f4f4e8000842069cf4a3");
}

public class BlueprintKingdomRegionGuid : BlueprintObjectGuid
{
    public BlueprintKingdomRegionGuid(string guid) 
        : base(guid)
    {

    }

    public bool GetBlueprint(out BlueprintRegion region)
    {
        if (!cached)
        {
            cachedObject = ResourcesLibrary.TryGetBlueprint<BlueprintRegion>(guid);
            if (cachedObject == null)
            {
                Main.Log.Error($"Could not find Kingdom Region blueprint with GUID '{guid}'");
            }
            cached = true;
        }

        region = cachedObject as BlueprintRegion;

        return region != null;
    }

    public static readonly BlueprintKingdomRegionGuid SouthNarlmarches = new("cd92c3a23b092584a95eb39f64225923");
}