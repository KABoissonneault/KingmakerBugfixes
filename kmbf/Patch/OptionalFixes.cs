using Kingmaker.Designers.Mechanics.Facts;
using kmbf.Blueprint;
using static kmbf.Blueprint.Builder.ElementBuilder;
using kmbf.Blueprint.Configurator;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic;
using Kingmaker.ElementsSystem;

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
    }
}
