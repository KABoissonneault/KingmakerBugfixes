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
using Kingmaker.Kingdom.Artisans;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using UnityEngine;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Items.Ecnchantments;

namespace kmbf;

public static class Main {
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        Log = modEntry.Logger;

        modEntry.OnGUI = OnGUI;
        try {
            HarmonyInstance = new Harmony(modEntry.Info.Id);
        } catch {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    public static void OnGUI(UnityModManager.ModEntry modEntry) {

    }

    class BlueprintObjectGuid
    {
        public string guid;

        public BlueprintObjectGuid(string guid)
        {
            this.guid = guid;
        }
    }

    class BlueprintFactGuid : BlueprintObjectGuid
    {
        public BlueprintFactGuid(string guid)
            : base(guid)
        {

        }
    }
    
    class BlueprintUnitFactGuid : BlueprintFactGuid
    {
        public BlueprintUnitFactGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintAbilityGuid : BlueprintUnitFactGuid
    {
        public BlueprintAbilityGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintCharacterClassGuid : BlueprintObjectGuid
    {
        public BlueprintCharacterClassGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintCueGuid : BlueprintObjectGuid
    {
        public BlueprintCueGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintKingdomArtisanGuid : BlueprintObjectGuid
    {
        public BlueprintKingdomArtisanGuid(string guid)
            : base(guid)
        {

        }
    }
    
    class BlueprintItemEnchantmentGuid : BlueprintFactGuid
    {
        public BlueprintItemEnchantmentGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintWeaponEnchantmentGuid : BlueprintItemEnchantmentGuid
    {
        public BlueprintWeaponEnchantmentGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintBuffGuid : BlueprintFactGuid
    {
        public BlueprintBuffGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintItemGuid : BlueprintObjectGuid
    {
        public BlueprintItemGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintItemEquipmentGuid : BlueprintItemGuid
    {
        public BlueprintItemEquipmentGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintItemWeaponGuid : BlueprintItemGuid
    {
        public BlueprintItemWeaponGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintItemEquipmentUsableGuid : BlueprintItemEquipmentGuid
    {
        public BlueprintItemEquipmentUsableGuid(string guid)
            : base(guid)
        {

        }
    }

    class BlueprintCheckGuid : BlueprintObjectGuid
    {
        public BlueprintCheckGuid(string guid)
            : base(guid)
        {

        }
    }

    static string GetDebugName(BlueprintScriptableObject bp)
    {
        return $"Blueprint:{bp.AssetGuid}:{bp.name}";
    }

    static string GetDebugName(BlueprintAbility ability)
    {
        return $"BlueprintAbility:{ability.AssetGuid}:{ability.name}";
    }

    static string GetDebugName(BlueprintCharacterClass characterClass)
    {
        return $"BlueprintCharacterClass:{characterClass.AssetGuid}:{characterClass.name}";
    }

    static string GetDebugName(BlueprintCue cue)
    {
        return $"BlueprintCue:{cue.AssetGuid}:{cue.name}";
    }

    static string GetDebugName(BlueprintKingdomArtisan artisan)
    {
        return $"BlueprintKingdomArtisan:{artisan.AssetGuid}:{artisan.name}";
    }

    // Abilities
    static readonly BlueprintAbilityGuid DarknessDomainGreaterAbility = new BlueprintAbilityGuid("31acd268039966940872c916782ae018");
    static readonly BlueprintAbilityGuid SummonMonsterVSingle = new BlueprintAbilityGuid("0964bf88b582bed41b74e79596c4f6d9");
    static readonly BlueprintAbilityGuid SummonNaturesAllyVSingle = new BlueprintAbilityGuid("28ea1b2e0c4a9094da208b4c186f5e4f");

    // Character classes
    static readonly BlueprintCharacterClassGuid DruidClass = new BlueprintCharacterClassGuid("610d836f3a3a9ed42a4349b62f002e96");

    // Cue
    static readonly BlueprintCueGuid IrleneGiftCue3 = new BlueprintCueGuid("03807d3897f73e44b84b476ae63a62f1");

    // Artisans
    static readonly BlueprintKingdomArtisanGuid Woodmaster = new BlueprintKingdomArtisanGuid("670c334ec3ecd1640b70024ea93d9229");
    static readonly BlueprintKingdomArtisanGuid ShadyTrader = new BlueprintKingdomArtisanGuid("42efca2aecce9ff43ad3ed2d4d516124");

    // Weapon Enchantment
    static readonly BlueprintWeaponEnchantmentGuid SoporiferousEnchantment = new BlueprintWeaponEnchantmentGuid("da0a0c76266c96b45aacc34dc6635b28");
    static readonly BlueprintWeaponEnchantmentGuid BaneLivingEnchantment = new BlueprintWeaponEnchantmentGuid("e1d6f5e3cd3855b43a0cb42f6c747e1c");
    static readonly BlueprintWeaponEnchantmentGuid NaturesWrathEnchantment = new BlueprintWeaponEnchantmentGuid("afa5d47f05724ac43a4dc19e5ecbd150");

    // Item Equipment Weapon
    static readonly BlueprintItemWeaponGuid SoporiferousSecondItem = new BlueprintItemWeaponGuid("af87d71820e93364c81b1aff840344ed");

    // Item Equipment Usable
    static readonly BlueprintItemEquipmentUsableGuid ScrollSummonNaturesAllyVSingle = new BlueprintItemEquipmentUsableGuid("4e9e261a93c7aa144a7b29c9fcfb4986");

    // Check
    static readonly BlueprintCheckGuid ShrewishGulchLastStageTwoActions = new BlueprintCheckGuid("373d384d88b55a244b74009dc6628b0e");
    static readonly BlueprintCheckGuid ShrewishGulchLastStageThreeActions = new BlueprintCheckGuid("e4f4fe6042b99cc4790f0103ae10345e");

    static bool GetBlueprint(BlueprintObjectGuid objectId, out BlueprintScriptableObject bp)
    {
        bp = ResourcesLibrary.TryGetBlueprint<BlueprintScriptableObject>(objectId.guid);
        if (bp == null)
        {
            Log.Error($"Could not find blueprint with GUID '{objectId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintAbilityGuid abilityId, out BlueprintAbility ability)
    {
        ability = ResourcesLibrary.TryGetBlueprint<BlueprintAbility>(abilityId.guid);
        if(ability == null)
        {
            Log.Error($"Could not find ability blueprint with GUID '{abilityId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintCharacterClassGuid characterClassId, out BlueprintCharacterClass characterClass)
    {
        characterClass = ResourcesLibrary.TryGetBlueprint<BlueprintCharacterClass>(characterClassId.guid);
        if(characterClass == null)
        {
            Log.Error($"Could not find character class blueprint with GUID '{characterClassId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintCueGuid cueId, out BlueprintCue cue)
    {
        cue = ResourcesLibrary.TryGetBlueprint<BlueprintCue>(cueId.guid);
        if(cue == null)
        {
            Log.Error($"Could not find cue blueprint with GUID '{cueId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintKingdomArtisanGuid artisanId, out BlueprintKingdomArtisan artisan)
    {
        artisan = ResourcesLibrary.TryGetBlueprint<BlueprintKingdomArtisan>(artisanId.guid);
        if (artisan == null)
        {
            Log.Error($"Could not find Kingdom Artisan blueprint with GUID '{artisanId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintBuffGuid buffId, out BlueprintBuff buff)
    {
        buff = ResourcesLibrary.TryGetBlueprint<BlueprintBuff>(buffId.guid);
        if (buff == null)
        {
            Log.Error($"Could not find Buff blueprint with GUID '{buffId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintCheckGuid checkId, out BlueprintCheck check)
    {
        check = ResourcesLibrary.TryGetBlueprint<BlueprintCheck>(checkId.guid);
        if (check == null)
        {
            Log.Error($"Could not find Check blueprint with GUID '{checkId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintItemWeaponGuid weaponId, out BlueprintItemWeapon weapon)
    {
        weapon = ResourcesLibrary.TryGetBlueprint<BlueprintItemWeapon>(weaponId.guid);
        if (weapon == null)
        {
            Log.Error($"Could not find Item Weapon blueprint with GUID '{weaponId.guid}'");
            return false;
        }

        return true;
    }

    static bool GetBlueprint(BlueprintWeaponEnchantmentGuid weaponEnchantmentId, out BlueprintWeaponEnchantment weaponEnchantment)
    {
        weaponEnchantment = ResourcesLibrary.TryGetBlueprint<BlueprintWeaponEnchantment>(weaponEnchantmentId.guid);
        if (weaponEnchantment == null)
        {
            Log.Error($"Could not find Weapon Enchantment blueprint with GUID '{weaponEnchantmentId.guid}'");
            return false;
        }

        return true;
    }


    static bool GetBlueprint(BlueprintItemEquipmentUsableGuid itemEquipmentUsableGuid, out BlueprintItemEquipmentUsable usable)
    {
        usable = ResourcesLibrary.TryGetBlueprint<BlueprintItemEquipmentUsable>(itemEquipmentUsableGuid.guid);
        if (usable == null)
        {
            Log.Error($"Could not find Item Equipment Usable blueprint with GUID '{itemEquipmentUsableGuid.guid}'");
            return false;
        }

        return true;
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
            if(!GetBlueprint(abilityId, out BlueprintAbility ability)) return;           
            if(!GetDamageRankConfig(ability, out ContextRankConfig damageRankConfig)) return;
            if(!GetBlueprint(characterClassId, out BlueprintCharacterClass characterClass)) return;

            if (!damageRankConfig.m_Class.Any(c => c.AssetGuid == characterClass.AssetGuid))
            {
                damageRankConfig.m_Class = damageRankConfig.m_Class.AddItem(characterClass).ToArray();
            }
        }

        static void ReplaceArtisan(BlueprintCueGuid cueId, BlueprintKingdomArtisanGuid currentArtisan, BlueprintKingdomArtisanGuid newArtisan)
        {
            if (!GetBlueprint(cueId, out BlueprintCue cue)) return;

            foreach(GameAction action in GetGameActionsRecursive(cue.OnShow)
                .Concat(GetGameActionsRecursive(cue.OnStop)))
            {
                var artisanGift = action as KingdomActionGetArtisanGift;
                if(artisanGift != null)
                {
                    if(artisanGift.Artisan != null && artisanGift.Artisan.AssetGuid == currentArtisan.guid)
                    {
                        GetBlueprint(newArtisan, out artisanGift.Artisan);
                    }
                    continue;
                }

                var artisanGiftWithTier = action as KingdomActionGetArtisanGiftWithCertainTier;
                if(artisanGiftWithTier != null)
                {
                    if (artisanGiftWithTier.Artisan != null && artisanGiftWithTier.Artisan.AssetGuid == currentArtisan.guid)
                    {
                        GetBlueprint(newArtisan, out artisanGiftWithTier.Artisan);
                    }
                    continue;
                }
            }
        }

        static void ReplaceAttackRollTriggerToWeaponTrigger(BlueprintObjectGuid bpId, bool WaitForAttackResolve)
        {
            if (!GetBlueprint(bpId, out BlueprintScriptableObject bp)) return;

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
            if (!GetBlueprint(bpId, out BlueprintScriptableObject bp)) return;

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
            if (!GetBlueprint(bpId, out BlueprintScriptableObject bp)) return;

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

            if (!GetBlueprint(newBuffId, out BlueprintBuff newBuff)) return;
            

            GetGameActionsRecursive(attackActions)
                .Select(a => a as ContextActionApplyBuff)
                .NotNull()
                .Where(a => a.Buff.AssetGuid == currentBuffId.guid)
                .ForEach(a => a.Buff = newBuff);
        }

        static void SetContextSetAbilityParamsDC(BlueprintObjectGuid bpId, int DC)
        {
            if (!GetBlueprint(bpId, out BlueprintScriptableObject bp)) return;

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
            if (!GetBlueprint(weaponEnchantmentId, out BlueprintScriptableObject bp)) return;

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
            if (!GetBlueprint(itemEquipmentUsableId, out BlueprintItemEquipmentUsable itemEquipmentUsable)) return;
            if (!GetBlueprint(newAbilityId, out BlueprintAbility newAbility)) return;

            if(itemEquipmentUsable.Ability != null && itemEquipmentUsable.Ability.AssetGuid == currentAbilityId.guid)
            {
                itemEquipmentUsable.Ability = newAbility;
            }
        }

        static void ReplaceCheckSkillType(BlueprintCheckGuid checkId, StatType currentStat, StatType newStat)
        {
            if (!GetBlueprint(checkId, out BlueprintCheck check)) return;

            if(check.Type == currentStat)
            {
                check.Type = newStat;
            }
        }

        static void AddWeaponEnchantment(BlueprintItemWeaponGuid weaponId, BlueprintWeaponEnchantmentGuid weaponEnchantmentId)
        {
            if(!GetBlueprint(weaponId, out BlueprintItemWeapon weapon)) return;
            if (!GetBlueprint(weaponEnchantmentId, out BlueprintWeaponEnchantment enchantment)) return;

            weapon.Enchantments.Add(enchantment);
        }

        static void Postfix()
        {
            if (loaded) return;
            loaded = true;

            // Blight Druid Darkness Domain's Moonfire damage scaling
            AddAbilityDamageDiceRankClass(DarknessDomainGreaterAbility, DruidClass);

            // Irlene "Relations rank 3" tier 3 gift
            ReplaceArtisan(IrleneGiftCue3, Woodmaster, ShadyTrader);

            // 'Datura' weapon attack buff
            ReplaceAttackRollTriggerToWeaponTrigger(SoporiferousEnchantment, WaitForAttackResolve: true); // The weapon attack automatically removes the sleep
            SetContextSetAbilityParamsDC(SoporiferousEnchantment, 16); // DC is 11 by default, raise it to 16 like in the description
            AddWeaponEnchantment(SoporiferousSecondItem, SoporiferousEnchantment);

            // Bane of the Living "Not Undead or Not Construct" instead of "Not Undead and Not Construct"
            FlipWeaponConditionAndOr(BaneLivingEnchantment);

            // Nature's Wrath trident "Outsider AND Aberration ..." instead of OR
            // Fix "Electricity Vulnerability" debuff to apply to target instead of initiator
            FlipWeaponConditionAndOr(NaturesWrathEnchantment);
            ReplaceWeaponBuffOnAttackToWeaponTrigger(NaturesWrathEnchantment);

            // Scroll of Summon Nature's Ally V (Single) would Summon Monster V (Single) instead
            ReplaceUsableAbility(ScrollSummonNaturesAllyVSingle, SummonMonsterVSingle, SummonNaturesAllyVSingle);

            // Shrewish Gulch Last Stage "Two Actions" and "Three Actions" checks
            ReplaceCheckSkillType(ShrewishGulchLastStageTwoActions, StatType.SkillLoreNature, StatType.SkillAthletics);
            ReplaceCheckSkillType(ShrewishGulchLastStageThreeActions, StatType.SkillLoreNature, StatType.SkillAthletics);
        }
    }
}
