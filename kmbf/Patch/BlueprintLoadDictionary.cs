using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
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
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Action;
using kmbf.Blueprint;
using static kmbf.Blueprint.Builder.ElementBuilder;
using kmbf.Blueprint.Configurator;
using kmbf.Component;
using UnityEngine;

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

            /// Rogue
            FixDoubleDebilitatingInjury();

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

            FixRaiseDead();
            FixBreathOfLife();

            #endregion

            #region Feat
            if(!Main.RunsCallOfTheWild && Main.UMMSettings.BalanceSettings.FixShatterDefenses)
            {
                OptionalFixes.FixShatterDefenses();
            }
            #endregion

            #region Buff

            // The "Master" features are accidentally added to Dog by the dialogue. Might as well put the stat bonuses directly on it
            // Plus the Offensive Master feature was accidentally giving the Defensive buff
            CopyComponents(sourceId: BlueprintFeatureGuid.EkunWolfOffensiveBuff, destinationId: BlueprintFeatureGuid.EkunWolfOffensiveMaster);
            CopyComponents(sourceId: BlueprintFeatureGuid.EkunWolfDefensiveBuff, destinationId: BlueprintFeatureGuid.EkunWolfDefensiveMaster);

            // Nauseated buff: remove Poison descriptor
            if (Main.UMMSettings.BalanceSettings.FixNauseatedPoisonDescriptor)
            {
                RemoveSpellDescriptor(BlueprintBuffGuid.Nauseated, SpellDescriptor.Poison);
            }

            #endregion

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

            if (!Main.RunsCallOfTheWild && Main.UMMSettings.BalanceSettings.FixNecklaceOfDoubleCrosses)
            {
                BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.NecklaceOfDoubleCrosses)
                    .EditComponent<AdditionalSneakDamageOnHit>(c => c.m_Weapon = AdditionalSneakDamageOnHit.WeaponType.Melee)
                    .AddComponent<AooAgainstAllies>();
            }

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

            // Research of Candlemere gives a global buff to all adjacent regions, rather than give a single buff that applies to adjacent regions
            if (Main.UMMSettings.BalanceSettings.FixCandlemereTowerResearch)
            {
                BlueprintKingdomUpgradeConfigurator.From(BlueprintKingdomUpgradeGuid.ResearchOftheCandlemere)
                    .EditEventSuccessAnyFinalResult(r =>
                    {
                        var addBuffAction = r.Actions.GetGameAction<KingdomActionAddBuff>();
                        if (addBuffAction != null)
                        {
                            addBuffAction.CopyToAdjacentRegions = false;
                        }
                    })
                    .Configure();

                BlueprintKingdomBuffConfigurator.From(BlueprintKingdomBuffGuid.CandlemereTowerResearch)
                    .EditComponent<KingdomEventModifier>(c =>
                    {
                        c.OnlyInRegion = true;
                        c.IncludeAdjacent = true;
                    })
                    .Configure();
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
                .AddAdjacencyBonusBuildings(KingdomStats.Type.Espionage, BlueprintSettlementBuildingGuid.Aviary, BlueprintSettlementBuildingGuid.BlackMarket)
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

            #region Script

            // Shrewish Gulch Last Stage "Two Actions" and "Three Actions" checks
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageTwoActions, StatType.SkillLoreNature, StatType.SkillAthletics);
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageThreeActions, StatType.SkillLoreNature, StatType.SkillAthletics);

            // Fix the "Magic of the Candlemere Tower" region upgrade not getting unlocked when delaying the Duke Dazzleflare fight
            BlueprintCueConfigurator.From(BlueprintCueGuid.CandlemereRismelDelayedStartFight)
                .AddOnStopAction(UnlockFlagConfigurator.New(BlueprintUnlockableFlagGuid.SouthNarlmarches_MagicalUpgrade, 1).Configure());

            // Unrest in the Streets Angry First Check DC goes from DC23 at 0, to 18 at -1, to 23 at -2 and -3, to -22 at -4
            // Fix the modifiers to actually check for -2 and -3 instead of all three checking for -4, giving the intended DC progression
            BlueprintCheckConfigurator.From(BlueprintCheckGuid.Unrest_AngryMob_FirstCheck_Diplomacy)
                .EditDCModifierAt(4, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -2)))
                .EditDCModifierAt(5, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -3)))
                .EditDCModifierAt(6, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -4)))
                .Configure();

            BlueprintCheckConfigurator.From(BlueprintCheckGuid.Unrest_AngryMob_FirstCheck_Intimidate)
                .EditDCModifierAt(4, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -2)))
                .EditDCModifierAt(5, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -3)))
                .EditDCModifierAt(6, m => m.Conditions = ConditionsCheckerFactory.Single(MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.AngryMob_FirstCheckModifier, -4)))
                .Configure();

            #endregion

            #region UI and Text

            // Ooze Spit info
            SetDisplayName(BlueprintAbilityGuid.MimicOozeSpit, KMLocalizedStrings.Spit);
            SetDescription(BlueprintAbilityGuid.MimicOozeSpit, KMBFLocalizedStrings.OozeSpitDescription);

            SetTypeName(BlueprintWeaponTypeGuid.GiantSlugTongue, KMBFLocalizedStrings.Tongue);
            SetDefaultName(BlueprintWeaponTypeGuid.GiantSlugTongue, KMBFLocalizedStrings.Tongue);

            #endregion
        }

        // Debilitating Injuries simply do not account for Double Debilitation, and will remove all existing injuries upon applying a new one
        // This fix adds a Condition on the Double Debilitation feature, which if true, will check whether the target has *two* existing buffs
        // before removing them
        static void FixDoubleDebilitatingInjury()
        {
            if (!BlueprintFeatureGuid.DoubleDebilitation.GetBlueprint(out BlueprintFeature DoubleDebilitation)) return;
            if (!BlueprintBuffGuid.DebilitatingInjuryBewilderedActive.GetBlueprint(out BlueprintBuff BewilderedActive)) return;
            if (!BlueprintBuffGuid.DebilitatingInjuryDisorientedActive.GetBlueprint(out BlueprintBuff DisorientedActive)) return;
            if (!BlueprintBuffGuid.DebilitatingInjuryHamperedActive.GetBlueprint(out BlueprintBuff HamperedActive)) return;

            Conditional MakeConditional(BlueprintBuffGuid[] otherActives, BlueprintBuffGuid[] otherBuffs, Conditional[] normalConditionals)
            {
                var featureCondition = ScriptableObject.CreateInstance<ContextConditionCasterHasFact>();
                featureCondition.Fact = DoubleDebilitation;

                // If the target has at least two of the "other injuries" from the caster, remove all the existing ones
                var doubleDebilitationAction = MakeGameActionConditional
                (
                    ConditionsCheckerFactory.Single(MakeContextConditionHasBuffsFromCaster("Debilitating Injury", otherBuffs, 2)),
                    ifTrue: ActionListFactory.From
                    (
                        MakeContextActionRemoveTargetBuffIfInitiatorNotActive(buffId: otherBuffs[0], activeId: otherActives[0])
                        , MakeContextActionRemoveTargetBuffIfInitiatorNotActive(buffId: otherBuffs[1], activeId: otherActives[1])
                    )
                );

                // If user has Double Debilitation, use the new "Remove if two buffs". Else, use the current "Remove if any buff"

                return MakeGameActionConditional
                (
                    ConditionsCheckerFactory.Single(featureCondition)
                    , ifTrue: ActionListFactory.From(doubleDebilitationAction)
                    , ifFalse: ActionListFactory.From(normalConditionals)
                );
            }

            void Fixup(BlueprintBuff active, BlueprintBuffGuid[] otherActives, BlueprintBuffGuid[] otherBuffs)
            {
                var triggerComponent = active.GetComponent<AddInitiatorAttackRollTrigger>();
                ActionList triggerActions = triggerComponent.Action;
                Conditional[] conditionals = triggerActions.Actions.OfType<Conditional>().ToArray();

                triggerActions.Actions = triggerActions.Actions.Where(a => !(a is Conditional))
                    .AddItem(MakeConditional(otherActives, otherBuffs, conditionals))
                    .ToArray();
            }

            Fixup
            (
                BewilderedActive
                , [BlueprintBuffGuid.DebilitatingInjuryDisorientedActive, BlueprintBuffGuid.DebilitatingInjuryHamperedActive]
                , [BlueprintBuffGuid.DebilitatingInjuryDisorientedEffect, BlueprintBuffGuid.DebilitatingInjuryHamperedEffect]
            );
            Fixup
            (
                DisorientedActive
                , [BlueprintBuffGuid.DebilitatingInjuryHamperedActive, BlueprintBuffGuid.DebilitatingInjuryBewilderedActive]
                , [BlueprintBuffGuid.DebilitatingInjuryHamperedEffect, BlueprintBuffGuid.DebilitatingInjuryBewilderedEffect]
            );
            Fixup
            (
                HamperedActive
                , [BlueprintBuffGuid.DebilitatingInjuryDisorientedActive, BlueprintBuffGuid.DebilitatingInjuryBewilderedActive]
                , [BlueprintBuffGuid.DebilitatingInjuryDisorientedEffect, BlueprintBuffGuid.DebilitatingInjuryBewilderedEffect]
            );

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.DebilitatingInjuryHamperedEffect)
                .SetIcon(HamperedActive.Icon)
                .Configure();
        }
        
        // Raise Dead does not actually give two negative levels
        // Like in Wrath of the Righteous, we add a condition on whether Enemy Stats Adjustment is Normal or above
        static void FixRaiseDead()
        {
            if (!BlueprintAbilityGuid.RaiseDead.GetBlueprint(out BlueprintScriptableObject raiseDead)) return;

            AbilityEffectRunAction runAction = raiseDead.GetComponent<AbilityEffectRunAction>();
            if(runAction == null)
            {
                Main.Log.Error($"Could not find Ability Effect Run Action in Blueprint {BlueprintAbilityGuid.RaiseDead.GetDebugName()}");
                return;
            }

            var difficultyCheck = ContextConditionDifficultyHigherThanConfigurator
                .New(BlueprintRoot.Instance.DifficultyList.CoreDifficulty)
                .SetCheckOnlyForMonsterCaster(false)
                .Configure();

            var dealDamageAction = ContextActionDealDamageConfigurator
                .NewPermanentEnergyDrain(ContextDiceFactory.BonusConstant(2))
                .Configure();

            Conditional difficultyConditional = MakeGameActionConditional
            (
                new ConditionsChecker() { Conditions = [difficultyCheck] }
                , ifTrue: new ActionList() { Actions = [dealDamageAction] }
             );

            // Put the drain first, resurrection makes the unit untargetable
            runAction.Actions.Actions = [difficultyConditional, .. runAction.Actions.Actions];
        }

        static void FixBreathOfLife()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.BreathOfLifeTouch)
                .SetFullRoundAction(false)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var aliveConditional = c.Actions.GetGameAction<Conditional>();
                    if(aliveConditional != null)
                    {
                        var isPartyMemberConditional = aliveConditional.IfFalse.GetGameAction<Conditional>();
                        if(isPartyMemberConditional != null)
                        {

                            var difficultyConditional = MakeGameActionConditional
                            (
                                ConditionsCheckerFactory.Single
                                (
                                    ContextConditionDifficultyHigherThanConfigurator.New(BlueprintRoot.Instance.DifficultyList.CoreDifficulty)
                                        .SetCheckOnlyForMonsterCaster(false)
                                        .Configure()
                                ),
                                ifTrue: ActionListFactory.From
                                (
                                    ContextActionDealDamageConfigurator
                                        .NewTemporaryEnergyDrain(ContextDiceFactory.BonusConstant(1), ContextDurationFactory.ConstantDays(1))
                                        .Configure()
                                )
                            );

                            isPartyMemberConditional.IfTrue.Actions = [difficultyConditional, .. isPartyMemberConditional.IfTrue.Actions];
                        }
                    }
                })
                .Configure();

        }
    }
}
