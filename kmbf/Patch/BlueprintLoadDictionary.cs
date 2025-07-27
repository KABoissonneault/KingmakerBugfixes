using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Action;
using kmbf.Blueprint;
using UnityEngine;

namespace kmbf.Patch
{
    using static kmbf.Blueprint.BlueprintCommands;

    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;

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

            #endregion

            #region Spell

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            ChangeAddStatBonusScaledDescriptor(BlueprintBuffGuid.MagicalVestmentShield, ModifierDescriptor.Shield, ModifierDescriptor.ShieldEnhancement);

            FixRaiseDead();

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

            // Bane of the Living "Not Undead or Not Construct" instead of "Not Undead and Not Construct"
            FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.BaneLiving);

            // Nature's Wrath trident "Outsider AND Aberration ..." instead of OR
            // Fix "Electricity Vulnerability" debuff to apply to target instead of initiator
            FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.NaturesWrath);
            ReplaceWeaponBuffOnAttackToWeaponTrigger(BlueprintWeaponEnchantmentGuid.NaturesWrath);

            // Scroll of Summon Nature's Ally V (Single) would Summon Monster V (Single) instead
            ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid.ScrollSummonNaturesAllyVSingle, BlueprintAbilityGuid.SummonMonsterVSingle, BlueprintAbilityGuid.SummonNaturesAllyVSingle);

            #endregion

            #region Kingdom

            // Irlene "Relations rank 3" tier 3 gift
            ReplaceArtisan(BlueprintCueGuid.IrleneGiftCue3, BlueprintKingdomArtisanGuid.Woodmaster, BlueprintKingdomArtisanGuid.ShadyTrader);

            #endregion

            #region Script

            // Shrewish Gulch Last Stage "Two Actions" and "Three Actions" checks
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageTwoActions, StatType.SkillLoreNature, StatType.SkillAthletics);
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageThreeActions, StatType.SkillLoreNature, StatType.SkillAthletics);

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
            if (!BlueprintBuffGuid.DebilitatingInjuryBewilderedEffect.GetBlueprint(out BlueprintBuff BewilderedEffect)) return;
            if (!BlueprintBuffGuid.DebilitatingInjuryDisorientedEffect.GetBlueprint(out BlueprintBuff DisorientedEffect)) return;
            if (!BlueprintBuffGuid.DebilitatingInjuryHamperedEffect.GetBlueprint(out BlueprintBuff HamperedEffect)) return;

            Conditional MakeConditional(BlueprintBuff[] otherActives, BlueprintBuff[] otherBuffs, Conditional[] normalConditionals)
            {
                var featureCondition = ScriptableObject.CreateInstance<ContextConditionCasterHasFact>();
                featureCondition.Fact = DoubleDebilitation;

                // If the target has at least two of the "other injuries" from the caster, remove all the existing ones
                var doubleDebilitationAction = ConditionalConfigurator.New(
                    ConditionsCheckerFactory.Single(
                        ContextConditionHasBuffsFromCasterConfigurator.New("Debilitating Injury", otherBuffs, 2).Configure()
                    ),
                    ifTrue: ActionListFactory.From(
                        ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator.New(buff: otherBuffs[0], active: otherActives[0]).Configure()
                        , ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator.New(buff: otherBuffs[1], active: otherActives[1]).Configure()
                    )
                ).Configure();

                // If user has Double Debilitation, use the new "Remove if two buffs". Else, use the current "Remove if any buff"

                return ConditionalConfigurator.New(
                    ConditionsCheckerFactory.Single(featureCondition)
                    , ifTrue: ActionListFactory.From(doubleDebilitationAction)
                    , ifFalse: ActionListFactory.From(normalConditionals)
                    ).Configure();
            }

            void Fixup(BlueprintBuff active, BlueprintBuff[] otherActives, BlueprintBuff[] otherBuffs)
            {
                var triggerComponent = active.GetComponent<AddInitiatorAttackRollTrigger>();
                ActionList triggerActions = triggerComponent.Action;
                Conditional[] conditionals = triggerActions.Actions.OfType<Conditional>().ToArray();

                triggerActions.Actions = triggerActions.Actions.Where(a => !(a is Conditional))
                    .AddItem(MakeConditional(otherActives, otherBuffs, conditionals))
                    .ToArray();
            }

            Fixup(BewilderedActive, [DisorientedActive, HamperedActive], [DisorientedEffect, HamperedEffect]);
            Fixup(DisorientedActive, [HamperedActive, BewilderedActive], [HamperedEffect, BewilderedEffect]);
            Fixup(HamperedActive, [DisorientedActive, BewilderedActive], [DisorientedEffect, BewilderedEffect]);

            BlueprintBuffConfigurator.From(HamperedEffect)
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
                Main.Log.Error($"Could not find Ability Effect Run Action in Blueprint {raiseDead.GetDebugName()}");
                return;
            }

            var difficultyCheck = ContextConditionDifficultyHigherThanConfigurator
                .New(BlueprintRoot.Instance.DifficultyList.CoreDifficulty)
                .SetCheckOnlyForMonsterCaster(false)
                .Configure();

            var dealDamageAction = ContextActionDealDamageConfigurator
                .NewEnergyDrain(EnergyDrainType.Permanent, ContextDiceFactory.BonusConstant(2))
                .Configure();

            Conditional difficultyConditional = ConditionalConfigurator.New(
                new ConditionsChecker() { Conditions = [difficultyCheck] }
                , ifTrue: new ActionList() { Actions = [dealDamageAction] })
                .Configure();

            // Put the drain first, resurrection makes the unit untargetable
            runAction.Actions.Actions = [difficultyConditional, .. runAction.Actions.Actions];
        }
    }
}
