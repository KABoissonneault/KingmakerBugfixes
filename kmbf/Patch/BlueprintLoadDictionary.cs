using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Kingdom.Actions;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.Utility;
using kmbf.Action;
using UnityEngine;

namespace kmbf.Patch
{
    using static BlueprintExtensions;

    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;

        static void AddAbilityDamageDiceRankClass(BlueprintAbilityGuid abilityId, BlueprintCharacterClassGuid characterClassId)
        {
            if (!abilityId.GetBlueprint(out BlueprintAbility ability)) return;
            if (!ability.GetDamageRankConfig(out ContextRankConfig damageRankConfig)) return;
            if (!characterClassId.GetBlueprint(out BlueprintCharacterClass characterClass)) return;

            if (!damageRankConfig.m_Class.Any(c => c.AssetGuid == characterClass.AssetGuid))
            {
                damageRankConfig.m_Class = damageRankConfig.m_Class.AddItem(characterClass).ToArray();
            }
        }

        static void ReplaceArtisan(BlueprintCueGuid cueId, BlueprintKingdomArtisanGuid currentArtisan, BlueprintKingdomArtisanGuid newArtisan)
        {
            if (!cueId.GetBlueprint(out BlueprintCue cue)) return;

            foreach (GameAction action in cue.OnShow.GetGameActionsRecursive()
                .Concat(cue.OnStop.GetGameActionsRecursive()))
            {
                var artisanGift = action as KingdomActionGetArtisanGift;
                if (artisanGift != null)
                {
                    if (artisanGift.Artisan != null && artisanGift.Artisan.AssetGuid == currentArtisan.guid)
                    {
                        newArtisan.GetBlueprint(out artisanGift.Artisan);
                    }
                    continue;
                }

                var artisanGiftWithTier = action as KingdomActionGetArtisanGiftWithCertainTier;
                if (artisanGiftWithTier != null)
                {
                    if (artisanGiftWithTier.Artisan != null && artisanGiftWithTier.Artisan.AssetGuid == currentArtisan.guid)
                    {
                        newArtisan.GetBlueprint(out artisanGiftWithTier.Artisan);
                    }
                    continue;
                }
            }
        }

