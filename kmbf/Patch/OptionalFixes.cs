using Kingmaker.Designers.Mechanics.Facts;
using kmbf.Blueprint;
using static kmbf.Blueprint.Builder.ElementBuilder;
using kmbf.Blueprint.Configurator;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic;
using Kingmaker.ElementsSystem;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.UnitLogic.Alignments;

namespace kmbf.Patch
{
    static class OptionalFixes
    {
        // Depends on the RuleCheckTargetFlatFooted patch
        public static void FixShatterDefenses()
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

        public static void FixFreeEvzankiTemple()
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
        public static void FixArcaneTricksterAlignmentRequirement()
        {
            BlueprintCharacterClassConfigurator.From(BlueprintCharacterClassGuid.ArcaneTrickster)
                .SetAlignmentRestriction(AlignmentMaskType.NeutralGood | AlignmentMaskType.TrueNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.Chaotic)
                .Configure();
        }
    }
}
