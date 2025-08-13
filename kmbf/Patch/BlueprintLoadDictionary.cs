using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Designers.Quests.Common;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Actions;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Buffs;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

namespace kmbf.Patch
{
    using static kmbf.Blueprint.BlueprintCommands;

    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;
        static public bool Loaded { get => loaded; }

        [HarmonyPostfix]
        public static void BlueprintPatch()
        {
            if (loaded) return;
            loaded = true;

            #region Abilities 

            #region Class
            
            /// Druid

            // Blight Druid Darkness Domain's Moonfire damage scaling
            AddAbilityDamageDiceRankClass(BlueprintAbilityGuid.DarknessDomainGreaterAbility, BlueprintCharacterClassGuid.Druid); 

            /// Kineticist
           
            // Deadly Earth: Metal (and Rare variant) has scaling that does not match other compound elements or other Metal abilities
            // Copy the ContextRankConfigs from the Mud version
            CopySomeComponents(sourceId: BlueprintAbilityAreaEffectGuid.DeadlyEarthMudBlast
                , destinationId: BlueprintAbilityAreaEffectGuid.DeadlyEarthMetalBlast
                , c => c is ContextRankConfig);
            CopySomeComponents(sourceId: BlueprintAbilityAreaEffectGuid.DeadlyEarthMudBlast
                , destinationId: BlueprintAbilityAreaEffectGuid.DeadlyEarthRareMetalBlast
                , c => c is ContextRankConfig);

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMudBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthEarthBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMagmaBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMetalBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();

            #endregion

            #region Spell

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            ChangeAddStatBonusScaledDescriptor(BlueprintBuffGuid.MagicalVestmentShield, ModifierDescriptor.Shield, ModifierDescriptor.ShieldEnhancement);

            #endregion

            #region Buff

            // The "Master" features are accidentally added to Dog by the dialogue. Might as well put the stat bonuses directly on it
            // Plus the Offensive Master feature was accidentally giving the Defensive buff
            CopyComponents(sourceId: BlueprintFeatureGuid.EkunWolfOffensiveBuff, destinationId: BlueprintFeatureGuid.EkunWolfOffensiveMaster);
            CopyComponents(sourceId: BlueprintFeatureGuid.EkunWolfDefensiveBuff, destinationId: BlueprintFeatureGuid.EkunWolfDefensiveMaster);

            #endregion

            AbilitiesFixes.Apply();

            #endregion

            #region Items

            // Make Darts light weapons (like in tabletop)
            SetWeaponTypeLight(BlueprintWeaponTypeGuid.Dart, light: true);

            // 'Datura' weapon attack buff
            ReplaceAttackRollTriggerToWeaponTrigger(BlueprintWeaponEnchantmentGuid.Soporiferous, WaitForAttackResolve: true); // The weapon attack automatically removes the sleep
            SetContextSetAbilityParamsDC(BlueprintWeaponEnchantmentGuid.Soporiferous, 16); // DC is 11 by default, raise it to 16 like in the description
            AddWeaponEnchantment(BlueprintItemWeaponGuid.SoporiferousSecond, BlueprintWeaponEnchantmentGuid.Soporiferous);

            // Bane of the Living / Penalty "Not Undead or Not Construct" instead of "Not Undead and Not Construct"
            if (Main.UMMSettings.BalanceSettings.FixBaneLiving)
            {
                FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.BaneLiving);
            }

