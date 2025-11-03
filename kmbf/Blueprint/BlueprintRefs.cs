using Kingmaker.Blueprints.Items;

namespace kmbf.Blueprint
{
    public static class ComponentListRefs
    {
        public static BlueprintComponentListGuid CapitalThroneRoomActions = new("d528c9a3bfea3ba4fb69ae5811c15499");
    }

    public static class AbilityRefs
    {
        public static readonly BlueprintAbilityGuid ProtectionFromArrowsCommunal = new("96c9d98b6a9a7c249b6c4572e4977157");
        public static readonly BlueprintAbilityGuid ControlledFireball = new("f72f8f03bf0136c4180cd1d70eb773a5");
        public static readonly BlueprintAbilityGuid RaiseDead = new("a0fc99f0933d01643b2b8fe570caa4c5");
        public static readonly BlueprintAbilityGuid RaiseDead_Cutscene = new("9e3b861c3ed7e404f896d187e995e0ad");
        public static readonly BlueprintAbilityGuid BreathOfLifeTouch = new("cbd03c874e39e6c4795fe0093544f2a2");
        public static readonly BlueprintAbilityGuid JoyfulRapture = new("15a04c40f84545949abeedef7279751a");
        public static readonly BlueprintAbilityGuid BreakEnchantment = new("7792da00c85b9e042a0fdfc2b66ec9a8");

        public static readonly BlueprintAbilityGuid DarknessDomainGreaterAbility = new("31acd268039966940872c916782ae018");
        public static readonly BlueprintAbilityGuid LawDomainGreaterAbility = new("0b1615ec2dabc6f4294a254b709188a4");
        public static readonly BlueprintAbilityGuid ChaosDomainGreaterAbility = new("5d8c4161d21f63e4a99b47d1e99e654e");

        public static readonly BlueprintAbilityGuid DeadlyEarthMudBlast = new("0be97d0e752060f468bbf62ce032b9f5");
        public static readonly BlueprintAbilityGuid DeadlyEarthEarthBlast = new("e29cf5372f89c40489227edc9ffc52be");
        public static readonly BlueprintAbilityGuid DeadlyEarthMagmaBlast = new("c0704daaf6e4c5840a94e7db6d7dbe0e");
        public static readonly BlueprintAbilityGuid DeadlyEarthMetalBlast = new("44804ca6ba7d495439cc9d5ad6d6cfcf");

        public static readonly BlueprintAbilityGuid SummonMonsterVSingle = new("0964bf88b582bed41b74e79596c4f6d9");
        public static readonly BlueprintAbilityGuid SummonNaturesAllyVSingle = new("28ea1b2e0c4a9094da208b4c186f5e4f");
        public static readonly BlueprintAbilityGuid AlchemistFire = new("28ea1b2e0c4a9094da208b4c186f5e4f");
        public static readonly BlueprintAbilityGuid AcidFlask = new("28ea1b2e0c4a9094da208b4c186f5e4f");

        public static readonly BlueprintAbilityGuid MimicOozeSpit = new("3ea0add618aab444bb5a4e2701a3ee4b");
    }

    public static class ActivatableAbilityRefs
    {
        public static readonly BlueprintActivatableAbilityGuid CombatExpertise = new("a75f33b4ff41fc846acbac75d1a88442");
    }

    public static class AbilityAreaEffectRefs
    {
        public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMetalBlast = new("38a2979db34ad0f45a449e5eb174729f");
        public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthRareMetalBlast = new("267f19ba174b21e4d9baf30afd589068");
        public static readonly BlueprintAbilityAreaEffectGuid DeadlyEarthMudBlast = new("0af604484b5fcbb41b328750797e3948");

        public static readonly BlueprintAbilityAreaEffectGuid BalefulGaze = new("1b6dc09a66357e14ab51b7db86e9a29d");

        public static readonly BlueprintAbilityAreaEffectGuid HeartOfIra = new("3a636a7438e92a14386fc460f466be1b");
    }

    public static class CharacterClassRefs
    {
        public static readonly BlueprintCharacterClassGuid ArcaneTrickster = new("9c935a076d4fe4d4999fd48d853e3cf3");
        public static readonly BlueprintCharacterClassGuid Druid = new("610d836f3a3a9ed42a4349b62f002e96");
    }

