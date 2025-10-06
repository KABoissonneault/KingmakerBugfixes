using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using UnityEngine;

using static kmbf.Blueprint.Builder.ElementBuilder;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch
{
    static class ClassFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Class patches");

            FixDruid();
            FixKineticist();
            FixDoubleDebilitatingInjury();
            FixFighterWeaponTraining();

            // Optional
            FixArcaneTricksterAlignmentRequirement();
        }

        static void FixDruid()
        {
            if (!StartPatch("Darkness Domain Moonfire", ModExclusionFlags.CallOfTheWild)) return;

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
            if (!StartPatch("Rogue Double Debilitation")) return;

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
            if (!StartPatch("Kineticist Deadly Earth")) return;

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

        // Both tabletop and in-game encyclopedia say Arcane Trickster requirement non-lawful, but the game (or WotR) does not enforce it
        static void FixArcaneTricksterAlignmentRequirement()
        {
            if (!StartBalancePatch("Arcane Trickster Alignment", nameof(BalanceSettings.FixArcaneTricksterAlignmentRequirement))) return;

            BlueprintCharacterClassConfigurator.From(BlueprintCharacterClassGuid.ArcaneTrickster)
                .SetAlignmentRestriction(AlignmentMaskType.NeutralGood | AlignmentMaskType.TrueNeutral | AlignmentMaskType.NeutralEvil | AlignmentMaskType.Chaotic)
                .Configure();
        }

        // KM is missing Weapon Training for Dart, Javelin, Kama, Nunchaku, Sai, Sling Staff, Throwing Axe, and Sling
        // Move them around
        static void FixFighterWeaponTraining()
        {
            if (!StartPatch("Fighter Weapon Training")) return;

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Dart)
                .SetFighterGroup(WeaponFighterGroup.BladesLight)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Javelin)
                .SetFighterGroup(WeaponFighterGroup.Spears)
                .Configure();
            
            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Kama)
                .SetFighterGroup(WeaponFighterGroup.BladesLight)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Nunchaku)
                .SetFighterGroup(WeaponFighterGroup.Close)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Sai)
                .SetFighterGroup(WeaponFighterGroup.BladesLight)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.Sling)
                .SetFighterGroup(WeaponFighterGroup.Hammers)
                .Configure();

            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.SlingStaff)
                .SetFighterGroup(WeaponFighterGroup.Hammers)
                .Configure();            
            
            BlueprintWeaponTypeConfigurator.From(BlueprintWeaponTypeGuid.ThrowingAxe)
                .SetFighterGroup(WeaponFighterGroup.Axes)
                .Configure();
        }
    }
}