            // Nature's Wrath trident "Outsider AND Aberration ..." instead of OR
            // Fix "Electricity Vulnerability" debuff to apply to target instead of initiator
            FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.NaturesWrath);
            ReplaceWeaponBuffOnAttackToWeaponTrigger(BlueprintWeaponEnchantmentGuid.NaturesWrath);

            // Scroll of Summon Nature's Ally V (Single) would Summon Monster V (Single) instead
            ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid.ScrollSummonNaturesAllyVSingle, BlueprintAbilityGuid.SummonMonsterVSingle, BlueprintAbilityGuid.SummonNaturesAllyVSingle);

            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.DwarvenChampionEnchant)
                .RemoveComponents<AddConditionImmunity>()
                .AddComponent<AddCondition>(c => c.Condition = Kingmaker.UnitLogic.UnitCondition.ImmuneToAttackOfOpportunity)
                .Configure();
            
            #endregion

            #region Kingdom

            // Irlene "Relations rank 3" tier 3 gift
            ReplaceArtisan(BlueprintCueGuid.IrleneGiftCue3, BlueprintKingdomArtisanGuid.Woodmaster, BlueprintKingdomArtisanGuid.ShadyTrader);

            // Fix Temple of Abadar being unusable
            {
                if (BlueprintKingdomRootGuid.KingdomRoot.GetBlueprint(out KingdomRoot kingdomRoot))
                {
                    if (BlueprintSettlementBuildingGuid.TempleOfAbadar.GetBlueprint(out BlueprintSettlementBuilding templeOfAbadar) 
                        && !kingdomRoot.Buildings.Contains(templeOfAbadar))
                    {
                        kingdomRoot.Buildings = [.. kingdomRoot.Buildings, templeOfAbadar];
                    }
                }
            }
            
            BlueprintKingdomUpgradeConfigurator.From(BlueprintKingdomUpgradeGuid.ItsAMagicalPlace)
                .EditEventSuccessAnyFinalResult(r =>
                {
                    r.Actions = ActionListFactory.Add(r.Actions, KingdomActionAddBuffConfigurator.NewRegional(BlueprintKingdomBuffGuid.ItsAMagicalPlace, BlueprintRegionGuid.ShrikeHills)
                        .Configure());
                })
                .Configure();

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.ItsAMagicalPlace)
                .RemoveComponents<AbilityScoreCheckBonus>()
                .AddComponent<BuffSkillBonus>(b =>
                {
                    b.Stat = StatType.SkillKnowledgeArcana;
                    b.Descriptor = ModifierDescriptor.Competence;
                    b.Value = 5;
                })
                .AddComponent<BuffSkillBonus>(b =>
                {
                    b.Stat = StatType.SkillLoreReligion;
                    b.Descriptor = ModifierDescriptor.Competence;
                    b.Value = 5;
                })
                .Configure();

            // Assassin's Guild, Thieves Guild, Black Market, Gambling Den
            {
                AlignmentMaskType nonLawfulOrGood = AlignmentMaskType.TrueNeutral | AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.ChaoticEvil;
                BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.AssassinsGuild).SetAlignmentRestriction(nonLawfulOrGood).Configure();
                BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.GamblingDen).SetAlignmentRestriction(nonLawfulOrGood).Configure();
                BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.ThievesGuild)
                    .SetAlignmentRestriction(nonLawfulOrGood)
                    .RemoveComponents<AdjacencyRestriction>() // Adjacency restrictions are easy to work around and not documented by the game, just remove it
                    .Configure();
                if (Main.UMMSettings.BalanceSettings.FixKingdomBuildingAccess)
                {
                    BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.BlackMarket)
                        .SetAlignmentRestriction(nonLawfulOrGood)
                        .SetOtherBuildRestriction([BlueprintSettlementBuildingGuid.ThievesGuild])
                        .Configure();
                }
            }

            BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.EmbassyRow)
                .EditComponent<KingdomEventModifier>(m =>
                {
                    m.ApplyToOpportunities = true;
                    m.OnlyInRegion = Main.UMMSettings.BalanceSettings.FixEmbassyRowGrandDiplomatBonus;
                })
                .AddAdjacencyBonusBuildings(KingdomStats.Type.Culture, BlueprintSettlementBuildingGuid.School)
                .AddAdjacencyBonusBuildings(KingdomStats.Type.Espionage, BlueprintSettlementBuildingGuid.Aviary, BlueprintSettlementBuildingGuid.BlackMarket, BlueprintSettlementBuildingGuid.ThievesGuild)
                .Configure();

            BlueprintKingdomBuffConfigurator.From(BlueprintKingdomBuffGuid.CulRank5_DiscountCulBuildings)
                .AddBuildingCostModifierBuilding(BlueprintSettlementBuildingGuid.Theater)
                .Configure();

            BlueprintKingdomBuffConfigurator.From(BlueprintKingdomBuffGuid.StaRank10_WigmoldSystem)
                .EditAllComponents<KingdomEventModifier>(m =>
                {
                    m.ApplyToOpportunities = true;
                })
                .Configure();

            // "Honor and Duty" is the Lawful Good quest from Renown Across Golarion
            // The quest starts with a Kingdom event which can be assigned a Regent or a General
            // The intention is that a Good or Neutral Regent, or a Good General, would inspire locals
            // to join the cause, while an Evil Regen or a Neutral or Evil General would evict the crusaders from the city
            // The quest objectives are:
            //  1. Deal with the Missionaries (Support or Evict)
            //  2. Wait for news of alliance with Mendev 
            //  ...
            //  5. Hidden Objective called "Fail"
            //
            // The Kingdom event therefore should assign Objective 1's success based on Kingdom Event success (even if the crusaders end up leaving in all cases)
            // Also, a "Good" successful resolution should start objective 2, and an "Evil" successful resolution should complete the entire quest
            // Any failure should fail the entire quest
            //
            // In the actual data, most outcomes are not assigned actions, and an awkward "Final Result" only covers the Success case, and forgets to filter for the "evict" scenarios
            {
                void FixSolution(PossibleEventSolution solution)
                {
                    foreach(EventResult result in solution.Resolutions)
                    {
                        List<GameAction> actions = new List<GameAction>();

                        if (result.Margin == EventResult.MarginType.Success || result.Margin == EventResult.MarginType.GreatSuccess || result.Margin == EventResult.MarginType.AnySuccess)
                        {
                            actions.Add(SetObjectiveStatusConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyProtectOrKickOut, SummonPoolCountTrigger.ObjectiveStatus.Complete).Configure());

                            // We check if the solution includes Good leaders. For regent, this should also allow the Neutral case
                            if ((result.LeaderAlignment & AlignmentMaskType.Good) != 0)
                            {
                                actions.Add(GiveObjectiveConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyWaitForPeopleReaction).Configure());
                            }
                            else
                            {
                                actions.Add(SetObjectiveStatusConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyFail, SummonPoolCountTrigger.ObjectiveStatus.Complete).Configure());
                            }
                        }
                        else
                        {
                            actions.Add(SetObjectiveStatusConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyProtectOrKickOut, SummonPoolCountTrigger.ObjectiveStatus.Fail).Configure());
                            actions.Add(SetObjectiveStatusConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyFail, SummonPoolCountTrigger.ObjectiveStatus.Fail).Configure());
                        }

                        result.Actions = ActionListFactory.Enumerable(actions);
                    }
                }

                BlueprintKingdomEventConfigurator.From(BlueprintKingdomEventGuid.HonorAndDuty)
                    .EditPossibleSolution(LeaderType.Regent, FixSolution)
                    .EditPossibleSolution(LeaderType.General, FixSolution)
                    .EditComponent<EventFinalResults>(c =>
                    {
                        c.Results = []; // All cases are handled in solutions
                    })
                    .Configure();

                BlueprintObjectConfigurator.From(BlueprintComponentListGuid.CapitalThroneRoomActions)
                    .EditFirstGameActionWhere<AlignmentSelector>(a =>
                    {
                        return a.LawfulGood.Action.GetGameActionsRecursive()
                            .OfType<KingdomActionStartEvent>()
                            .Any(s => s.Event.AssetGuid == BlueprintKingdomEventGuid.HonorAndDuty.guid);
                    }, a =>
                    {
                        a.LawfulGood.Action = ActionListFactory.Enumerable(a.LawfulGood.Action.Actions
                            .AddItem(GiveObjectiveConfigurator.New(BlueprintQuestObjectiveGuid.HonorAndDutyProtectOrKickOut).Configure())
                            );
                    });

                BlueprintRandomEncounterConfigurator.From(BlueprintRandomEncounterGuid.HonorAndDuty)
                    .SetPool(EncounterPool.Mixed) // Putting it in the Combat pool makes it harder to get than other special encounters. Plus you don't necessarily fight
                    .Configure();
            }
            #endregion

            TextFixes.Apply();
            EventFixes.Apply();
            OptionalFixes.ApplyAllEnabledFixes();
        }
    }
}