    public static class CueRefs
    {
        public static readonly BlueprintCueGuid IrleneGiftCue3 = new("03807d3897f73e44b84b476ae63a62f1");

        public static readonly BlueprintCueGuid CandlemereRismelDelayedStartFight = new("f099ec070656d0f45981e29aeac9d190");

        public static readonly BlueprintCueGuid Act2KestenTourToThroneRoom_Cue01 = new("61fd0dbd69f5c354995a559f79888c6f");

        public static readonly BlueprintCueGuid LKBattle_Phase5_Cue_0065 = new("ae9cd8c41b0d9ec46af03bfa1238fd51");
    }

    public static class CheckRefs
    {
        public static readonly BlueprintCheckGuid ShrewishGulchLastStageTwoActions = new("373d384d88b55a244b74009dc6628b0e");
        public static readonly BlueprintCheckGuid ShrewishGulchLastStageThreeActions = new("e4f4fe6042b99cc4790f0103ae10345e");

        public static readonly BlueprintCheckGuid Unrest_AngryMob_FirstCheck_Diplomacy = new("f0a74d43a46cff44e9cefa07710bb1e6");
        public static readonly BlueprintCheckGuid Unrest_AngryMob_FirstCheck_Intimidate = new("46b410623d176764fbd57bdc0e5b921d");
    }

    public static class AnswerRefs
    {
        public static readonly BlueprintAnswerGuid TestOfStrength_PushSolution_Conclusion = new("4df1f2b4fcc3f6243899d843ea3c14a7"); // Answer_0023 in BookEvent-TestOfStrength-NEW
        public static readonly BlueprintAnswerGuid TestOfStrength_BreakWallsSolution_Conclusion = new("6fecd8590fb8a214a92e130a5d6013dd"); // Answer_0047 in BookEvent-TestOfStrength-NEW
    }

    public static class EquipmentEnchantmentRefs
    {
        public static readonly BlueprintEquipmentEnchantmentGuid Charisma4 = new("4dcb81fcbfb11714db17ece0c152ed8b");
    }

    public static class WeaponEnchantmentRefs
    {
        public static readonly BlueprintWeaponEnchantmentGuid Holy = new("28a9964d81fedae44bae3ca45710c140");
        public static readonly BlueprintWeaponEnchantmentGuid Axiomatic = new("0ca43051edefcad4b9b2240aa36dc8d4");
        public static readonly BlueprintWeaponEnchantmentGuid Unholy = new("d05753b8df780fc4bb55b318f06af453");
        public static readonly BlueprintWeaponEnchantmentGuid Anarchic = new("57315bc1e1f62a741be0efde688087e9");

        public static readonly BlueprintWeaponEnchantmentGuid Soporiferous = new("da0a0c76266c96b45aacc34dc6635b28");
        public static readonly BlueprintWeaponEnchantmentGuid BaneLiving = new("e1d6f5e3cd3855b43a0cb42f6c747e1c");
        public static readonly BlueprintWeaponEnchantmentGuid NaturesWrath = new("afa5d47f05724ac43a4dc19e5ecbd150");

        public static readonly BlueprintWeaponEnchantmentGuid LightningArrows = new("d0cab2c642c912245a5c35821db45d0e");
        public static readonly BlueprintWeaponEnchantmentGuid LoversArrows = new("c5301596b1e29a846a92e27344d1844a");
    }

    public static class BuffRefs
    {
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

        public static readonly BlueprintBuffGuid StrengthSurge = new("94dfcf5f3a72ce8478c8de5db69e752b");
        public static readonly BlueprintBuffGuid TouchOfGlory = new("55edcfff497a1e04a963f72c485da5cb");

        public static readonly BlueprintBuffGuid ItsAMagicalPlace = new("670ab5958ff9ab246966ecb968132f37");

        public static readonly BlueprintBuffGuid CraneStyleWingBuff = new("f78a249bacba9924b9595e52495cb02f");

