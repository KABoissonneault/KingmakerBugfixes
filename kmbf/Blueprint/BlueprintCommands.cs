using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
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
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using kmbf.Blueprint.Configurator;
using UnityEngine;

namespace kmbf.Blueprint
{
    internal class BlueprintCommands
    {
        public static void AddAbilityDamageDiceRankClass(BlueprintAbilityGuid abilityId, BlueprintCharacterClassGuid characterClassId)
        {
            BlueprintAbilityConfigurator.From(abilityId)
                .EditDamageDiceRankConfig(damageDiceRankConfig =>
                {
                    if (!characterClassId.GetBlueprint(out BlueprintCharacterClass characterClass)) return;

                    if (!damageDiceRankConfig.m_Class.Any(c => c.AssetGuid == characterClass.AssetGuid))
                    {
                        damageDiceRankConfig.m_Class = damageDiceRankConfig.m_Class.AddItem(characterClass).ToArray();
                    }
                })
                .Configure();            
        }

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

        // Replaces the "Apply Buff" action in the "Add Initiator Attack Role Trigger" component
        public static void ReplaceAttackBuff(BlueprintObjectGuid bpId, BlueprintBuffGuid currentBuffId, BlueprintBuffGuid newBuffId)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            ActionList attackActions = null;

            var attackTrigger = bp.Components.FirstOrDefault(c => c is AddInitiatorAttackRollTrigger || c is AddInitiatorAttackWithWeaponTrigger);
            if (attackTrigger == null)
            {
                Main.Log.Error($"Could not find Attack trigger component in blueprint '{bpId.GetDebugName()}'");
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

        public static void ReplaceCheckSkillType(BlueprintCheckGuid checkId, StatType currentStat, StatType newStat)
        {
            if (!checkId.GetBlueprint(out BlueprintCheck check)) return;

            if (check.Type == currentStat)
            {
                check.Type = newStat;
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

        public static void RemoveSpellDescriptor(BlueprintObjectGuid bpId, SpellDescriptor descriptor)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var spellDescriptorComponent = bp.GetComponent<SpellDescriptorComponent>();
            if (spellDescriptorComponent == null)
            {
                Main.Log.Error($"Could not find Spell Descriptor Component on Blueprint {bpId.GetDebugName()}'");
                return;
            }

            spellDescriptorComponent.Descriptor &= ~descriptor;
        }

        public static void SetDisplayName(BlueprintUnitFactGuid factId, LocalizedString displayName)
        {
            if (!factId.GetBlueprint(out BlueprintUnitFact fact)) return;

            fact.m_DisplayName = displayName;
        }

        public static void SetDescription(BlueprintUnitFactGuid factId, LocalizedString description)
        {
            if (!factId.GetBlueprint(out BlueprintUnitFact fact)) return;

            fact.m_Description = description;
        }

        public static void SetTypeName(BlueprintWeaponTypeGuid weaponTypeId, LocalizedString typeName)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_TypeNameText = typeName;
        }

        public static void SetDefaultName(BlueprintWeaponTypeGuid weaponTypeId, LocalizedString defaultName)
        {
            if (!weaponTypeId.GetBlueprint(out BlueprintWeaponType weaponType)) return;

            weaponType.m_DefaultNameText = defaultName;
        }

        public static void CopyComponents(BlueprintObjectGuid sourceId, BlueprintObjectGuid destinationId)
        {
            if (!sourceId.GetBlueprint(out BlueprintScriptableObject source)) return;
            if (!destinationId.GetBlueprint(out BlueprintScriptableObject destination)) return;

            destination.Components = source.Components;
        }

        public static void CopySomeComponents(BlueprintObjectGuid sourceId, BlueprintObjectGuid destinationId, Predicate<BlueprintComponent> predicate)
        {
            if (!sourceId.GetBlueprint(out BlueprintScriptableObject source)) return;
            if (!destinationId.GetBlueprint(out BlueprintScriptableObject destination)) return;

            destination.Components = destination.Components.Where(c => !predicate(c))
                .Concat(source.Components.Where(c => predicate(c))).ToArray();
        }
    }
}
