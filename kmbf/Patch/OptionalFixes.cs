using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom.Actions;
using Kingmaker.Kingdom.Buffs;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using kmbf.Component;
using static kmbf.Blueprint.BlueprintCommands;
using static kmbf.Blueprint.Builder.ElementBuilder;

namespace kmbf.Patch
{
    static class OptionalFixes
    {
        public static void ApplyAllEnabledFixes()
        {
            if (!Main.RunsCallOfTheWild && Main.UMMSettings.BalanceSettings.FixNecklaceOfDoubleCrosses)
                FixNecklaceOfDoubleCrosses();
            if (!Main.RunsCallOfTheWild && Main.UMMSettings.BalanceSettings.FixShatterDefenses)
                FixShatterDefenses();

            if (Main.UMMSettings.BalanceSettings.FixBaneLiving)
                FixBaneOfTheLiving();
            if (Main.UMMSettings.BalanceSettings.FixNauseatedPoisonDescriptor) 
                FixNauseatedPoisonDescriptor();
            if (Main.UMMSettings.BalanceSettings.FixCandlemereTowerResearch) 
                FixCandlemereTowerResearch();         
            if (Main.UMMSettings.BalanceSettings.FixArcaneTricksterAlignmentRequirement) 
                FixArcaneTricksterAlignmentRequirement();
            if (Main.UMMSettings.BalanceSettings.FixCraneWingRequirements) 
                FixCraneWingHandCheck();
            if (Main.UMMSettings.BalanceSettings.FixControlledFireball)
                FixControlledFireball();
            if (Main.UMMSettings.EventSettings.FixFreeEzvankiTemple)
                FixFreeEzvankiTemple();
        }

        // Bane of the Living / Penalty "Not Undead or Not Construct" instead of "Not Undead and Not Construct"
        static void FixBaneOfTheLiving()
        {
            FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.BaneLiving);
        }

