using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Components;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using kmbf.Component;

using static kmbf.Blueprint.Builder.ElementBuilder;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch
{
    static class ItemFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Item patches");

            ChangeDartsWeaponType();

            FixDatura();
            FixNaturesWrath();
            FixSummonNaturesAllyVSingle();
            FixDwarvenChampion();
            FixRingOfRecklessCourageStatBonus();
            FixQuivers();
            FixCursedItemCasterLevels();
            FixBladeOfTheMerciful();
            FixGamekeeperOfTheFirstWorld();

            // Optional
            FixBaneOfTheLiving();
            FixNecklaceOfDoubleCrosses();
        }

        // Make Darts light weapons (like in tabletop)
        static void ChangeDartsWeaponType()
        {
            if (!StartPatch("Darts Weapon Type")) return;

            BlueprintWeaponTypeConfigurator.From(WeaponTypeRefs.Dart)
                .SetIsLight(true)
                .Configure();
        }

        // 'Datura' automatically removes its own sleep on attack, since the damage applies after the buff
        // Fix this by using another component that allows waiting for the attack to resolve
        // Also, add a ContextSetAbilityParams component to allow the DC to be properly 16
        // Finally, add the enchantment to the second head
        static void FixDatura()
        {
            if (!StartPatch("Datura")) return;

            BlueprintWeaponEnchantmentConfigurator.From(WeaponEnchantmentRefs.Soporiferous)
                .ReplaceAttackRollTriggerWithWeaponTrigger(c =>
                {
                    c.WaitForAttackResolve = true;
                })
                .SetDC(16, add10ToDC: false)
                .Configure();
                        
            BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.SoporiferousSecond)
                .AddEnchantment(WeaponEnchantmentRefs.Soporiferous)
                .Configure();
        }

        // Nature's Wrath trident accidentally checks "Outsider AND Aberration ..." instead of OR
        // Fix "Electricity Vulnerability" debuff to apply to target instead of initiator
        static void FixNaturesWrath()
        {
            if (!StartPatch("Nature's Wrath (Trident)")) return;

            BlueprintObjectConfigurator.From(WeaponEnchantmentRefs.NaturesWrath)
                .EditComponent<WeaponConditionalDamageDice>(c =>
                {
                    c.Conditions.Operation = Operation.Or;
                })
                .ReplaceComponents<WeaponBuffOnAttack, AddInitiatorAttackWithWeaponTrigger>((buffOnAttack, weaponTrigger) =>
                {
                    weaponTrigger.Action = ActionListFactory.Single(
                        MakeContextActionApplyBuffSeconds(new BlueprintBuffGuid(buffOnAttack.Buff.AssetGuid), (float)buffOnAttack.Duration.Seconds.TotalSeconds)
                    );
                    weaponTrigger.OnlyHit = true;
                })
                .Configure();
        }

        // Scroll of Summon Nature's Ally V (Single) would Summon Monster V (Single) instead
        static void FixSummonNaturesAllyVSingle()
        {
            if(!StartPatch("Summon Nature's Ally V (Single)")) return;

            BlueprintItemEquipmentConfigurator.From(ItemEquipmentUsableRefs.ScrollSummonNaturesAllyVSingle)
                .SetAbility(AbilityRefs.SummonNaturesAllyVSingle)
                .Configure();
        }

        // Give it immunity to Attacks of Opportunity, rather than immunity to Immunity to Attacks of Opportunity
        static void FixDwarvenChampion()
        {
            if (!StartPatch("Solid Strategy")) return;

            BlueprintFeatureConfigurator.From(FeatureRefs.SolidStrategyEnchant)
                .RemoveComponents<AddConditionImmunity>()
                .AddComponent<AddCondition>(c => c.Condition = Kingmaker.UnitLogic.UnitCondition.ImmuneToAttackOfOpportunity)
                .Configure();
        }

        // All of the Ring of Reckless Courage bonuses are put on a Feature added by an AddUnitFeatureEquipment enchantment on the item
        // Its +4 Charisma bonus could only be considered permanent if it was added by a AddStatBonusEquipment component instead of AddStatBonus,
        // but those are only valid on Equipment Enchantments, not Features
        // Remove the +4 Charisma from the feature, and give the ring a Charisma4 enchantment instead
        static void FixRingOfRecklessCourageStatBonus()
        {
            if (!StartPatch("Ring of Reckless Courage")) return;

            BlueprintFeatureConfigurator.From(FeatureRefs.RingOfRecklessCourage)
                .RemoveComponentsWhere<AddStatBonus>(c => c.Stat == StatType.Charisma)
                .Configure();

            BlueprintItemEquipmentSimpleConfigurator.From(ItemEquipmentRingRefs.RingOfRecklessCourage)
                .AddEnchantment(EquipmentEnchantmentRefs.Charisma4)
                .Configure();
        }

        // Both Quiver of Lightning Arrows and Quiver of Lovers Arrows mention shooting "speed arrows", but are actually missing the extra attack that the other quivers have
        static void FixQuivers()
        {
            if (!StartPatch("Quiver Haste")) return;

            BlueprintObjectConfigurator.From(WeaponEnchantmentRefs.LightningArrows)
                .AddComponent<WeaponExtraAttack>(c =>
                {
                    c.Number = 1;
                    c.Haste = true;
                })
                .Configure();

            BlueprintObjectConfigurator.From(WeaponEnchantmentRefs.LoversArrows)
                .AddComponent<WeaponExtraAttack>(c =>
                {
                    c.Number = 1;
                    c.Haste = true;
                })
                .Configure();
        }

        // There are three cursed magic items in KM: Cloak of Sold Souls, Gentle Persuasion, and The Narrow Path
        // All three of them have a fixed DC in their description (ex: DC 25).
        // When using Remove Curse, this claim is correct, as the Dispel Magic action targets the specified DC of those curses
        // When using Break Enchantments, the Dispel Magic targets the caster level instead, which gives a DC of 11 + Caster Level
        // Cloak of Sold Souls sets a caster level of 25, giving DC 36 (instead of 25)
        // Gentle Persuasion sets a caster level of 0, giving DC 11 (instead of 33)
        // The Narrow Path sets a caster level of 15, giving DC 26 (instead of 25)
        // According to the tabletop description, Break Enchantment should be able to remove the curse from magic items (as it does in KM), but it should
        // use the DC of the curse.
        // Since this is one specific case hitting only three curses, instead of changing Break Enchantment to use the DC on cursed magic items,
        // we'll adjust the caster level of the three curses to match DC - 11
        static void FixCursedItemCasterLevels()
        {
            if (!StartPatch("Cursed Items Dispel")) return;

            BlueprintBuffConfigurator.From(BuffRefs.CloakOfSoldSoulsCurse)
                .SetCasterLevel(ContextValueFactory.Simple(14))
                .Configure();

            BlueprintBuffConfigurator.From(BuffRefs.GentlePersuasionCurse)
                .SetCasterLevel(ContextValueFactory.Simple(22))
                .Configure();

            BlueprintBuffConfigurator.From(BuffRefs.NarrowPathCurse)
                .SetCasterLevel(ContextValueFactory.Simple(14))
                .Configure();
        }
        
        // Blade of the Merciful is a scimitar that can be bought in HatEoT, which has the ability to cast Mass Heal
        // Since HatEoT notably has a bunch of ghosts that are vulnerable to offensive use of Heal, the DC of this item
        // is relevant. However, it is set to 0
        static void FixBladeOfTheMerciful()
        {
            if (!StartPatch("Blade of the Merciful")) return;

            BlueprintItemWeaponConfigurator.From(ItemWeaponRefs.BladeOfTheMerciful)
                .SetDC(23) // 10 + spell level + "modifier from minimum ability to cast spell" = 10 + 9 + 4 (19 wisdom)
                .Configure();
        }

        // Game Keeper of the First World applies a "Glitterdust like" debuff on hit
        // What's not mentioned in the description is that it also tries to apply Glitterdust Blindness,
        // but the Saving Throw on application is of Unknown type (untyped), and the DC is 11
        // On each round, the saving throw to remove the blindness is properly Will
        // However, the duration of the Blindness is 0 rounds, so it is a useless log spam of saving throws
        // Just remove this effect
        // Show the main buff on enemies Inspect as a convenience, to track this effect more easily
        static void FixGamekeeperOfTheFirstWorld()
        {
            if (!StartPatch("Gamekeeper of the First World")) return;

            BlueprintBuffConfigurator.From(BuffRefs.GameKeeperOfTheFirstWorld)
                .RemoveComponents<AddFactContextActions>()
                .SetDisplayName(KMLocalizedStrings.GameKeeperOfTheFirstWorldName)
                .SetDescription(KMLocalizedStrings.GameKeeperOfTheFirstWorldDescription)
                .RemoveFlag(BlueprintBuff.Flags.HiddenInUi)
                .Configure();
        }

        // Bane of the Living / Penalty "Not Undead or Not Construct" instead of "Not Undead and Not Construct"
        static void FixBaneOfTheLiving()
        {
            if (!StartBalancePatch("Bane of the Living", nameof(BalanceSettings.FixBaneLiving))) return;
            
            BlueprintObjectConfigurator.From(WeaponEnchantmentRefs.BaneLiving)
                .EditComponent<WeaponConditionalEnhancementBonus>(c =>
                {
                    c.Conditions.Operation = Operation.And;
                })
                .Configure();
        }

        // In the base game, Necklace of Double Crosses applies to all sneak attacks, and the "attack against allies" mechanic is not implemented at all
        private static void FixNecklaceOfDoubleCrosses()
        {
            if (!StartBalancePatch("Necklace of Double Crosses", nameof(BalanceSettings.FixNecklaceOfDoubleCrosses))) return;

            BlueprintFeatureConfigurator.From(FeatureRefs.NecklaceOfDoubleCrosses)
                .EditComponent<AdditionalSneakDamageOnHit>(c => c.m_Weapon = AdditionalSneakDamageOnHit.WeaponType.Melee)
                .AddComponent<AooAgainstAllies>()
                .Configure();
        }
    }
}
