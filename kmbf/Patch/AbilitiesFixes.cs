using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using kmbf.Action;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using kmbf.Component;

using static kmbf.Blueprint.Builder.ElementBuilder;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch
{
    static class AbilitiesFixes
    {
        public static void Apply()
        {
            Main.Log.Log("Starting Ability patches");

            FixEkunWolfBuffs();
            FixMagicalVestmentShield();
            FixRaiseDead();
            FixBreathOfLife();
            FixJoyfulRapture();
            FixProtectionFromArrows();
            FixLeopardCompanionUpgrade();
            FixGazeImmunities();
            FixTieflingFoulspawn();
            FixExplosionRing();
            FixBreakEnchantment();
            FixAbilityScoreCheckBonuses();

            // Optional
            FixTouchOfGlory();
            TweakCombatExpertise();
            FixNauseatedPoisonDescriptor();
            FixShatterDefenses();
            FixCraneWingHandCheck();
            FixControlledFireball();
        }

        static void FixEkunWolfBuffs()
        {
            if (!StartPatch("Ekun Wolf Buffs")) return;

            // The "Master" features are accidentally added to Dog by the dialogue. Might as well put the stat bonuses directly on it
            // Plus the Offensive Master feature was accidentally giving the Defensive buff
            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.EkunWolfOffensiveMaster)
                .ReplaceAllComponentsWithSource(BlueprintFeatureGuid.EkunWolfOffensiveBuff)
                .Configure();

            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.EkunWolfDefensiveMaster)
                .ReplaceAllComponentsWithSource(BlueprintFeatureGuid.EkunWolfDefensiveBuff)
                .Configure();
        }

        static void FixMagicalVestmentShield()
        {
            if (!StartPatch("Magical Vestment Shield", ModExclusionFlags.CallOfTheWild)) return;

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            BlueprintBuffConfigurator.From(BlueprintBuffGuid.MagicalVestmentShield)
                .EditComponent<AddStatBonusScaled>(c =>
                {
                    c.Descriptor = ModifierDescriptor.ShieldEnhancement;
                })
                .Configure();
        }

        // Raise Dead does not actually give two negative levels
        // Like in Wrath of the Righteous, we add a condition on whether Enemy Stats Adjustment is Normal or above
        // Don't use this in cutscenes though, it affects the final fight
        static void FixRaiseDead()
        {
            if (!StartPatch("Raise Dead")) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.RaiseDead)
                .EditComponent<AbilityEffectRunAction>(runAction =>
                {
                    var difficultyCheck = ContextConditionDifficultyHigherThanConfigurator
                        .New(BlueprintRoot.Instance.DifficultyList.CoreDifficulty)
                        .SetCheckOnlyForMonsterCaster(false)
                        .Configure();

                    var dealDamageAction = ContextActionDealDamageConfigurator
                        .NewPermanentEnergyDrain(ContextDiceFactory.BonusConstant(2))
                        .Configure();

                    Conditional difficultyConditional = MakeGameActionConditional
                    (
                        new ConditionsChecker() { Conditions = [difficultyCheck] }
                        , ifTrue: new ActionList() { Actions = [dealDamageAction] }
                     );

                    // Put the drain first, resurrection makes the unit untargetable
                    runAction.Actions.Actions = [difficultyConditional, .. runAction.Actions.Actions];
                })
                .Configure();

            if (!BlueprintAbilityGuid.RaiseDead.GetBlueprint(out BlueprintAbility raiseDead)) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.RaiseDead_Cutscene)
                .SetDisplayName(raiseDead.m_DisplayName)
                .SetDescription(raiseDead.m_Description)
                .SetIcon(raiseDead.m_Icon)
                .Configure();

            BlueprintCueConfigurator.From(BlueprintCueGuid.LKBattle_Phase5_Cue_0065)
                .EditOnStopActionRecursiveWhere<EvaluatedTrapCastSpell>(pred: null, editAction: a =>
                {
                    if (!BlueprintAbilityGuid.RaiseDead_Cutscene.GetBlueprint(out BlueprintAbility raiseDead_Cutscene)) return;

                    a.Spell = raiseDead_Cutscene;
                })
                .Configure();
        }

        // Breath of Life does not actually give one negative level when used to resurrect
        // Like in Wrath of the Righteous, we add a condition on whether Enemy Stats Adjustment is Normal or above
        static void FixBreathOfLife()
        {
            if (!StartPatch("Breath of Life")) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.BreathOfLifeTouch)
                .SetFullRoundAction(false)
                .EditComponent<AbilityEffectRunAction>(c =>
                {
                    var aliveConditional = c.Actions.GetGameAction<Conditional>();
                    if (aliveConditional != null)
                    {
                        var isPartyMemberConditional = aliveConditional.IfFalse.GetGameAction<Conditional>();
                        if (isPartyMemberConditional != null)
                        {

                            var difficultyConditional = MakeGameActionConditional
                            (
                                ConditionsCheckerFactory.Single
                                (
                                    ContextConditionDifficultyHigherThanConfigurator.New(BlueprintRoot.Instance.DifficultyList.CoreDifficulty)
                                        .SetCheckOnlyForMonsterCaster(false)
                                        .Configure()
                                ),
                                ifTrue: ActionListFactory.From
                                (
                                    ContextActionDealDamageConfigurator
                                        .NewTemporaryEnergyDrain(ContextDiceFactory.BonusConstant(1), ContextDurationFactory.ConstantDays(1))
                                        .Configure()
                                )
                            );

                            isPartyMemberConditional.IfTrue.Actions = [difficultyConditional, .. isPartyMemberConditional.IfTrue.Actions];
                        }
                    }
                })
                .Configure();
        }

        // Joyful Rapture is supposed to free all allies from any "emotion effects", but the base game only includes Petrified
        // Add Fear, Shaken, Frightened, and NegativeEmotion, which include the "Unbreakable Heart" descriptors, plus overall
        // negative emotion
        static void FixJoyfulRapture()
        {
            if (!StartPatch("Joyful Rapture")) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.JoyfulRapture)
                .EditComponentGameAction<AbilityEffectRunAction, ContextActionDispelMagic>("$ContextActionDispelBuffs$b4781573-55ad-4e71-9dd9-75a0c38652e0", a =>
                {
                    a.Descriptor |= SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Frightened | SpellDescriptor.NegativeEmotion;
                })
                .Configure();
        }

        // Protection From Arrows Communal should not have spell resistance, like Protection from Arrows, and like other communal buffs
        // Protection from Arrows generally fails to protect from mundane arrows. This patch replaces the poorly made DRAgainstRangedWithPool
        // component with the general AddDamageResistancePhysical, with a bypass by any magic or melee weapons
        // Note that without FixWeaponEnhancementDamageReduction, Composite bows and Thrown weapons are treated as Magic for this check
        static void FixProtectionFromArrows()
        {
            if (!StartPatch("Protection from Arrows")) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.ProtectionFromArrowsCommunal)
                .SetSpellResistance(false)
                .Configure();

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.ProtectionFromArrows)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.Or = true;
                    c.BypassedByMagic = true;
                    c.BypassedByMeleeWeapon = true;
                    c.Value = ContextValueFactory.Simple(10);
                    c.UsePool = true;
                    c.Pool = ContextValueFactory.Rank();
                })
                .EditOrAddDefaultContextRankConfig(c =>
                {
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.SetMultiplyByModifier(step: 10, max: 100);
                })
                .Configure();

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.ProtectionFromArrowsCommunal)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.Or = true;
                    c.BypassedByMagic = true;
                    c.BypassedByMeleeWeapon = true;
                    c.Value = ContextValueFactory.Simple(10);
                    c.UsePool = true;
                    c.Pool = ContextValueFactory.Rank();
                })
                .EditOrAddDefaultContextRankConfig(c =>
                {
                    c.m_BaseValueType = ContextRankBaseValueType.CasterLevel;
                    c.SetMultiplyByModifier(step: 10, max: 100);
                })
                .Configure();
        }

        // Description says +2 Dex, +2 Con, but the game applies +4 str, -2 dex, and +4 con
        static void FixLeopardCompanionUpgrade()
        {
            if (!StartPatch("Leopard Companion")) return;

            BlueprintObjectConfigurator.From(BlueprintFeatureGuid.AnimalCompanionUpgradeLeopard)
                .RemoveComponentsWhere<AddStatBonus>(b => b.Stat == StatType.Strength)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Constitution, b => b.Value = 2)
                .EditComponentWhere<AddStatBonus>(b => b.Stat == StatType.Dexterity, b => b.Value = 2)
                .Configure();
        }

        // Baleful Gaze applies stat damage, which does not check descriptor immunities like "Sight Based" or "Gaze Attack"
        // Add a conditional wrapper that checks for the immunity for now
        // Still does not cover Saving Throw bonuses and the like, but we'll see if anyone notices
        static void FixGazeImmunities()
        {
            if (!StartPatch("Gaze Attack Immunity")) return;

            BlueprintAbilityAreaEffectConfigurator.From(BlueprintAbilityAreaEffectGuid.BalefulGaze)
                .EditRoundActions(roundActions =>
                {
                    foreach(GameAction action in roundActions.GetGameActionsRecursive())
                    {
                        // Skip the own wrappers we're adding
                        if (action is Conditional cond && cond.ConditionsChecker.HasConditions && cond.ConditionsChecker.Conditions[0] is ContextConditionHasSpellImmunityToContextDescriptors)
                            continue;

                        foreach(ActionList childList in action.GetChildActionLists())
                        {
                            for(int index = 0; index < childList.Actions.Length; ++index)
                            {
                                var childAction = childList.Actions[index];
                                if(childAction is ContextActionSavingThrow)
                                {
                                    var conditional = MakeGameActionConditional(
                                        ConditionsCheckerFactory.Single(MakeContextConditionHasSpellImmunityToContextDescriptors())
                                        , ifFalse: ActionListFactory.Single(childAction)
                                        );

                                    childList.Actions[index] = conditional;
                                }
                            }
                        }
                    }
                })
                .AddSpellDescriptor(SpellDescriptor.SightBased)
                .Configure();
        }

        // Foulspawn tieflings are supposed to have a bonus against Cleric, Paladins, and Inquisitors
        // It checks if the target has all three classes instead of any
        static void FixTieflingFoulspawn()
        {
            if (!StartPatch("Foulspawn Tiefling")) return;

            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.TieflingHeritageFoulspawn)
                .EditComponent<AttackBonusConditional>(c =>
                {
                    c.Conditions.Operation = Operation.Or;
                })
                .Configure();
        }
        
        // The +12 damage only applies to Bomb weapons
        // Since Bombs are virtually always abilities, we need a special component
        // The +12 damage relies on the Bomb descriptor on the ability, so we also add it to abilities that are missing it (cross-referencing WotR to make sure the descriptor is intended)
        static void FixExplosionRing()
        {
            if (!StartPatch("Explosion Ring")) return;

            BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.ExplosionRing)
                .AddComponent<AdditionalBonusOnDamage>(c =>
                {
                    c.BonusOnDamage = 12;
                    c.CheckSpellDescriptor = true;
                    c.SpellDescriptorsList = SpellDescriptor.Bomb;
                })
                .Configure();

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.AlchemistFire)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.AcidFlask)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();
        }

        // Break Enchantment should remove effects "that can be dispelled by 'stone to flesh'" in tabletop
        // The in-game description agrees that it should remove transmutations
        // So I'm bringing the "remove petrify effects" from WotR
        static void FixBreakEnchantment()
        {
            if (!StartPatch("Break Enchantment")) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.BreakEnchantment)
                .AddAbilityEffectRunAction(
                    MakeGameActionConditional(
                        ConditionsCheckerFactory.Single(MakeContextConditionHasBuffWithDescriptor(SpellDescriptor.Petrified)),
                        ifTrue: ActionListFactory.Single(
                            ContextActionDispelMagicConfigurator.New(ContextActionDispelMagic.BuffType.All)
                                .SetDescriptor(SpellDescriptor.Petrified)
                                .AddOnSuccessAction(MakeContextActionResurrect(0.0f))
                                .Configure()
                        )
                    )
                )
                .EditComponent<AbilityTargetsAround>(c =>
                {
                    c.m_IncludeDead = true;
                })
                .Configure();
        }

        // While AbilityScoreCheckBonus has been fixed by this mod,
        // it's still not ideal for skill bonuses from a user perspective, because those
        // bonuses are invisible in the character sheet.
        // Change to an AddContextStatBonus for similar results
        static void FixAbilityScoreCheckBonuses()
        {
            if (StartPatch("Strength Surge"))
            {
                BlueprintBuffConfigurator.From(BlueprintBuffGuid.StrengthSurge)
                    .RemoveComponents<AbilityScoreCheckBonus>()
                    .AddComponent<AddContextStatBonus>(b =>
                    {
                        b.Stat = StatType.SkillAthletics;
                        b.Descriptor = ModifierDescriptor.Enhancement;
                        b.Value = ContextValueFactory.Rank();
                    })
                    .Configure();
            }

            if (StartPatch("Animal Domain Perception Bonus"))
            {
                BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.AnimalDomainBaseFeature)
                    .RemoveComponents<AbilityScoreCheckBonus>()
                    .AddComponent<AddContextStatBonus>(b =>
                    {
                        b.Stat = StatType.SkillPerception;
                        b.Descriptor = ModifierDescriptor.Racial;
                        b.Value = ContextValueFactory.Rank();
                    })
                    .Configure();
            }
        }

        // Touch of Glory adds +1-10 Charisma instead of adding +1-10 to Charisma checks
        // Fortunately, this mod fixed AbilityScoreCheckBonus, so we can use that instead
        static void FixTouchOfGlory()
        {
            if (!StartBalancePatch("Touch of Glory", nameof(BalanceSettings.FixTouchOfGlory))) return;

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.TouchOfGlory)
                .RemoveComponentWhere<AddContextStatBonus>(c => c.Stat == StatType.Charisma)
                .AddComponent<AbilityScoreCheckBonus>(c =>
                {
                    c.Stat = StatType.Charisma;
                    c.Bonus = ContextValueFactory.Rank(AbilityRankType.DamageBonus); // Not sure why DamageBonus instead of Default, but eh
                    c.Descriptor = ModifierDescriptor.UntypedStackable;
                })
                .Configure();
        }

        // Combat Expertise has the annoying tendency to turn itself back on whenever you do something that refreshes the character, like saving or loading
        // It's niche enough that it's probably better for it to be off by default, whenever the feature gets readded
        static void TweakCombatExpertise()
        {
            if (!StartQualityOfLifePatch("Combat Expertise Off By Default", nameof(QualityOfLifeSettings.CombatExpertiseOffByDefault))) return;

            BlueprintActivatableAbilityConfigurator.From(BlueprintActivatableAbilityGuid.CombatExpertise)
                .SetIsOnByDefault(false)
                .Configure();
        }

        // Nauseated buff: remove Poison descriptors
        static void FixNauseatedPoisonDescriptor()
        {
            if (!StartBalancePatch("Nauseated Poison Descriptor", nameof(BalanceSettings.FixNauseatedPoisonDescriptor))) return;

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.Nauseated)
                .RemoveSpellDescriptor(SpellDescriptor.Poison)
                .Configure();
        }

        // Shatter Defenses should, by description, make the target flat-footed until the end of next round if hit while shaken or frightnened
        // In the base game, it instead makes the target flat footed for the current round if shaken or frightened
        // The fix applies a debuff on hit while shaken or frightened, and flat-footed only checks for the debuff (not the conditions)
        // This differs from the CotW fix, which double-checks the conditions for flat-footed
        // Depends on the RuleCheckTargetFlatFooted patch
        static void FixShatterDefenses()
        {
            if (!StartBalancePatch("Shatter Defenses", nameof(BalanceSettings.FixShatterDefenses), ModExclusionFlags.CallOfTheWild)) return;

            var shatterDefensesFeatureConfig = BlueprintFeatureConfigurator.From(BlueprintFeatureGuid.ShatterDefenses);
            BlueprintFeature shatterDefenses = shatterDefensesFeatureConfig.Instance;
            if (shatterDefenses == null)
                return;

            var shatterDefensesHitBuff = BlueprintBuffConfigurator.New
                (
                    BlueprintBuffGuid.KMBF_ShatterDefensesHit
                    , "ShatterDefensesHit"
                    , shatterDefenses.m_DisplayName
                    , shatterDefenses.m_Description
                    , shatterDefenses.m_Icon
                )
                .AddComponent<NewRoundTrigger>(n =>
                {
                    n.NewRoundActions = ActionListFactory.Single
                    (
                        MakeGameActionConditional
                        (
                            ConditionsCheckerFactory.Single(MakeBuffConditionCheckRoundNumber(3))
                            , ifTrue: ActionListFactory.Single(MakeContextActionRemoveSelf())
                        )
                    );
                })
                .SetStacking(StackingType.Stack)
                .Configure();

            var shatterDefensesAppliedThisRoundBuff = BlueprintBuffConfigurator
                .NewHidden(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, "ShatterDefensesAppliedThisRound")
                .AddComponent<NewRoundTrigger>(n =>
                {
                    n.NewRoundActions = ActionListFactory.Single(MakeContextActionRemoveSelf());
                })
                .SetStacking(StackingType.Stack)
                .Configure();

            shatterDefensesFeatureConfig.AddComponent<AddInitiatorAttackWithWeaponTrigger>(t =>
            {
                t.Action = ActionListFactory.Single
                (
                    MakeGameActionConditional
                    (
                        ConditionsCheckerFactory.From
                        (
                            Operation.And
                            , MakeContextConditionHasConditions([UnitCondition.Shaken, UnitCondition.Frightened], any: true)
                            , MakeContextConditionHasBuffFromCaster(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, not: true)
                        )
                        , ifTrue: ActionListFactory.From
                        (
                            MakeContextActionRemoveBuffFromCaster(BlueprintBuffGuid.KMBF_ShatterDefensesHit)
                            , MakeContextActionApplyUndispelableBuff(BlueprintBuffGuid.KMBF_ShatterDefensesHit, ContextDurationFactory.ConstantRounds(2))
                            , MakeContextActionApplyUndispelableBuff(BlueprintBuffGuid.KMBF_ShatterDefensesAppliedThisRound, ContextDurationFactory.ConstantRounds(1))
                        )

                    )
                );
                t.WaitForAttackResolve = true;
            })
            .Configure();
        }

        // Crane Wing should require a free hand to give its bonus
        // This means that 1) nothing should be equipped there 2) it shouldn't be used for any purpose (ex: wielding with two hands)
        // KM easily supports checking for a shield, but the rest is a bit of a mess without adding a full-on "Wield 1h weapon with Two Hands" toggle to turn off
        static void FixCraneWingHandCheck()
        {
            if (!StartBalancePatch("Crane Wing Free Hand", nameof(BalanceSettings.FixCraneWingRequirements))) return;

            BlueprintBuffConfigurator.From(BlueprintBuffGuid.CraneStyleWingBuff)
                .EditComponent<ACBonusAgainstAttacks>(c => c.NoShield = true)
                .Configure();
        }

        // Controlled Fireball has the default Target Type of "Enemy". This is fixed to "Any" in Wrath
        static void FixControlledFireball()
        {
            if (!StartBalancePatch("Controlled Fireball", nameof(BalanceSettings.FixControlledFireball))) return;

            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.ControlledFireball)
                .EditComponent<AbilityTargetsAround>(c => c.m_TargetType = TargetType.Any)
                .Configure();
        }
    }
}
