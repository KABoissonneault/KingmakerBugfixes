using Kingmaker.EntitySystem.Stats;
using Kingmaker.UnitLogic.FactLogic;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

using static kmbf.Blueprint.BlueprintCommands;

namespace kmbf.Patch
{
    static class ItemFixes
    {
        public static void Apply()
        {
            ChangeDartsWeaponType();

            FixDatura();
            FixNaturesWrath();
            FixSummonNaturesAllyVSingle();
            FixDwarvenChampion();
            FixRingOfRecklessCourageStatBonus();                        
        }

        static void ChangeDartsWeaponType()
        {
            // Make Darts light weapons (like in tabletop)
            SetWeaponTypeLight(BlueprintWeaponTypeGuid.Dart, light: true);
        }


        // 'Datura' automatically removes its own sleep on attack, since the damage applies after the buff
        // Fix this by using another component that allows waiting for the attack to resolve
        // Also, add a ContextSetAbilityParams component to allow the DC to be properly 16
        // Finally, add the enchantment to the second head
        static void FixDatura()
        {
            ReplaceAttackRollTriggerToWeaponTrigger(BlueprintWeaponEnchantmentGuid.Soporiferous, WaitForAttackResolve: true);
            SetContextSetAbilityParamsDC(BlueprintWeaponEnchantmentGuid.Soporiferous, 16);
            AddWeaponEnchantment(BlueprintItemWeaponGuid.SoporiferousSecond, BlueprintWeaponEnchantmentGuid.Soporiferous);
        }

        // Nature's Wrath trident "Outsider AND Aberration ..." instead of OR
        // Fix "Electricity Vulnerability" debuff to apply to target instead of initiator
        static void FixNaturesWrath()
        {            
            FlipWeaponConditionAndOr(BlueprintWeaponEnchantmentGuid.NaturesWrath);
            ReplaceWeaponBuffOnAttackToWeaponTrigger(BlueprintWeaponEnchantmentGuid.NaturesWrath);
        }

        // Scroll of Summon Nature's Ally V (Single) would Summon Monster V (Single) instead
        static void FixSummonNaturesAllyVSingle()
        {
            ReplaceUsableAbility(BlueprintItemEquipmentUsableGuid.ScrollSummonNaturesAllyVSingle, BlueprintAbilityGuid.SummonMonsterVSingle, BlueprintAbilityGuid.SummonNaturesAllyVSingle);
        }

        // Give it immunity to Attacks of Opportunity, rather than immunity to Immunity to Attacks of Opportunity
        static void FixDwarvenChampion()
        {
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.DwarvenChampionEnchant)
                .RemoveComponents<AddConditionImmunity>()
                .AddComponent<AddCondition>(c => c.Condition = Kingmaker.UnitLogic.UnitCondition.ImmuneToAttackOfOpportunity)
                .Configure();
        }

        // All of the Ring of Reckless Courage bonuses are put on a Feature added by an AddUnitFeatureEquipment enchantment on the item
        // Its +4 Charisma bonus could only be considered permanent if it was added by a AddStatBonusEquipment component instead of AddStatBonus,
        // but those are only valid on Equipment Enchantments, not Features
        // Remove the +4 Charisma from the feature, and give the ring a second Charisma4 enchantment instead
        static void FixRingOfRecklessCourageStatBonus()
        {
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.RingOfRecklessCourage)
                .RemoveComponentsWhere<AddStatBonus>(c => c.Stat == StatType.Charisma)
                .Configure();

            BlueprintItemEquipmentSimpleConfigurator.From(BlueprintItemEquipmentRingGuid.RingOfRecklessCourage)
                .AddEnchantment(BlueprintEquipmentEnchantmentGuid.Charisma4)
                .Configure();
        }
    }
}