        public static readonly BlueprintBuffGuid CloakOfSoldSoulsCurse = new("40f948d8e5ee2534eb3d701f256f96b5");
        public static readonly BlueprintBuffGuid GentlePersuasionCurse = new("846c41744b0995848a614a86d8cb4272");
        public static readonly BlueprintBuffGuid NarrowPathCurse = new("12fa3abf3176f6f409c34f5d46bf4754");
        public static readonly BlueprintBuffGuid Tormentor = new("cdfc8d816234c5a47bfa0a4377375e59");
        public static readonly BlueprintBuffGuid GameKeeperOfTheFirstWorld = new("3e106161933ea694380f560f967825dc");
    }

    public static class ItemWeaponRefs
    {
        public static readonly BlueprintItemWeaponGuid SoporiferousSecond = new("af87d71820e93364c81b1aff840344ed");
        public static readonly BlueprintItemWeaponGuid ColdIronRapierPlus3 = new("925d399524ae74748ac78b49f48785c0");
        public static readonly BlueprintItemWeaponGuid BladeOfTheMerciful = new("58a5d7af98c250944acf5f54ab88a924");
        public static readonly BlueprintItemWeaponGuid GameKeeperOfTheFirstWorld = new("717369864d0ef1f43a291b7b8f3ce4c9");

        public static readonly BlueprintItemWeaponGuid StandardSling = new("d30a1e8901890a04eaddaceb4abd7002");
        public static readonly BlueprintItemWeaponGuid MasterworkSling = new("15aedc56990d7394eae5364e08d7bc81");
        public static readonly BlueprintItemWeaponGuid SlingPlus1 = new("32786a0a5807d5a448581a8b17864319");
        public static readonly BlueprintItemWeaponGuid SlingPlus2 = new("c293e5abe38167b4e8b8cdaa1e95a054");
        public static readonly BlueprintItemWeaponGuid SlingPlus3 = new("e42a450c2e14e2b458dc5c78a4d7b7ff");
        public static readonly BlueprintItemWeaponGuid SlingPlus4 = new("30157903ff22e464e99e8098070e4de9");
        public static readonly BlueprintItemWeaponGuid SlingPlus5 = new("3bec1e2f21ccfaf4bb8f4539abc5fd0e");

        public static readonly BlueprintItemWeaponGuid StandardSlingStaff = new("dda1a4f8cbf8ad34ca7845ca17313e86");
        public static readonly BlueprintItemWeaponGuid MasterworkSlingStaff = new("9f6b88554d5eb564ab2d247ab11f40b0");
        public static readonly BlueprintItemWeaponGuid SlingStaffPlus1 = new("643b5b476b7fc1942aa24a50dedc8de4");
        public static readonly BlueprintItemWeaponGuid SlingStaffPlus2 = new("1dc78587aa10b0849b863f57615ddce2");
        public static readonly BlueprintItemWeaponGuid SlingStaffPlus3 = new("69245594968164347b63b2ddc3c24eef");
        public static readonly BlueprintItemWeaponGuid SlingStaffPlus4 = new("db4d0076c83d9434a9e8557f805d7b9a");
        public static readonly BlueprintItemWeaponGuid SlingStaffPlus5 = new("565ba27cf9808ae4285cf360df076723");
    }

    public static class ItemEquipmentRingRefs
    {
        public static BlueprintItemEquipmentRingGuid AmethystEncrustedRing = new("3cfeae23012c8b9488b5b36d51ee4a8d");
        public static BlueprintItemEquipmentRingGuid GarnetEncrustedRing = new("489a7b81151c40541b8de88b0eaa6a77");

        public static BlueprintItemEquipmentRingGuid RingOfRecklessCourage = new("06b345774d4a34541b5c91033b3220fb");
    }

    public static class ItemEquipmentUsableRefs
    {
        public static readonly BlueprintItemEquipmentUsableGuid ScrollSummonNaturesAllyVSingle = new("4e9e261a93c7aa144a7b29c9fcfb4986");
    }

    public static class WeaponTypeRefs
    {
        public static readonly BlueprintWeaponTypeGuid Dart = new("f415ae950523a7843a74d7780dd551af");
        public static readonly BlueprintWeaponTypeGuid Javelin = new("a70cea34b275522458654beb3c53fe3f");
        public static readonly BlueprintWeaponTypeGuid Kama = new("f5872eb0deb3a1b48a36549f8d92c19e");
        public static readonly BlueprintWeaponTypeGuid Nunchaku = new("4703b4c0922142f4cbe8895c10a47a9f");
        public static readonly BlueprintWeaponTypeGuid Sai = new("0944f411666c7594aa1398a7476ecf7d");
        public static readonly BlueprintWeaponTypeGuid SlingStaff = new("25da2dc95ed4a6b419608c678f2a9cc3");
        public static readonly BlueprintWeaponTypeGuid Sling = new("f807334ef058b7148a5d1582767c70ab");
        public static readonly BlueprintWeaponTypeGuid ThrowingAxe = new("ca131c71f4fefcb48b30b5991520e01d");

