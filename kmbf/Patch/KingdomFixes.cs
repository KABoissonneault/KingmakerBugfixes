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

using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch
{
    static class KingdomFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Kingdom patches");

            FixIrleneGift();
            FixTempleOfAbadar();
            FixItsAMagicalPlace();
            FixAlignedBuildings();
            FixEmbassyRow();
            FixStatRankUpgrades();

            FixHonorAndDuty();
            FixLureOfTheFirstWorld();

            // Optional
            FixCandlemereTowerResearch();
        }

        // Irlene "Relations rank 3" tier 3 gift
        private static void FixIrleneGift()
        {
            if (!StartPatch("Irlene Relations Rank 3 Tier 3 Gift")) return;

            BlueprintCueConfigurator.From(CueRefs.IrleneGiftCue3)
                .EditOnShowActionRecursive<KingdomActionGetArtisanGiftWithCertainTier>("$KingdomActionGetArtisanGiftWithCertainTier$cde3c826-6adc-4681-aee0-ed4c0c9f218c", a =>
                {
                    if (KingdomArtisanRefs.ShadyTrader.GetBlueprint(out BlueprintKingdomArtisan artisan))
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
            if (!StartPatch("Honor and Duty")) return;

            void FixSolution(PossibleEventSolution solution)
            {
                foreach (EventResult result in solution.Resolutions)
                {
                    List<GameAction> actions = new List<GameAction>();

                    if (result.Margin == EventResult.MarginType.Success || result.Margin == EventResult.MarginType.GreatSuccess || result.Margin == EventResult.MarginType.AnySuccess)
                    {
                        actions.Add(SetObjectiveStatusConfigurator.New(QuestObjectiveRefs.HonorAndDutyProtectOrKickOut, SummonPoolCountTrigger.ObjectiveStatus.Complete).Configure());

                        // We check if the solution includes Good leaders. For regent, this should also allow the Neutral case
                        if ((result.LeaderAlignment & AlignmentMaskType.Good) != 0)
                        {
                            actions.Add(GiveObjectiveConfigurator.New(QuestObjectiveRefs.HonorAndDutyWaitForPeopleReaction).Configure());
                        }
                        else
                        {
                            actions.Add(SetObjectiveStatusConfigurator.New(QuestObjectiveRefs.HonorAndDutyFail, SummonPoolCountTrigger.ObjectiveStatus.Complete).Configure());
                        }
                    }
                    else
                    {
                        actions.Add(SetObjectiveStatusConfigurator.New(QuestObjectiveRefs.HonorAndDutyProtectOrKickOut, SummonPoolCountTrigger.ObjectiveStatus.Fail).Configure());
                        actions.Add(SetObjectiveStatusConfigurator.New(QuestObjectiveRefs.HonorAndDutyFail, SummonPoolCountTrigger.ObjectiveStatus.Fail).Configure());
                    }

                    result.Actions = ActionListFactory.Enumerable(actions);
                }
            }

            BlueprintKingdomEventConfigurator.From(KingdomEventRefs.HonorAndDuty)
                .EditPossibleSolution(LeaderType.Regent, FixSolution)
                .EditPossibleSolution(LeaderType.General, FixSolution)
                .EditComponent<EventFinalResults>(c =>
                {
                    c.Results = []; // All cases are handled in solutions
                })
                .Configure();

            BlueprintObjectConfigurator.From(ComponentListRefs.CapitalThroneRoomActions)
                .EditFirstGameActionWhere<AlignmentSelector>(a =>
                {
                    return a.LawfulGood.Action.GetGameActionsRecursive()
                        .OfType<KingdomActionStartEvent>()
                        .Any(s => s.Event.AssetGuid == KingdomEventRefs.HonorAndDuty.guid);
                }, a =>
                {
                    a.LawfulGood.Action = ActionListFactory.Enumerable(a.LawfulGood.Action.Actions
                        .AddItem(GiveObjectiveConfigurator.New(QuestObjectiveRefs.HonorAndDutyProtectOrKickOut).Configure())
                        );
                })
                .Configure();

            BlueprintRandomEncounterConfigurator.From(RandomEncounterRefs.HonorAndDuty)
                .SetPool(EncounterPool.Mixed) // Putting it in the Combat pool makes it harder to get than other special encounters. Plus you don't necessarily fight
                .Configure();
        }

        private static void FixStatRankUpgrades()
        {
            if (StartPatch("Improving Cultural Development Theater"))
            {
                BlueprintKingdomBuffConfigurator.From(KingdomBuffRefs.CulRank5_DiscountCulBuildings)
                                .AddBuildingCostModifierBuilding(SettlementBuildingRefs.Theater)
                                .Configure();
            }

            if (StartPatch("Wigmold System Opportunities"))
            {
                BlueprintKingdomBuffConfigurator.From(KingdomBuffRefs.StaRank10_WigmoldSystem)
                    .EditAllComponents<KingdomEventModifier>(m =>
                    {
                        m.ApplyToOpportunities = true;
                    })
                    .Configure();
            }
        }

        private static void FixEmbassyRow()
        {
            if (StartPatch("Embassy Row Adjacency"))
            {
                BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.EmbassyRow)
                    .EditComponent<KingdomEventModifier>(m =>
                    {
                        m.ApplyToOpportunities = true;
                    })
                    .AddAdjacencyBonusBuildings(KingdomStats.Type.Culture, SettlementBuildingRefs.School)
                    .AddAdjacencyBonusBuildings(KingdomStats.Type.Espionage, SettlementBuildingRefs.Aviary, SettlementBuildingRefs.BlackMarket, SettlementBuildingRefs.ThievesGuild)
                    .Configure();
            }

            if (StartBalancePatch("Embassy Row Grand Diplomat Region", nameof(BalanceSettings.FixEmbassyRowGrandDiplomatBonus)))
            {
                BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.EmbassyRow)
                    .EditComponent<KingdomEventModifier>(m =>
                    {
                        m.OnlyInRegion = true;
                    })
                    .Configure();
            }
        }


        // Assassin's Guild, Thieves Guild, Black Market, Gambling Den
        private static void FixAlignedBuildings()
        {
            if (!StartPatch("Building Alignment Requirement")) return;

            AlignmentMaskType nonLawfulOrGood = AlignmentMaskType.TrueNeutral | AlignmentMaskType.ChaoticNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.ChaoticEvil;
            
            BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.AssassinsGuild)
                .SetAlignmentRestriction(nonLawfulOrGood)
                .Configure();
            BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.GamblingDen)
                .SetAlignmentRestriction(nonLawfulOrGood)
                .Configure();
            BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.ThievesGuild)
                .SetAlignmentRestriction(nonLawfulOrGood)
                .RemoveComponents<AdjacencyRestriction>() // Adjacency restrictions are easy to work around and not documented by the game, just remove it
                .Configure();

            if (StartBalancePatch("Black Market Alignemnt Requirement", nameof(BalanceSettings.FixKingdomBuildingAccess)))
            {
                BlueprintSettlementBuildingConfigurator.From(SettlementBuildingRefs.BlackMarket)
                    .SetAlignmentRestriction(nonLawfulOrGood)
                    .SetOtherBuildRestriction([SettlementBuildingRefs.ThievesGuild])
                    .Configure();
            }
        }

        // "It's a Magical Place" buff is not properly granted by the region upgrade
        // Even if it were, it uses the obscure AbilityScoreCheckBonus component to add the bonus, 
        // which is otherwise mostly used for ability scores, not skills
        // Using BuffSkillBonus seems to work properly
        private static void FixItsAMagicalPlace()
        {
            if (!StartPatch("It's a Magical Place")) return;

            BlueprintKingdomUpgradeConfigurator.From(KingdomUpgradeRefs.ItsAMagicalPlace)
                .EditEventSuccessAnyFinalResult(r =>
                {
                    r.Actions = ActionListFactory.Add(r.Actions, KingdomActionAddBuffConfigurator.NewRegional(KingdomBuffRefs.ItsAMagicalPlace, RegionRefs.ShrikeHills)
                        .Configure());
                })
                .Configure();

            BlueprintBuffConfigurator.From(BuffRefs.ItsAMagicalPlace)
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
        static void FixTempleOfAbadar()
        {
            if (!StartPatch("Temple of Abadar")) return;

            if (KingdomRootRefs.KingdomRoot.GetBlueprint(out KingdomRoot kingdomRoot))
            {
                if (SettlementBuildingRefs.TempleOfAbadar.GetBlueprint(out BlueprintSettlementBuilding templeOfAbadar)
                    && !kingdomRoot.Buildings.Contains(templeOfAbadar))
                {
                    kingdomRoot.Buildings = [.. kingdomRoot.Buildings, templeOfAbadar];
                }
            }
        }

        static void FixLureOfTheFirstWorld()
        {
            if (!StartPatch("Lure of the First World")) return;

            BlueprintKingdomEventConfigurator.From(KingdomEventRefs.LureOfTheFirstWorld)
                .CopyPossibleSolutionResolutions(fromLeader: LeaderType.Regent, toLeader: LeaderType.Counsilor)
                .EditPossibleSolution(LeaderType.Regent, s =>
                {
                    s.CanBeSolved = false;
                    s.Resolutions = [];
                })
                .EditPossibleSolution(LeaderType.Counsilor, s =>
                {
                    s.CanBeSolved = true;
                })
                .Configure();

            BlueprintKingdomEventConfigurator.From(KingdomEventRefs.LureOfTheFirstWorldSimple)
                .CopyPossibleSolutionResolutions(fromLeader: LeaderType.Regent, toLeader: LeaderType.Counsilor)
                .EditPossibleSolution(LeaderType.Regent, s =>
                {
                    s.CanBeSolved = false;
                    s.Resolutions = [];
                })
                .EditPossibleSolution(LeaderType.Counsilor, s =>
                {
                    s.CanBeSolved = true;
                })
                .Configure();
        }

        // Research of Candlemere gives a global buff to all adjacent regions, rather than give a single buff that applies to adjacent regions
        // Requires a fix in SaveFixes too, like all Kingdom buffs
        static void FixCandlemereTowerResearch()
        {
            if (!StartBalancePatch("Candlemere Tower Research", nameof(BalanceSettings.FixCandlemereTowerResearch))) return;

            BlueprintKingdomUpgradeConfigurator.From(KingdomUpgradeRefs.ResearchOftheCandlemere)
                .EditEventSuccessAnyFinalResult(r =>
                {
                    var addBuffAction = r.Actions.GetGameAction<KingdomActionAddBuff>();
                    if (addBuffAction != null)
                    {
                        addBuffAction.CopyToAdjacentRegions = false;
                    }
                })
                .Configure();

            BlueprintKingdomBuffConfigurator.From(KingdomBuffRefs.CandlemereTowerResearch)
                .EditComponent<KingdomEventModifier>(c =>
                {
                    c.OnlyInRegion = true;
                    c.IncludeAdjacent = true;
                })
                .Configure();
        }
    }    
}
