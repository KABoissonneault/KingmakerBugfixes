using HarmonyLib;
using System.Reflection;
using UnityModManagerNet;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom.Actions;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Enums;

namespace kmbf;

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        Log = modEntry.Logger;

        try {
            HarmonyInstance = new Harmony(modEntry.Info.Id);
        } catch {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    static string GetDebugName(BlueprintScriptableObject bp)
    {
        return $"Blueprint:{bp.AssetGuid}:{bp.name}";
    }

    static string GetDebugName(BlueprintAbility ability)
    {
        return $"BlueprintAbility:{ability.AssetGuid}:{ability.name}";
    }

    static bool GetDamageRankConfig(BlueprintAbility ability, out ContextRankConfig damageRankConfig)
    {
        damageRankConfig = ability.ComponentsArray
            .Select(c => c as ContextRankConfig)
            .Where(c => c != null && c.Type == Kingmaker.Enums.AbilityRankType.DamageDice)
            .First();

        if (damageRankConfig == null)
        {
            Log.Error($"Could not find damage dice rank config in ability blueprint '{GetDebugName(ability)}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintObjectGuid weaponTypeId, out BlueprintWeaponType weaponType)
    {
        weaponType = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponType>(weaponTypeId.guid);
        if (weaponType == null)
        {
            Log.Error($"Could not find Weapon Type blueprint with GUID '{weaponTypeId.guid}'");
            return false;
        }

        return true;
    }

    static IEnumerable<GameAction> GetGameActionsRecursive(ActionList actionList)
    {
        foreach(GameAction gameAction in actionList.Actions.EmptyIfNull())
        {
            yield return gameAction;

            if(gameAction is Conditional)
            {
                var conditionalAction = (Conditional)gameAction;
                foreach (GameAction trueAction in GetGameActionsRecursive(conditionalAction.IfTrue))
                {
                    yield return trueAction;
                }

                foreach (GameAction falseAction in GetGameActionsRecursive(conditionalAction.IfFalse))
                {
                    yield return falseAction;
                }
            }
            else if (gameAction is ContextActionSavingThrow)
            {
                var savingThrowAction = (ContextActionSavingThrow)gameAction;
                foreach (GameAction action in GetGameActionsRecursive(savingThrowAction.Actions))
                {
                    yield return action;
                }
            }
            else if (gameAction is ContextActionConditionalSaved)
            {
                var conditionalAction = (ContextActionConditionalSaved)gameAction;
                foreach (GameAction trueAction in GetGameActionsRecursive(conditionalAction.Succeed))
                {
                    yield return trueAction;
                }

                foreach (GameAction falseAction in GetGameActionsRecursive(conditionalAction.Failed))
                {
                    yield return falseAction;
                }
            }
        }
    }

    [HarmonyPatch(typeof(LibraryScriptableObject), "LoadDictionary")]
    static class LibraryScriptableObject_LoadDictionary_Patch
    {
        static bool loaded = false;

        static void AddAbilityDamageDiceRankClass(BlueprintAbilityGuid abilityId, BlueprintCharacterClassGuid characterClassId)
        {
            if(!abilityId.GetBlueprint(out BlueprintAbility ability)) return;           
            if(!GetDamageRankConfig(ability, out ContextRankConfig damageRankConfig)) return;
            if(!characterClassId.GetBlueprint(out BlueprintCharacterClass characterClass)) return;

            if (!damageRankConfig.m_Class.Any(c => c.AssetGuid == characterClass.AssetGuid))
            {
                damageRankConfig.m_Class = damageRankConfig.m_Class.AddItem(characterClass).ToArray();
            }
        }

        static void ReplaceArtisan(BlueprintCueGuid cueId, BlueprintKingdomArtisanGuid currentArtisan, BlueprintKingdomArtisanGuid newArtisan)
        {
            if (!cueId.GetBlueprint(out BlueprintCue cue)) return;

            foreach(GameAction action in GetGameActionsRecursive(cue.OnShow)
                .Concat(GetGameActionsRecursive(cue.OnStop)))
            {
                var artisanGift = action as KingdomActionGetArtisanGift;
                if(artisanGift != null)
                {
                    if(artisanGift.Artisan != null && artisanGift.Artisan.AssetGuid == currentArtisan.guid)
                    {
                        newArtisan.GetBlueprint(out artisanGift.Artisan);
                    }
                    continue;
                }

                var artisanGiftWithTier = action as KingdomActionGetArtisanGiftWithCertainTier;
                if(artisanGiftWithTier != null)
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

            for(int i = 0; i < bp.Components.Length; ++i)
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

                var buffOnAttack = component as WeaponBuffOnAttack;

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
            if(attackTrigger == null)
            {
                Log.Error($"Could not find Attack trigger component in blueprint '{GetDebugName(bp)}'");
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
            

            GetGameActionsRecursive(attackActions)
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

            foreach(var comp in bp.Components)
            {
                ConditionsChecker checker = null;
                if(comp is WeaponConditionalEnhancementBonus)
                {
                    checker = ((WeaponConditionalEnhancementBonus)comp).Conditions;
                }
                else if(comp is WeaponConditionalDamageDice)
                {
                    checker = ((WeaponConditionalDamageDice)comp).Conditions;
                }

                if (checker == null) continue;

                if(checker.Operation == Operation.And)
                {
                    checker.Operation = Operation.Or;
                }
                else if(checker.Operation == Operation.Or)
                {
                    checker.Operation = Operation.And;
                }
            }
        }

        static void ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid itemEquipmentUsableId, BlueprintAbilityGuid currentAbilityId, BlueprintAbilityGuid newAbilityId)
        {
            if (!itemEquipmentUsableId.GetBlueprint(out BlueprintItemEquipmentUsable itemEquipmentUsable)) return;
            if (!newAbilityId.GetBlueprint(out BlueprintAbility newAbility)) return;

            if(itemEquipmentUsable.Ability != null && itemEquipmentUsable.Ability.AssetGuid == currentAbilityId.guid)
            {
                itemEquipmentUsable.Ability = newAbility;
            }
        }

        static void ReplaceCheckSkillType(BlueprintCheckGuid checkId, StatType currentStat, StatType newStat)
        {
            if (!checkId.GetBlueprint(out BlueprintCheck check)) return;

            if(check.Type == currentStat)
            {
                check.Type = newStat;
            }
        }

        static void AddWeaponEnchantment(BlueprintItemWeaponGuid weaponId, BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
        {
            if(!weaponId.GetBlueprint(out BlueprintItemWeapon weapon)) return;
            if(!weaponEnchantmentId.GetBlueprint(out BlueprintWeaponEnchantment enchantment)) return;

            weapon.Enchantments.Add(enchantment);
        }

        static void SetWeaponTypeLight(BlueprintWeaponTypeGuid weaponTypeId, bool light)
        {
            if (!GetBlueprint(weaponTypeId, out BlueprintWeaponType weaponType)) return;

            weaponType.m_IsLight = light;
        }

        static void ChangeAddStatBonusScaledDescriptor(BlueprintObjectGuid bpId, ModifierDescriptor expectedDescriptor, ModifierDescriptor newDescriptor)
        {
            if (!bpId.GetBlueprint(out BlueprintScriptableObject bp)) return;

            var statBonusComponent = bp.Components.FirstOrDefault(c => c is AddStatBonusScaled) as AddStatBonusScaled;
            if(statBonusComponent.Descriptor == expectedDescriptor)
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

    [HarmonyPatch(typeof(RuleCombatManeuver), nameof(RuleCombatManeuver.IsSuccessRoll))]
    static class RuleCombatManeuver_IsSuccessRoll_Patch
    {
        // If a unit is immune to a Combat Maneuver (ex: Freedom of Movement's immunity to Grapple),
        // IsSuccess will be checked with a d20 roll of 0 and Target CMD of 0, which always succeeds,
        // rather than always fails
        // This fix makes a d20 roll of 0 always fail
        [HarmonyPostfix]
        static bool Fix(bool __result, int d20) => d20 != 0 && __result;
    }
}