        public static readonly BlueprintWeaponTypeGuid GiantSlugTongue = new("4957290cee0b59542808c65c77bfbee3");
        public static readonly BlueprintWeaponTypeGuid BuletteBite = new("ebb1e708e46d32c4888207913f76e555");
        public static readonly BlueprintWeaponTypeGuid ShockerLizardTouch = new("25bcf03956c51b14682c68d6fc21cc79");
        public static readonly BlueprintWeaponTypeGuid MiteStoneThrow = new("983f63decbbec424c8ed071e725b7717");
    }

    public static class FeatureRefs
    {
        public static readonly BlueprintFeatureGuid DoubleDebilitation = new("dd699394df0ef8847abba26038333f02");

        public static readonly BlueprintFeatureGuid AnimalDomainBaseFeature = new("d577aba79b5727a4ab74627c4c6ba23c");

        public static readonly BlueprintFeatureGuid EkunWolfOffensiveMaster = new("64f74b75ed0d1f8478de5245cf061bcc");
        public static readonly BlueprintFeatureGuid EkunWolfDefensiveMaster = new("915616ab61446694dbd73c7d269ea184");
        public static readonly BlueprintFeatureGuid EkunWolfOffensiveBuff = new("29b33987fae4f81448410007f8f9b902");
        public static readonly BlueprintFeatureGuid EkunWolfDefensiveBuff = new("b6cb208baaff10542a8230f1b9f6b26d");

        public static readonly BlueprintFeatureGuid SolidStrategyEnchant = new("09de6da5469ab1943924139d5145835f");
        public static readonly BlueprintFeatureGuid NecklaceOfDoubleCrosses = new("64d5a59feeb292e49a6c459fe37c3953");
        public static readonly BlueprintFeatureGuid RingOfRecklessCourage = new("aa9102221a042504c94f0a313c4c40a1");

        public static readonly BlueprintFeatureGuid ShatterDefenses = new("61a17ccbbb3d79445b0926347ec07577");

        public static readonly BlueprintFeatureGuid AnimalCompanionUpgradeLeopard = new("b8c98af302ee334499d30a926306327d");

        public static readonly BlueprintFeatureGuid TieflingHeritageFoulspawn = new("a53d760a364cd90429e16aa1e7048d0a");

        public static readonly BlueprintFeatureGuid ExplosionRing = new("9583bd98ef0b65a448ac79c5ec273db8");
    }

    public static class RaceRefs
    {
        public static readonly BlueprintRaceGuid Halfling = new("b0c3ef2729c498f47970bb50fa1acd30");
    }

    public static class FeatureSelectionRefs
    {
        public static readonly BlueprintFeatureSelectionGuid WeaponTrainingSelection = new("b8cecf4e5e464ad41b79d5b42b76b399");
        public static readonly BlueprintFeatureSelectionGuid WeaponTrainingRankUpSelection = new("5f3cc7b9a46b880448275763fe70c0b0");
    }

    public static class ProgressionRefs
    {
        public static readonly BlueprintProgressionGuid SylvanSorcererAnimalCompanionProgression = new("09c91f959fb737f4289d121e595c657c");
    }

    public static class KingdomArtisanRefs
    {
        public static readonly BlueprintKingdomArtisanGuid Woodmaster = new("670c334ec3ecd1640b70024ea93d9229");
        public static readonly BlueprintKingdomArtisanGuid ShadyTrader = new("42efca2aecce9ff43ad3ed2d4d516124");
    }

    public static class KingdomUpgradeRefs
    {
        public static readonly BlueprintKingdomUpgradeGuid ItsAMagicalPlace = new("f9e28dd6f77a0b5468b2325b91c4195c");
        public static readonly BlueprintKingdomUpgradeGuid ResearchOftheCandlemere = new("767fc35b0d514cd45a5aaf2ea8681cbb");
    }