        // In the base game, Necklace of Double Crosses applies to all sneak attacks, and the "attack against allies" mechanic is not implemented at all
        private static void FixNecklaceOfDoubleCrosses()
        {
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.NecklaceOfDoubleCrosses)
                .EditComponent<AdditionalSneakDamageOnHit>(c => c.m_Weapon = AdditionalSneakDamageOnHit.WeaponType.Melee)
                .AddComponent<AooAgainstAllies>()
                .Configure();
        }

        // Nauseated buff: remove Poison descriptors
        static void FixNauseatedPoisonDescriptor()
        {
            BlueprintBuffConfigurator.From(BlueprintBuffGuid.Nauseated)
                .RemoveSpellDescriptor(SpellDescriptor.Poison)
                .Configure();
        }            

        // Research of Candlemere gives a global buff to all adjacent regions, rather than give a single buff that applies to adjacent regions
        // Requires a fix in SaveFixes too, like all Kingdom buffs
        static void FixCandlemereTowerResearch()
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

        // Shatter Defenses should, by description, make the target flat-footed until the end of next round if hit while shaken or frightnened
        // In the base game, it instead makes the target flat footed for the current round if shaken or frightened
        // The fix applies a debuff on hit while shaken or frightened, and flat-footed only checks for the debuff (not the conditions)
        // This differs from the CotW fix, which double-checks the conditions for flat-footed
        // Depends on the RuleCheckTargetFlatFooted patch
        static void FixShatterDefenses()
        {
            var shatterDefensesFeatureConfig = BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.ShatterDefenses);
            BlueprintFeature shatterDefenses = shatterDefensesFeatureConfig.Instance;
            if (shatterDefenses == null)
                return;
            
            var shatterDefensesHitBuff = BlueprintBuffConfigurator.New
                (
                    BlueprintBuffGuid.KMBF_ShatterDefensesHit
                    , "ShatterDefensesHit"
                    , shatterDefenses.m_DisplayName
                    , shatterDefenses.m_Description
                    , shatterDefenses.m_Icon
                )
                .AddComponent<NewRoundTrigger>(n =>
                {
                    n.NewRoundActions = ActionListFactory.Single
                    (
                        MakeGameActionConditional
                        (
                            ConditionsCheckerFactory.Single(MakeBuffConditionCheckRoundNumber(3))
                            , ifTrue: ActionListFactory.Single(MakeContextActionRemoveSelf())
                        )
                    );
                })
                .SetStacking(StackingType.Stack)
                .Configure();

            var shatterDefensesAppliedThisRoundBuff = BlueprintBuffConfigurator
                .NewHidden(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, "ShatterDefensesAppliedThisRound")
                .AddComponent<NewRoundTrigger>(n =>
                {
                    n.NewRoundActions = ActionListFactory.Single(MakeContextActionRemoveSelf());
                })
                .SetStacking(StackingType.Stack)
                .Configure();

            shatterDefensesFeatureConfig.AddComponent<AddInitiatorAttackWithWeaponTrigger>(t =>
            {
                t.Action = ActionListFactory.Single
                (
                    MakeGameActionConditional
                    (
                        ConditionsCheckerFactory.From
                        (
                            Operation.And
                            , MakeContextConditionHasConditions([UnitCondition.Shaken, UnitCondition.Frightened], any: true)
                            , MakeContextConditionHasBuffFromCaster(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, not: true)
                        )
                        , ifTrue: ActionListFactory.From
                        (
                            MakeContextActionRemoveBuffFromCaster(BlueprintBuffGuid.KMBF_ShatterDefensesHit)
                            , MakeContextActionApplyUndispelableBuff(BlueprintBuffGuid.KMBF_ShatterDefensesHit, ContextDurationFactory.ConstantRounds(2))
                            , MakeContextActionApplyUndispelableBuff(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, ContextDurationFactory.ConstantRounds(1))
                        )
                       
                    )
                );
                t.WaitForAttackResolve = true;
            })
            .Configure();
        }

        // In the base game, "Ezvanki's Offer" is always offered, when it should only be given on a successful diplomacy check
        static void FixFreeEzvankiTemple()
        {
            BlueprintCueConfigurator.From(BlueprintCueGuid.Act2KestenTourToThroneRoom_Cue01)
                .EditOnStopActionWhere<Conditional>(c =>
                {
                    return c.IfTrue.HasActions && c.IfTrue.Actions[0].name.Equals("$KingdomActionStartEvent$9f6659ab-f2f5-4481-b254-0d03340b7ba4");
                }, c =>
                {
                    c.ConditionsChecker = ConditionsCheckerFactory.WithCondition(c.ConditionsChecker, MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid.EzvankiDeal));
                })
                .Configure();
        }

        // Both tabletop and in-game encyclopedia say Arcane Trickster requirement non-lawful, but the game (or WotR) does not enforce it
        static void FixArcaneTricksterAlignmentRequirement()
        {
            BlueprintCharacterClassConfigurator.From(BlueprintCharacterClassGuid.ArcaneTrickster)
                .SetAlignmentRestriction(AlignmentMaskType.NeutralGood | AlignmentMaskType.TrueNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.Chaotic)
                .Configure();
        }

        // Crane Wing should require a free hand to give its bonus
        // This means that 1) nothing should be equipped there 2) it shouldn't be used for any purpose (ex: wielding with two hands)
        // KM easily supports checking for a shield, but the rest is a bit of a mess without adding a full-on "Wield 1h weapon with Two Hands" toggle to turn off
        static void FixCraneWingHandCheck()
        {
            BlueprintBuffConfigurator.From(BlueprintBuffGuid.CraneStyleWingBuff)
                .EditComponent<ACBonusAgainstAttacks>(c => c.NoShield = true)
                .Configure();
        }
        
        // Controlled Fireball has the default Target Type of "Enemy". This is fixed to "Any" in Wrath
        static void FixControlledFireball()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.ControlledFireball)
                .EditComponent<AbilityTargetsAround>(c => c.m_TargetType = TargetType.Any)
                .Configure();
        }
    }
}
