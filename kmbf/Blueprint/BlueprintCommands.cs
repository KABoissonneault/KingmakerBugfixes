using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.Kingdom.Actions;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using UnityEngine;

namespace kmbf.Blueprint
{
    internal class BlueprintCommands
    {
        public static void ReplaceArtisan(BlueprintCueGuid cueId, BlueprintKingdomArtisanGuid currentArtisan, BlueprintKingdomArtisanGuid newArtisan)
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

        public static void ReplaceAttackRollTriggerToWeaponTrigger(BlueprintObjectGuid bpId, bool WaitForAttackResolve)
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
        public static void ReplaceWeaponBuffOnAttackToWeaponTrigger(BlueprintObjectGuid bpId)
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
                weaponTrigger.Action.Actions = [applyBuff];

                bp.Components[i] = weaponTrigger;
            }
        }

        public static void SetContextSetAbilityParamsDC(BlueprintObjectGuid bpId, int DC)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var abilityParams = (ContextSetAbilityParams)bp.Components.FirstOrDefault(c => c is ContextSetAbilityParams);
            if (abilityParams == null)
            {
                abilityParams = ScriptableObject.CreateInstance<ContextSetAbilityParams>();
                bp.Components = bp.Components.AddToArray(abilityParams);
            }

            abilityParams.DC = DC;
        }

        // Flips AND and OR in Weapon Enhancement / Weapon Damage components
        // Easy to get them mixed up for negative checks (Not Undead or Not Construct)
        public static void FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
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

        public static void ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid itemEquipmentUsableId, BlueprintAbilityGuid currentAbilityId, BlueprintAbilityGuid newAbilityId)
        {
            if (!itemEquipmentUsableId.GetBlueprint(out BlueprintItemEquipmentUsable itemEquipmentUsable)) return;
            if (!newAbilityId.GetBlueprint(out BlueprintAbility newAbility)) return;

            if (itemEquipmentUsable.Ability != null && itemEquipmentUsable.Ability.AssetGuid == currentAbilityId.guid)
            {
                itemEquipmentUsable.Ability = newAbility;
            }
        }

        public static void AddWeaponEnchantment(BlueprintItemWeaponGuid weaponId, BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
        {
            if (!weaponId.GetBlueprint(out BlueprintItemWeapon weapon)) return;
            if (!weaponEnchantmentId.GetBlueprint(out BlueprintWeaponEnchantment enchantment)) return;

            weapon.Enchantments.Add(enchantment);
        }

        public static void SetWeaponTypeLight(BlueprintWeaponTypeGuid weaponTypeId, bool light)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_IsLight = light;
        }

        public static void ChangeAddStatBonusScaledDescriptor(BlueprintObjectGuid bpId, ModifierDescriptor expectedDescriptor, ModifierDescriptor newDescriptor)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            foreach (var statBonusComponent in bp.Components.OfType<AddStatBonusScaled>().Where(c => c.Descriptor == expectedDescriptor))
                statBonusComponent.Descriptor = newDescriptor;
        }
    }
}
