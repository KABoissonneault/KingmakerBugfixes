using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using UnityEngine;

using static kmbf.Blueprint.Builder.ElementBuilder;

namespace kmbf.Patch
{
    static class AbilitiesFixes
    {
        public static void Apply()
        {
            FixDoubleDebilitatingInjury();
            FixRaiseDead();
            FixBreathOfLife();
            FixJoyfulRapture();
            FixProtectionFromArrows();
            FixLeopardCompanionUpgrade();
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
            if (runAction == null)
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

        // Breath of Life does not actually give one negative level when used to resurrect
        // Like in Wrath of the Righteous, we add a condition on whether Enemy Stats Adjustment is Normal or above
        static void FixBreathOfLife()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.BreathOfLifeTouch)
                .SetFullRoundAction(false)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var aliveConditional = c.Actions.GetGameAction<Conditional>();
                    if (aliveConditional != null)
                    {
                        var isPartyMemberConditional = aliveConditional.IfFalse.GetGameAction<Conditional>();
                        if (isPartyMemberConditional != null)
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

        // Joyful Rapture is supposed to free all allies from any "emotion effects", but the base game only includes Petrified
        // Add Fear, Shaken, Frightened, and NegativeEmotion, which include the "Unbreakable Heart" descriptors, plus overall
        // negative emotion
        static void FixJoyfulRapture()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.JoyfulRapture)
                .EditComponentGameAction<AbilityEffectRunAction, ContextActionDispelMagic>("$ContextActionDispelBuffs$b4781573-55ad-4e71-9dd9-75a0c38652e0", a =>
                {
                    a.Descriptor |= SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Frightened | SpellDescriptor.NegativeEmotion;
                })
                .Configure();
        }

        // Protection From Arrows Communal should not have spell resistance, like Protection from Arrows, and like other communal buffs
        static void FixProtectionFromArrows()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.ProtectionFromArrowsCommunal)
                .SetSpellResistance(false)
                .Configure();

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.ProtectionFromArrows)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.Or = true;
                    c.BypassedByMagic = true;
                    c.BypassedByMeleeWeapon = true;
                    c.Value = ContextValueFactory.Simple(10);
                    c.UsePool = true;
                    c.Pool = ContextValueFactory.Rank();
                })
                .AddOrEditDefaultContextRankConfig(c =>
                {
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.SetMultiplyByModifier(step: 10, max: 100);
                })
                .Configure();

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.ProtectionFromArrowsCommunal)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.Or = true;
                    c.BypassedByMagic = true;
                    c.BypassedByMeleeWeapon = true;
                    c.Value = ContextValueFactory.Simple(10);
                    c.UsePool = true;
                    c.Pool = ContextValueFactory.Rank();
                })
                .AddOrEditDefaultContextRankConfig(c =>
                {
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.SetMultiplyByModifier(step: 10, max: 100);
                })
                .Configure();
        }

        static void FixLeopardCompanionUpgrade()
        {
            BlueprintObjectConfigurator.From(BlueprintFeatureGuid.AnimalCompanionUpgradeLeopard)
                .RemoveComponentsWhere<AddStatBonus>(b => b.Stat == StatType.Strength)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Constitution, b => b.Value = 2)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Dexterity, b => b.Value = 2)
                .Configure();
        }
    }
}
