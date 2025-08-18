using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Action;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using kmbf.Component;
using UnityEngine;

using static kmbf.Blueprint.Builder.ElementBuilder;

namespace kmbf.Patch
{
    static class AbilitiesFixes
    {
        public static void Apply()
        {
            FixDruid();
            FixKineticist();
            FixDoubleDebilitatingInjury();
            FixEkunWolfBuffs();
            FixMagicalVestmentShield();
            FixRaiseDead();
            FixBreathOfLife();
            FixJoyfulRapture();
            FixProtectionFromArrows();
            FixLeopardCompanionUpgrade();
            FixGazeImmunities();
            FixTieflingFoulspawn();
            FixExplosionRing();
        }

        static void FixDruid()
        {
            // Blight Druid Darkness Domain's Moonfire damage scaling
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DarknessDomainGreaterAbility)
                .AddDamageDiceRankConfigClass(BlueprintCharacterClassGuid.Druid)
                .Configure();
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

        static void FixKineticist()
        {
            // Deadly Earth: Metal (and Rare variant) has scaling that does not match other compound elements or other Metal abilities
            // Copy the ContextRankConfigs from the Mud version
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMudBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthEarthBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMagmaBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.DeadlyEarthMetalBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();

            BlueprintAbilityAreaEffectConfigurator.From(BlueprintAbilityAreaEffectGuid.DeadlyEarthMetalBlast)
                .ReplaceComponentsWithSource<ContextRankConfig>(BlueprintAbilityAreaEffectGuid.DeadlyEarthMudBlast)
                .Configure();

            BlueprintAbilityAreaEffectConfigurator.From(BlueprintAbilityAreaEffectGuid.DeadlyEarthRareMetalBlast)
                .ReplaceComponentsWithSource<ContextRankConfig>(BlueprintAbilityAreaEffectGuid.DeadlyEarthMudBlast)
                .Configure();
        }

        static void FixEkunWolfBuffs()
        {
            // The "Master" features are accidentally added to Dog by the dialogue. Might as well put the stat bonuses directly on it
            // Plus the Offensive Master feature was accidentally giving the Defensive buff
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.EkunWolfOffensiveMaster)
                .ReplaceAllComponentsWithSource(BlueprintFeatureGuid.EkunWolfOffensiveBuff)
                .Configure();

            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.EkunWolfDefensiveMaster)
                .ReplaceAllComponentsWithSource(BlueprintFeatureGuid.EkunWolfDefensiveBuff)
                .Configure();
        }

        static void FixMagicalVestmentShield()
        {
            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            BlueprintBuffConfigurator.From(BlueprintBuffGuid.MagicalVestmentShield)
                .EditComponent<AddStatBonusScaled>(c =>
                {
                    c.Descriptor = ModifierDescriptor.ShieldEnhancement;
                })
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

        // Description says +2 Dex, +2 Con, but the game applies +4 str, -2 dex, and +4 con
        static void FixLeopardCompanionUpgrade()
        {
            BlueprintObjectConfigurator.From(BlueprintFeatureGuid.AnimalCompanionUpgradeLeopard)
                .RemoveComponentsWhere<AddStatBonus>(b => b.Stat == StatType.Strength)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Constitution, b => b.Value = 2)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Dexterity, b => b.Value = 2)
                .Configure();
        }

        // Baleful Gaze applies stat damage, which does not check descriptor immunities like "Sight Based" or "Gaze Attack"
        // Add a conditional wrapper that checks for the immunity for now
        // Still does not cover Saving Throw bonuses and the like, but we'll see if anyone notices
        static void FixGazeImmunities()
        {
            BlueprintAbilityAreaEffectConfigurator.From(BlueprintAbilityAreaEffectGuid.BalefulGaze)
                .EditRoundActions(roundActions =>
                {
                    foreach(GameAction action in roundActions.GetGameActionsRecursive())
                    {
                        // Skip the own wrappers we're adding
                        if (action is Conditional cond && cond.ConditionsChecker.HasConditions && cond.ConditionsChecker.Conditions[0] is ContextConditionHasSpellImmunityToContextDescriptors)
                            continue;

                        foreach(ActionList childList in action.GetChildActionLists())
                        {
                            for(int index = 0; index < childList.Actions.Length; ++index)
                            {
                                var childAction = childList.Actions[index];
                                if(childAction is ContextActionSavingThrow)
                                {
                                    var conditional = MakeGameActionConditional(
                                        ConditionsCheckerFactory.Single(MakeContextConditionHasSpellImmunityToContextDescriptors())
                                        , ifFalse: ActionListFactory.Single(childAction)
                                        );

                                    childList.Actions[index] = conditional;
                                }
                            }
                        }
                    }
                })
                .AddSpellDescriptor(SpellDescriptor.SightBased)
                .Configure();
        }

        // Foulspawn tieflings are supposed to have a bonus against Cleric, Paladins, and Inquisitors
        // It checks if the target has all three classes instead of any
        static void FixTieflingFoulspawn()
        {
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.TieflingHeritageFoulspawn)
                .EditComponent<AttackBonusConditional>(c =>
                {
                    c.Conditions.Operation = Operation.Or;
                })
                .Configure();
        }
        
        // The +12 damage only applies to Bomb weapons
        // Since Bombs are virtually always abilities, we need a special component
        // The +12 damage relies on the Bomb descriptor on the ability, so we also add it to abilities that are missing it (cross-referencing WotR to make sure the descriptor is intended)
        static void FixExplosionRing()
        {
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.ExplosionRing)
                .AddComponent<AdditionalBonusOnDamage>(c =>
                {
                    c.BonusOnDamage = 12;
                    c.CheckSpellDescriptor = true;
                    c.SpellDescriptorsList = SpellDescriptor.Bomb;
                })
                .Configure();

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.AlchemistFire)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.AcidFlask)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();
        }
    }
}