        static void ReplaceAttackRollTriggerToWeaponTrigger(BlueprintObjectGuid bpId, bool WaitForAttackResolve)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            for (int i = 0; i < bp.Components.Length; ++i)
            {
                var component = bp.Components[i];
                if (!(component is AddInitiatorAttackRollTrigger)) continue;

                var attackRollTrigger = component as AddInitiatorAttackRollTrigger;

                var weaponTrigger = ScriptableObject.CreateInstance<AddInitiatorAttackWithWeaponTrigger>();
                weaponTrigger.name = attackRollTrigger.name;
                weaponTrigger.OnlyHit = attackRollTrigger.OnlyHit;
                weaponTrigger.CriticalHit = attackRollTrigger.CriticalHit;
                weaponTrigger.OnlySneakAttack = attackRollTrigger.SneakAttack;
                weaponTrigger.CheckWeaponCategory = attackRollTrigger.CheckWeapon;
                weaponTrigger.Category = attackRollTrigger.WeaponCategory;
                weaponTrigger.Action = attackRollTrigger.Action;

                weaponTrigger.WaitForAttackResolve = WaitForAttackResolve;

                bp.Components[i] = weaponTrigger;
            }
        }

        // "Buff on Attack" applies to instigator, "Weapon Trigger" applies to target
        static void ReplaceWeaponBuffOnAttackToWeaponTrigger(BlueprintObjectGuid bpId)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            for (int i = 0; i < bp.Components.Length; ++i)
            {
                var component = bp.Components[i];
                if (!(component is WeaponBuffOnAttack)) continue;

                var buffOnAttack = (WeaponBuffOnAttack)component;

                var applyBuff = ScriptableObject.CreateInstance<ContextActionApplyBuff>();
                applyBuff.Buff = buffOnAttack.Buff;
                applyBuff.UseDurationSeconds = true;
                applyBuff.DurationSeconds = (float)buffOnAttack.Duration.Seconds.TotalSeconds;

                var weaponTrigger = ScriptableObject.CreateInstance<AddInitiatorAttackWithWeaponTrigger>();
                weaponTrigger.OnlyHit = true;
                weaponTrigger.Action = new ActionList();
                weaponTrigger.Action.Actions = new GameAction[] { applyBuff };

                bp.Components[i] = weaponTrigger;
            }
        }

        // Replaces the "Apply Buff" action in the "Add Initiator Attack Role Trigger" component
        static void ReplaceAttackBuff(BlueprintObjectGuid bpId, BlueprintBuffGuid currentBuffId, BlueprintBuffGuid newBuffId)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            ActionList attackActions = null;

            var attackTrigger = bp.Components.FirstOrDefault(c => c is AddInitiatorAttackRollTrigger || c is AddInitiatorAttackWithWeaponTrigger);
            if (attackTrigger == null)
            {
                Main.Log.Error($"Could not find Attack trigger component in blueprint '{bp.GetDebugName()}'");
                return;
            }

            if (attackTrigger is AddInitiatorAttackRollTrigger)
            {
                attackActions = ((AddInitiatorAttackRollTrigger)attackTrigger).Action;
            }
            else if (attackTrigger is AddInitiatorAttackWithWeaponTrigger)
            {
                attackActions = ((AddInitiatorAttackWithWeaponTrigger)attackTrigger).Action;
            }

            if (!newBuffId.GetBlueprint(out BlueprintBuff newBuff)) return;


            attackActions.GetGameActionsRecursive()
                .Select(a => a as ContextActionApplyBuff)
                .NotNull()
                .Where(a => a.Buff.AssetGuid == currentBuffId.guid)
                .ForEach(a => a.Buff = newBuff);
        }

        static void SetContextSetAbilityParamsDC(BlueprintObjectGuid bpId, int DC)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var abilityParams = (ContextSetAbilityParams)bp.Components.FirstOrDefault(c => c is ContextSetAbilityParams);
            if (abilityParams == null)
            {
                abilityParams = ScriptableObject.CreateInstance<ContextSetAbilityParams>();
                bp.Components = bp.Components.AddToArray(abilityParams);
            }

            abilityParams.DC = 16;
        }

        // Flips AND and OR in Weapon Enhancement / Weapon Damage components
        // Easy to get them mixed up for negative checks (Not Undead or Not Construct)
        static void FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
        {
            if (!weaponEnchantmentId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            foreach (var comp in bp.Components)
            {
                ConditionsChecker checker = null;
                if (comp is WeaponConditionalEnhancementBonus)
                {
                    checker = ((WeaponConditionalEnhancementBonus)comp).Conditions;
                }
                else if (comp is WeaponConditionalDamageDice)
                {
                    checker = ((WeaponConditionalDamageDice)comp).Conditions;
                }

                if (checker == null) continue;

                if (checker.Operation == Operation.And)
                {
                    checker.Operation = Operation.Or;
                }
                else if (checker.Operation == Operation.Or)
                {
                    checker.Operation = Operation.And;
                }
            }
        }

        static void ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid itemEquipmentUsableId, BlueprintAbilityGuid currentAbilityId, BlueprintAbilityGuid newAbilityId)
        {
            if (!itemEquipmentUsableId.GetBlueprint(out BlueprintItemEquipmentUsable itemEquipmentUsable)) return;
            if (!newAbilityId.GetBlueprint(out BlueprintAbility newAbility)) return;

            if (itemEquipmentUsable.Ability != null && itemEquipmentUsable.Ability.AssetGuid == currentAbilityId.guid)
            {
                itemEquipmentUsable.Ability = newAbility;
            }
        }

        static void ReplaceCheckSkillType(BlueprintCheckGuid checkId, StatType currentStat, StatType newStat)
        {
            if (!checkId.GetBlueprint(out BlueprintCheck check)) return;

            if (check.Type == currentStat)
            {
                check.Type = newStat;
            }
        }

        static void AddWeaponEnchantment(BlueprintItemWeaponGuid weaponId, BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
        {
            if (!weaponId.GetBlueprint(out BlueprintItemWeapon weapon)) return;
            if (!weaponEnchantmentId.GetBlueprint(out BlueprintWeaponEnchantment enchantment)) return;

            weapon.Enchantments.Add(enchantment);
        }

        static void SetWeaponTypeLight(BlueprintWeaponTypeGuid weaponTypeId, bool light)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_IsLight = light;
        }

        static void ChangeAddStatBonusScaledDescriptor(BlueprintObjectGuid bpId, ModifierDescriptor expectedDescriptor, ModifierDescriptor newDescriptor)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var statBonusComponent = bp.Components.FirstOrDefault(c => c is AddStatBonusScaled) as AddStatBonusScaled;
            if (statBonusComponent.Descriptor == expectedDescriptor)
            {
                statBonusComponent.Descriptor = newDescriptor;
            }
        }

        static void RemoveSpellDescriptor(BlueprintObjectGuid bpId, SpellDescriptor descriptor)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var spellDescriptorComponent = bp.GetComponent<SpellDescriptorComponent>();
            if(spellDescriptorComponent == null)
            {
                Main.Log.Error($"Could not find Spell Descriptor Component on Blueprint {bp.GetDebugName()}'");
                return;
            }

            spellDescriptorComponent.Descriptor &= ~descriptor;
        }

        static void AddEventResultBuff(BlueprintObjectGuid bpId, BlueprintBuffGuid buffId)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var finalResults = bp.GetComponent<EventFinalResults>();
            if (finalResults == null)
            {
                Main.Log.Error($"Could not find Event Final Results Component on Blueprint {bp.GetDebugName()}'");
                return;
            }
        }

        static void SetDisplayName(BlueprintUnitFactGuid factId, LocalizedString displayName)
        {
            if (!factId.GetBlueprint(out BlueprintUnitFact fact)) return;

            fact.m_DisplayName = displayName;
        }

        static void SetDescription(BlueprintUnitFactGuid factId, LocalizedString description)
        {
            if (!factId.GetBlueprint(out BlueprintUnitFact fact)) return;

            fact.m_Description = description;
        }

        static void SetTypeName(BlueprintWeaponTypeGuid weaponTypeId, LocalizedString typeName)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_TypeNameText = typeName;
        }

        static void SetDefaultName(BlueprintWeaponTypeGuid weaponTypeId, LocalizedString defaultName)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_DefaultNameText = defaultName;
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
            
            Conditional MakeConditional(BlueprintBuff[] otherBuffs, Conditional[] normalConditionals)
            {
                var featureCondition = ScriptableObject.CreateInstance<ContextConditionCasterHasFact>();
                featureCondition.Fact = DoubleDebilitation;

                // If the target has at least two of the "other injuries" from the caster, remove all the existing ones
                var doubleDebilitationBuffsCondition = ScriptableObject.CreateInstance<ContextConditionHasBuffsFromCaster>();
                doubleDebilitationBuffsCondition.Buffs = otherBuffs;
                doubleDebilitationBuffsCondition.Count = 2;
                doubleDebilitationBuffsCondition.CaptionName = "Debilitating Injury";

                var doubleDebilitationAction = ScriptableObject.CreateInstance<Conditional>();
                doubleDebilitationAction.ConditionsChecker = new ConditionsChecker() { Conditions = [doubleDebilitationBuffsCondition] };
                // Take the ContextActionRemoveBuff actions from the normal conditionals (removes the other buffs)
                doubleDebilitationAction.IfTrue = new ActionList() { Actions = [.. normalConditionals.Select(c => c.IfTrue.Actions.First(a => a is ContextActionRemoveBuff))] }; 

                // If user has Double Debilitation, use the new "Remove if two buffs". Else, use the current "Remove if any buff"
                var featureConditional = ScriptableObject.CreateInstance<Conditional>();
                featureConditional.ConditionsChecker = new ConditionsChecker() { Conditions = [featureCondition] };
                featureConditional.IfTrue = new ActionList() { Actions = [doubleDebilitationAction] };
                featureConditional.IfFalse = new ActionList() { Actions = normalConditionals };

                return featureConditional;
            }

            void Fixup(BlueprintBuff active, BlueprintBuff[] otherBuffs)
            {
                var triggerComponent = active.GetComponent<AddInitiatorAttackRollTrigger>();
                ActionList triggerActions = triggerComponent.Action;
                Conditional[] conditionals = triggerActions.Actions.OfType<Conditional>().ToArray();

                triggerActions.Actions = triggerActions.Actions.Where(a => !(a is Conditional))
                    .AddItem(MakeConditional(otherBuffs, conditionals))
                    .ToArray();
            }

            Fixup(BewilderedActive, [DisorientedEffect, HamperedEffect]);
            Fixup(DisorientedActive, [HamperedEffect, BewilderedEffect]);
            Fixup(HamperedActive, [DisorientedEffect, BewilderedEffect]);
        }

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

            #endregion

            #region Spell

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            ChangeAddStatBonusScaledDescriptor(BlueprintBuffGuid.MagicalVestmentShield, ModifierDescriptor.Shield, ModifierDescriptor.ShieldEnhancement);

            #endregion

            #region Buff

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
    }
}