    public static class KingdomRootRefs
    {
        public static readonly BlueprintKingdomRootGuid KingdomRoot = new("cdba36795a8ae944091f4b9f94e6d689");
    }

    public static class SettlementBuildingRefs
    {
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

    public static class KingdomBuffRefs
    {
        public static readonly BlueprintKingdomBuffGuid CandlemereTowerResearch = new("b92f2b91fe37f4f4e8000842069cf4a3");
        public static readonly BlueprintKingdomBuffGuid ItsAMagicalPlace = new("540f7dfba39b96c46bb76680e46077da");
        public static readonly BlueprintKingdomBuffGuid PitaxianSpies = new("51a7482c5e6d485438ca4cefe03d0ded");

        public static readonly BlueprintKingdomBuffGuid CulRank5_DiscountCulBuildings = new("60e1176b4240bda409ce6a2ba7037f8c");
        public static readonly BlueprintKingdomBuffGuid StaRank10_WigmoldSystem = new("133818e83c8fb724fa9a59b547a7e0c6");
    }

    public static class RegionRefs
    {
        public static readonly BlueprintRegionGuid ShrikeHills = new("caacbcf9f6d6561459f526e584ded703");
        public static readonly BlueprintRegionGuid SouthNarlmarches = new("cd92c3a23b092584a95eb39f64225923");
    }

    public static class KingdomEventRefs
    {
        public static readonly BlueprintKingdomEventGuid HonorAndDuty = new("acd37baba5a0d4343b7b184780bd68cf");

        public static readonly BlueprintKingdomEventGuid LureOfTheFirstWorld = new("b29f6fc145f0ac84fa4fa617d6f3a463");
        public static readonly BlueprintKingdomEventGuid LureOfTheFirstWorldSimple = new("e90811a551263844687b86034bafc18b");
    }

    public static class QuestObjectiveRefs
    {
        public static readonly BlueprintQuestObjectiveGuid HonorAndDutyProtectOrKickOut = new("d45987ff311b94d40940167aae5356e9");
        public static readonly BlueprintQuestObjectiveGuid HonorAndDutyWaitForPeopleReaction = new("7368326029429d14286d5447e2dde37b");
        public static readonly BlueprintQuestObjectiveGuid HonorAndDutyFail = new("4adc57ec7cf0ea24b99acf1095eeefd9");
    }

    public static class RandomEncounterRefs
    {
        public static readonly BlueprintRandomEncounterGuid HonorAndDuty = new("d46dea989708a6f4c84e800fdb999449");
    }

    public static class UnlockableFlagRefs
    {
        public static readonly BlueprintUnlockableFlagGuid AngryMob_FirstCheckModifier = new("8258f133257832541bf2b7bd9f99ed05");

        public static readonly BlueprintUnlockableFlagGuid SouthNarlmarches_MagicalUpgrade = new("a069e9ffe15aa214c830c8ef57a7bee0");

        public static readonly BlueprintUnlockableFlagGuid EzvankiDeal = new("00b40c4c7e679cb47be2eeb6cd857311");
    }

    public static class CategoryDefaultsRefs
    {
        public static readonly BlueprintCategoryDefaultsGuid DefaultsForWeaponCategories = new("567dc59213fd9664c8cb291643439714");
    }

    public static class TrashLootSettingsRefs
    {
        public static readonly BlueprintTrashLootSettingsGuid TrashLootSettings = new("b2554a2be56f1c6478c14a442b0f64f3");
        public static readonly BlueprintTrashLootSettingsGuid DLC3TrashLootSettings = new("77fe4de4c707e51439971ee480e94efc");
    }

    public static class SharedVendorTableRefs
    {
        public static readonly BlueprintSharedVendorTableGuid Oleg = new("f720440559fc00949900bfa1575196ac");

        public static readonly BlueprintSharedVendorTableGuid DLC2QuartermasterBase = new("8035c1313902fae4796d36065e769297"); // aka Kjerdi
        public static readonly BlueprintSharedVendorTableGuid DLC2QuartermasterImproved = new("7c68519dca4334c408227bb0140ac50f"); // aka Kjerdi
    }
}
