using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using UnityEngine;

using static kmbf.Blueprint.Builder.ElementBuilder;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch.BP
{
    static class ClassFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Class patches");

            FixAlchemist();
            FixBard();
            FixDruid();
            FixFighter();
            FixKineticist();
            FixDoubleDebilitatingInjury();
            
            // Optional
            FixArcaneTricksterAlignmentRequirement();
        }

        static void FixAlchemist()
        {
            if (StartPatch("Wisdom Cognatogen", ModExclusionFlags.CallOfTheWild))
            {
                BlueprintAbilityConfigurator.From(AbilityRefs.CognatogenWisdom)
                    .EditComponentGameAction<AbilityEffectRunAction, ContextActionApplyBuff>("$ContextActionApplyBuff$2e1602e5-fa74-47db-9bf6-163e65d056d1", b =>
                    {
                        b.DurationValue = ContextDurationFactory.RankTenMinutes();
                    })
                    .Configure();
            }
        }

        static void FixBard()
        {
            if (StartPatch("Inspire Greatness/Heroics Combat End"))
            {
                BlueprintActivatableAbilityConfigurator.From(ActivatableAbilityRefs.InspireGreatness)
                    .SetDeactivateIfCombatEnded(true)
                    .Configure();

                BlueprintActivatableAbilityConfigurator.From(ActivatableAbilityRefs.InspireHeroics)
                    .SetDeactivateIfCombatEnded(true)
                    .Configure();
            }
        }

        static void FixDruid()
        {
            if (!StartPatch("Darkness Domain Moonfire", ModExclusionFlags.CallOfTheWild)) return;

            // Blight Druid Darkness Domain's Moonfire damage scaling
            BlueprintAbilityConfigurator.From(AbilityRefs.DarknessDomainGreaterAbility)
                .AddDamageDiceRankConfigClass(CharacterClassRefs.Druid)
                .Configure();
        }

        // Debilitating Injuries simply do not account for Double Debilitation, and will remove all existing injuries upon applying a new one
        // This fix adds a Condition on the Double Debilitation feature, which if true, will check whether the target has *two* existing buffs
        // before removing them
        static void FixDoubleDebilitatingInjury()
        {
            if (!StartPatch("Rogue Double Debilitation")) return;

            if (!FeatureRefs.DoubleDebilitation.GetBlueprint(out BlueprintFeature DoubleDebilitation)) return;
            if (!BuffRefs.DebilitatingInjuryBewilderedActive.GetBlueprint(out BlueprintBuff BewilderedActive)) return;
            if (!BuffRefs.DebilitatingInjuryDisorientedActive.GetBlueprint(out BlueprintBuff DisorientedActive)) return;
            if (!BuffRefs.DebilitatingInjuryHamperedActive.GetBlueprint(out BlueprintBuff HamperedActive)) return;

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
                , [BuffRefs.DebilitatingInjuryDisorientedActive, BuffRefs.DebilitatingInjuryHamperedActive]
                , [BuffRefs.DebilitatingInjuryDisorientedEffect, BuffRefs.DebilitatingInjuryHamperedEffect]
            );
            Fixup
            (
                DisorientedActive
                , [BuffRefs.DebilitatingInjuryHamperedActive, BuffRefs.DebilitatingInjuryBewilderedActive]
                , [BuffRefs.DebilitatingInjuryHamperedEffect, BuffRefs.DebilitatingInjuryBewilderedEffect]
            );
            Fixup
            (
                HamperedActive
                , [BuffRefs.DebilitatingInjuryDisorientedActive, BuffRefs.DebilitatingInjuryBewilderedActive]
                , [BuffRefs.DebilitatingInjuryDisorientedEffect, BuffRefs.DebilitatingInjuryBewilderedEffect]
            );

            BlueprintBuffConfigurator.From(BuffRefs.DebilitatingInjuryHamperedEffect)
                .SetIcon(HamperedActive.Icon)
                .Configure();
        }

        static void FixKineticist()
        {
            if (!StartPatch("Kineticist Deadly Earth")) return;

            // Deadly Earth: Metal (and Rare variant) has scaling that does not match other compound elements or other Metal abilities
            // Copy the ContextRankConfigs from the Mud version
            BlueprintAbilityConfigurator.From(AbilityRefs.DeadlyEarthMudBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(AbilityRefs.DeadlyEarthEarthBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(AbilityRefs.DeadlyEarthMagmaBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();
            BlueprintAbilityConfigurator.From(AbilityRefs.DeadlyEarthMetalBlast).AddSpellDescriptor(SpellDescriptor.Ground).Configure();

            BlueprintAbilityAreaEffectConfigurator.From(AbilityAreaEffectRefs.DeadlyEarthMetalBlast)
                .ReplaceComponentsWithSource<ContextRankConfig>(AbilityAreaEffectRefs.DeadlyEarthMudBlast)
                .Configure();

            BlueprintAbilityAreaEffectConfigurator.From(AbilityAreaEffectRefs.DeadlyEarthRareMetalBlast)
                .ReplaceComponentsWithSource<ContextRankConfig>(AbilityAreaEffectRefs.DeadlyEarthMudBlast)
                .Configure();
        }

        // Both tabletop and in-game encyclopedia say Arcane Trickster requirement non-lawful, but the game (or WotR) does not enforce it
        static void FixArcaneTricksterAlignmentRequirement()
        {
            if (!StartBalancePatch("Arcane Trickster Alignment", nameof(BalanceSettings.FixArcaneTricksterAlignmentRequirement))) return;

            BlueprintCharacterClassConfigurator.From(CharacterClassRefs.ArcaneTrickster)
                .SetAlignmentRestriction(AlignmentMaskType.NeutralGood | AlignmentMaskType.TrueNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.Chaotic)
                .Configure();
        }

        // KM is missing Weapon Training for Dart, Javelin, Kama, Nunchaku, Sai, Sling Staff, Throwing Axe, and Sling
        // Move them around
        static void FixFighter()
        {
            if (StartPatch("Fighter Weapon Training"))
            {
                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Dart)
                    .SetFighterGroup(WeaponFighterGroup.BladesLight)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Javelin)
                    .SetFighterGroup(WeaponFighterGroup.Spears)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Kama)
                    .SetFighterGroup(WeaponFighterGroup.BladesLight)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Nunchaku)
                    .SetFighterGroup(WeaponFighterGroup.Close)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Sai)
                    .SetFighterGroup(WeaponFighterGroup.BladesLight)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Sling)
                    .SetFighterGroup(WeaponFighterGroup.Hammers)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.SlingStaff)
                    .SetFighterGroup(WeaponFighterGroup.Hammers)
                    .Configure();

                BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.ThrowingAxe)
                    .SetFighterGroup(WeaponFighterGroup.Axes)
                    .Configure();
            }

            if(StartPatch("Penetrating Strike"))
            {
                BlueprintFeatureConfigurator.From(FeatureRefs.PenetratingStrike)
                    .ReplaceComponents<Kingmaker.Designers.Mechanics.Facts.PenetratingStrike, kmbf.Component.PenetratingStrike>((prev, next) =>
                    {
                        next.ReductionPenalty = prev.ReductionReduction;
                    })
                    .Configure();

                BlueprintFeatureConfigurator.From(FeatureRefs.GreaterPenetratingStrike)
                    .ReplaceComponents<Kingmaker.Designers.Mechanics.Facts.PenetratingStrike, kmbf.Component.PenetratingStrike>((prev, next) =>
                    {
                        next.ReductionPenalty = prev.ReductionReduction;
                    })
                    .Configure();
            }
        }
    }
}
