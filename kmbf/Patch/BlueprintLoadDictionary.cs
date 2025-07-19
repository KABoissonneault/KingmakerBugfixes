using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Kingdom.Actions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
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

        [HarmonyPostfix]
        public static void BlueprintPatch()
        {
            if (loaded) return;
            loaded = true;

            // Blight Druid Darkness Domain's Moonfire damage scaling
            AddAbilityDamageDiceRankClass(BlueprintAbilityGuid.DarknessDomainGreaterAbility, BlueprintCharacterClassGuid.Druid);

            // Irlene "Relations rank 3" tier 3 gift
            ReplaceArtisan(BlueprintCueGuid.IrleneGiftCue3, BlueprintKingdomArtisanGuid.Woodmaster, BlueprintKingdomArtisanGuid.ShadyTrader);

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

            // Shrewish Gulch Last Stage "Two Actions" and "Three Actions" checks
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageTwoActions, StatType.SkillLoreNature, StatType.SkillAthletics);
            ReplaceCheckSkillType(BlueprintCheckGuid.ShrewishGulchLastStageThreeActions, StatType.SkillLoreNature, StatType.SkillAthletics);

            // Make Darts light weapons (like in tabletop)
            SetWeaponTypeLight(BlueprintWeaponTypeGuid.Dart, light: true);

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            ChangeAddStatBonusScaledDescriptor(BlueprintBuffGuid.MagicalVestmentShield, ModifierDescriptor.Shield, ModifierDescriptor.ShieldEnhancement);
        }
    }
}
