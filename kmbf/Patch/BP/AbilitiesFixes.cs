using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.TargetCheckers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.UnitLogic.Parts;
using kmbf.Action;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;
using kmbf.Component;
using static kmbf.Blueprint.Builder.ElementBuilder;
using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch.BP
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
            FixBreakEnchantment();
            FixFieryBody();

            FixLeopardCompanionUpgrade();
            FixGazeImmunities();

            FixTieflingFoulspawn();
            FixExplosionRing();
            
            
            FixAbilityScoreCheckBonuses();
            FixLawChaosGreaterAbility();
            FixUndeadImmunities();
            FixSpellDescriptors();

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
            BlueprintFeatureConfigurator.From(FeatureRefs.EkunWolfOffensiveMaster)
                .ReplaceAllComponentsWithSource(FeatureRefs.EkunWolfOffensiveBuff)
                .Configure();

            BlueprintFeatureConfigurator.From(FeatureRefs.EkunWolfDefensiveMaster)
                .ReplaceAllComponentsWithSource(FeatureRefs.EkunWolfDefensiveBuff)
                .Configure();
        }

        static void FixMagicalVestmentShield()
        {
            if (!StartPatch("Magical Vestment Shield", ModExclusionFlags.CallOfTheWild)) return;

            // Magical Vestment: Make the Shield version as Shield Enhancement rather than pure Shield AC
            BlueprintBuffConfigurator.From(BuffRefs.MagicalVestmentShield)
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

            BlueprintAbilityConfigurator.From(AbilityRefs.RaiseDead)
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

            if (!AbilityRefs.RaiseDead.GetBlueprint(out BlueprintAbility raiseDead)) return;

            BlueprintAbilityConfigurator.From(AbilityRefs.RaiseDead_Cutscene)
                .SetDisplayName(raiseDead.m_DisplayName)
                .SetDescription(raiseDead.m_Description)
                .SetIcon(raiseDead.m_Icon)
                .Configure();

            BlueprintCueConfigurator.From(CueRefs.LKBattle_Phase5_Cue_0065)
                .EditOnStopActionRecursiveWhere<EvaluatedTrapCastSpell>(pred: null, editAction: a =>
                {
                    if (!AbilityRefs.RaiseDead_Cutscene.GetBlueprint(out BlueprintAbility raiseDead_Cutscene)) return;

                    a.Spell = raiseDead_Cutscene;
                })
                .Configure();
        }

        // Breath of Life does not actually give one negative level when used to resurrect
        // Like in Wrath of the Righteous, we add a condition on whether Enemy Stats Adjustment is Normal or above
        static void FixBreathOfLife()
        {
            if (!StartPatch("Breath of Life")) return;

            BlueprintAbilityConfigurator.From(AbilityRefs.BreathOfLifeTouch)
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

            BlueprintAbilityConfigurator.From(AbilityRefs.JoyfulRapture)
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

            BlueprintAbilityConfigurator.From(AbilityRefs.ProtectionFromArrowsCommunal)
                .SetSpellResistance(false)
                .Configure();

            BlueprintBuffConfigurator.From(BuffRefs.ProtectionFromArrows)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.name = "$AddDamageResistancePhysical$ddda39fa-685e-46e7-8517-f02635766e13";
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

            BlueprintBuffConfigurator.From(BuffRefs.ProtectionFromArrowsCommunal)
                .RemoveComponents<DRAgainstRangedWithPool>()
                .AddComponent<AddDamageResistancePhysical>(c =>
                {
                    c.name = "$AddDamageResistancePhysical$86197669-0d2c-407d-be88-d72b4bed5263";
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

            BlueprintObjectConfigurator.From(FeatureRefs.AnimalCompanionUpgradeLeopard)
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

            BlueprintAbilityAreaEffectConfigurator.From(AbilityAreaEffectRefs.BalefulGaze)
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

            BlueprintFeatureConfigurator.From(FeatureRefs.TieflingHeritageFoulspawn)
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

            BlueprintFeatureConfigurator.From(FeatureRefs.ExplosionRing)
                .AddComponent<AdditionalBonusOnDamage>(c =>
                {
                    c.BonusOnDamage = 12;
                    c.CheckSpellDescriptor = true;
                    c.SpellDescriptorsList = SpellDescriptor.Bomb;
                })
                .Configure();

            BlueprintAbilityConfigurator.From(AbilityRefs.AlchemistFire)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();

            BlueprintAbilityConfigurator.From(AbilityRefs.AcidFlask)
                .AddSpellDescriptor(SpellDescriptor.Bomb)
                .Configure();
        }

        // Break Enchantment should remove effects "that can be dispelled by 'stone to flesh'" in tabletop
        // The in-game description agrees that it should remove transmutations
        // So I'm bringing the "remove petrify effects" from WotR
        static void FixBreakEnchantment()
        {
            if (!StartPatch("Break Enchantment")) return;

            BlueprintAbilityConfigurator.From(AbilityRefs.BreakEnchantment)
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
        
        // The "Fiery Body" weapon enchantment is supposed to add 3d6 to the "claws", not 1d6 like normal Flaming
        // Description (23d1f690-1f1a-429e-b111-47a3309634fd) fixed in DefaultStrings_enGB.json
        static void FixFieryBody()
        {
            if(StartPatch("Fiery Body"))
            {
                BlueprintWeaponEnchantmentConfigurator.From(WeaponEnchantmentRefs.FieryBody)
                    .EditComponent<WeaponEnergyDamageDice>(c =>
                    {
                        c.EnergyDamageDice = DiceFormulaFactory.Value(DiceType.D6, 3);
                    })
                    .Configure();
            }
        }

        // While AbilityScoreCheckBonus has been fixed by this mod,
        // it's still not ideal for skill bonuses from a user perspective, because those
        // bonuses are invisible in the character sheet.
        // Change to an AddContextStatBonus for similar results
        static void FixAbilityScoreCheckBonuses()
        {
            if (StartPatch("Strength Surge"))
            {
                BlueprintBuffConfigurator.From(BuffRefs.StrengthSurge)
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
                BlueprintFeatureConfigurator.From(FeatureRefs.AnimalDomainBaseFeature)
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

        // Both Staff of Order and Chaos Blade fail to do anything on use.
        // Give them a ContextActionEnchantWornItem like Scythe of Evil and Holy Lance
        static void FixLawChaosGreaterAbility()
        {
            if (StartPatch("Staff of Order"))
            {
                BlueprintAbilityConfigurator.From(AbilityRefs.LawDomainGreaterAbility)
                    .RemoveAbilityEffectRunActionsWhere(a => a == null)
                    .AddAbilityEffectRunAction(
                        ContextActionEnchantWornItemConfigurator.New(WeaponEnchantmentRefs.Axiomatic)
                            .SetSlot(EquipSlotBase.SlotType.PrimaryHand)
                            .SetDuration(ContextDurationFactory.RankRounds())
                            .Configure()
                        )
                    .Configure();
            }

            if (StartPatch("Chaos Blade"))
            {
                BlueprintAbilityConfigurator.From(AbilityRefs.ChaosDomainGreaterAbility)
                    .RemoveAbilityEffectRunActionsWhere(a => a == null)
                    .AddAbilityEffectRunAction(
                        ContextActionEnchantWornItemConfigurator.New(WeaponEnchantmentRefs.Anarchic)
                            .SetSlot(EquipSlotBase.SlotType.PrimaryHand)
                            .SetDuration(ContextDurationFactory.RankRounds())
                            .Configure()
                        )
                    .Configure();
            }
        }

        static void FixUndeadImmunities()
        {
            // Undead don't take Charisma modifiers into account for saving throws
            // Paladin's Divine Grace uses a RecalculateOnStatChange, so we copy this approach here
            if (StartPatch("Undead Saving Throws", ModExclusionFlags.CallOfTheWild))
            {
                BlueprintFeatureConfigurator.From(FeatureRefs.UndeadImmunities)
                    .AddComponent<RecalculateOnStatChange>(c =>
                    {
                        c.Stat = StatType.Charisma;
                    })
                    .Configure();
            }

            // Undead have too many immunities compared to their description, and too many are added conditionally on the caster not having the Undead Bloodline arcana
            // This patch tries to resolve this mess by limiting unconditional immunities to the ones in the description (matching tabletop),
            // and having MindAffecting as the only conditional one
            // We also need to go and make sure spells targeting humanoids don't target undead unless the undead bloodline arcana is present
            // In short,
            // Added immunities: Paralysis
            // Removed immunities: Shaken, Frightened, Sickened, Nauseated
            // Immunities made unconditional: Sleep
            //
            // Fortitude spells handled: Baleful Polymorph, Flare, Flare Burst, Ray of Sickening
            if (StartBalancePatch("Construct and Undead Immunities", nameof(BalanceSettings.FixConstructUndeadImmunities), ModExclusionFlags.CallOfTheWild))
            {
                if (!FeatureRefs.ConstructType.GetBlueprint(out BlueprintFeature constructType)) return;
                if (!FeatureRefs.UndeadType.GetBlueprint(out BlueprintFeature undeadType)) return;

                SpellDescriptorWrapper unconditionalDescriptor = SpellDescriptor.Bleed | SpellDescriptor.Death | SpellDescriptor.Disease | SpellDescriptor.Fatigue | SpellDescriptor.Paralysis
                            | SpellDescriptor.Poison | SpellDescriptor.Sleep | SpellDescriptor.Stun | SpellDescriptor.VilderavnBleed;
                SpellDescriptorWrapper conditionalDescriptor = SpellDescriptor.MindAffecting;
                BlueprintFeatureConfigurator.From(FeatureRefs.UndeadImmunities)
                    // TODO: fix saving throws for Swarm distraction before removing the Nauseated/Sickened immunity
                    .AddComponent<AddConditionImmunity>(c =>
                    {
                        c.Condition = UnitCondition.Exhausted;
                    })
                    .AddComponent<AddConditionImmunity>(c =>
                    {
                        c.Condition = UnitCondition.Paralyzed;
                    })
                    .AddComponent<AddConditionImmunity>(c =>
                    {
                        c.Condition = UnitCondition.Stunned;
                    })
                    .EditComponentWithName<BuffDescriptorImmunity>("$BuffDescriptorImmunity$eb929088-4f9e-4c60-92ee-89a0fa13d8f1", c =>
                    {
                        c.Descriptor = unconditionalDescriptor;
                    })
                    .EditComponentWithName<BuffDescriptorImmunity>("$BuffDescriptorImmunity$d4fb14f4-7d7b-45b3-ab7f-d7eb6f9f7a63", c =>
                    {
                        c.Descriptor = conditionalDescriptor;
                    })
                    .EditComponentWithName<SpellImmunityToSpellDescriptor>("$SpellImmunityToSpellDescriptor$c0976aae-8934-4994-9b1a-f5614f7d4f26", c =>
                    {
                        c.Descriptor = unconditionalDescriptor;
                    })
                    .EditComponentWithName<SpellImmunityToSpellDescriptor>("$SpellImmunityToSpellDescriptor$fb56d182-0078-4f5e-a1dd-5730215f7e72", c =>
                    {
                        c.Descriptor = conditionalDescriptor;
                    })
                    .Configure();

                void AddUndeadConstructCheck(BlueprintAbilityGuid ability)
                {
                    if (!ability.GetBlueprint(out BlueprintAbility bp)) return;

                    BlueprintAbilityConfigurator.From(ability)
                        .AddComponent<AbilityTargetHasFact>(c =>
                        {
                            c.CheckedFacts = [constructType, undeadType];
                            c.Inverted = true;
                        })
                        .Configure();
                }

                // Removes the "unless undead blood arcana" to be an unconditional untargetable
                // Used for Paralysis abilities
                void MoveUndeadCheck(BlueprintAbilityGuid ability)
                {
                    if (!ability.GetBlueprint(out BlueprintAbility bp)) return;

                    BlueprintAbilityConfigurator.From(ability)
                        .RemoveComponentWhere<AbilityTargetHasNoFactUnless>(c => c.CheckedFacts.Length == 1 && c.CheckedFacts[0] == undeadType)
                        .EditComponentWhere<AbilityTargetHasFact>(c => c.Inverted && c.CheckedFacts.Contains(constructType), c =>
                        {
                            c.CheckedFacts = [.. c.CheckedFacts, undeadType];
                        })
                        .Configure();
                }

                AddUndeadConstructCheck(AbilityRefs.BalefulPolymorph);
                AddUndeadConstructCheck(AbilityRefs.Flare);
                AddUndeadConstructCheck(AbilityRefs.FlareBurst);
                AddUndeadConstructCheck(AbilityRefs.RayOfSickening);
                AddUndeadConstructCheck(AbilityRefs.LostlandKeep_TransmutationTrap);
                MoveUndeadCheck(AbilityRefs.HoldPerson);
                MoveUndeadCheck(AbilityRefs.HoldPersonAasimar);
                MoveUndeadCheck(AbilityRefs.HoldMonster);
            }
        }

        // A bunch of spells have bad descriptors, let's do a bunch of them here
        static void FixSpellDescriptors()
        {
            if (!StartPatch("Spell Descriptors")) return;

            BlueprintAbilityConfigurator.From(AbilityRefs.Daze).AddSpellDescriptor(SpellDescriptor.Daze).Configure();
            BlueprintBuffConfigurator.From(BuffRefs.Daze).RemoveSpellDescriptor(SpellDescriptor.Stun).Configure();
        }

        // Touch of Glory adds +1-10 Charisma instead of adding +1-10 to Charisma checks
        // Fortunately, this mod fixed AbilityScoreCheckBonus, so we can use that instead
        static void FixTouchOfGlory()
        {
            if (!StartBalancePatch("Touch of Glory", nameof(BalanceSettings.FixTouchOfGlory))) return;

            BlueprintBuffConfigurator.From(BuffRefs.TouchOfGlory)
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

            BlueprintActivatableAbilityConfigurator.From(ActivatableAbilityRefs.CombatExpertise)
                .SetIsOnByDefault(false)
                .Configure();
        }

        // Nauseated buff: remove Poison descriptors
        static void FixNauseatedPoisonDescriptor()
        {
            if (!StartBalancePatch("Nauseated Poison Descriptor", nameof(BalanceSettings.FixNauseatedPoisonDescriptor))) return;

            BlueprintBuffConfigurator.From(BuffRefs.Nauseated)
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

            var shatterDefensesFeatureConfig = BlueprintFeatureConfigurator.From(FeatureRefs.ShatterDefenses);
            BlueprintFeature shatterDefenses = shatterDefensesFeatureConfig.Instance;
            if (shatterDefenses == null)
                return;

            var shatterDefensesHitBuff = BlueprintBuffConfigurator.New
                (
                    FixBuffRefs.ShatterDefensesHit
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
                .NewHidden(FixBuffRefs.ShatterDefensesAppliedThisRound, "ShatterDefensesAppliedThisRound")
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
                            , MakeContextConditionHasBuffFromCaster(FixBuffRefs.ShatterDefensesAppliedThisRound, not: true)
                        )
                        , ifTrue: ActionListFactory.From
                        (
                            MakeContextActionRemoveBuffFromCaster(FixBuffRefs.ShatterDefensesHit)
                            , MakeContextActionApplyUndispelableBuff(FixBuffRefs.ShatterDefensesHit, ContextDurationFactory.ConstantRounds(2))
                            , MakeContextActionApplyUndispelableBuff(FixBuffRefs.ShatterDefensesAppliedThisRound, ContextDurationFactory.ConstantRounds(1))
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

            BlueprintBuffConfigurator.From(BuffRefs.CraneStyleWingBuff)
                .EditComponent<ACBonusAgainstAttacks>(c => c.NoShield = true)
                .Configure();
        }

        // Controlled Fireball has the default Target Type of "Enemy". This is fixed to "Any" in Wrath
        static void FixControlledFireball()
        {
            if (!StartBalancePatch("Controlled Fireball", nameof(BalanceSettings.FixControlledFireball))) return;

            BlueprintAbilityConfigurator.From(AbilityRefs.ControlledFireball)
                .EditComponent<AbilityTargetsAround>(c => c.m_TargetType = TargetType.Any)
                .Configure();
        }
    }
}
