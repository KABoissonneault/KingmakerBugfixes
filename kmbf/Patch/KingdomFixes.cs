using HarmonyLib;
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
using Kingmaker.Kingdom.Artisans;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Buffs;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.UnitLogic.Alignments;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

namespace kmbf.Patch
{
    static class KingdomFixes
    {
        public static void Apply()
        {
            FixIrleneGift();
            FixTemplateOfAbadar();
            FixItsAMagicalPlace();
            FixAlignedBuildings();
            FixEmbassyRow();
            FixStatRankUpgrades();

            FixHonorAndDuty();
        }

        // Irlene "Relations rank 3" tier 3 gift
        private static void FixIrleneGift()
        {
            BlueprintCueConfigurator.From(BlueprintCueGuid.IrleneGiftCue3)
                .EditOnShowActionRecursive<KingdomActionGetArtisanGiftWithCertainTier>("$KingdomActionGetArtisanGiftWithCertainTier$cde3c826-6adc-4681-aee0-ed4c0c9f218c", a =>
                {
                    if (BlueprintKingdomArtisanGuid.ShadyTrader.GetBlueprint(out BlueprintKingdomArtisan artisan))
                    {
                        a.Artisan = artisan;
                    }
                })
                .Configure();
        }

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
        private static void FixHonorAndDuty()
        {
            
            void FixSolution(PossibleEventSolution solution)
            {
                foreach (EventResult result in solution.Resolutions)
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
                })
                .Configure();

            BlueprintRandomEncounterConfigurator.From(BlueprintRandomEncounterGuid.HonorAndDuty)
                .SetPool(EncounterPool.Mixed) // Putting it in the Combat pool makes it harder to get than other special encounters. Plus you don't necessarily fight
                .Configure();
        }

        private static void FixStatRankUpgrades()
        {
            BlueprintKingdomBuffConfigurator.From(BlueprintKingdomBuffGuid.CulRank5_DiscountCulBuildings)
                            .AddBuildingCostModifierBuilding(BlueprintSettlementBuildingGuid.Theater)
                            .Configure();

            BlueprintKingdomBuffConfigurator.From(BlueprintKingdomBuffGuid.StaRank10_WigmoldSystem)
                .EditAllComponents<KingdomEventModifier>(m =>
                {
                    m.ApplyToOpportunities = true;
                })
                .Configure();
        }

        private static void FixEmbassyRow()
        {
            BlueprintSettlementBuildingConfigurator.From(BlueprintSettlementBuildingGuid.EmbassyRow)
                            .EditComponent<KingdomEventModifier>(m =>
                            {
                                m.ApplyToOpportunities = true;
                                m.OnlyInRegion = Main.UMMSettings.BalanceSettings.FixEmbassyRowGrandDiplomatBonus;
                            })
                            .AddAdjacencyBonusBuildings(KingdomStats.Type.Culture, BlueprintSettlementBuildingGuid.School)
                            .AddAdjacencyBonusBuildings(KingdomStats.Type.Espionage, BlueprintSettlementBuildingGuid.Aviary, BlueprintSettlementBuildingGuid.BlackMarket, BlueprintSettlementBuildingGuid.ThievesGuild)
                            .Configure();
        }


        // Assassin's Guild, Thieves Guild, Black Market, Gambling Den
        private static void FixAlignedBuildings()
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

        // "It's a Magical Place" buff is not properly granted by the region upgrade
        // Even if it were, it uses the obscure AbilityScoreCheckBonus component to add the bonus, 
        // which is otherwise mostly used for ability scores, not skills
        // Using BuffSkillBonus seems to work properly
        private static void FixItsAMagicalPlace()
        {
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
        }

        // Fix Temple of Abadar being unusable
        static void FixTemplateOfAbadar()
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
    }    
}
