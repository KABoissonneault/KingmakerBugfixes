using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.GameDifficulties;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Quests.Common;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom.Actions;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseElementConfigurator<T, TBuilder> : BaseObjectConfigurator<T, TBuilder>
        where T : Element
        where TBuilder : BaseElementConfigurator<T, TBuilder>, new()
    {

    }

    public abstract class BaseConditionConfigurator<T, TBuilder> : BaseElementConfigurator<T, TBuilder>
        where T : Condition
        where TBuilder : BaseConditionConfigurator<T, TBuilder>, new()
    {
        public TBuilder SetNot(bool Not)
        {
            return AddOperation(i => { i.Not = Not; });
        }
    }

    public abstract class BaseContextConditionConfigurator<T, TBuilder> : BaseConditionConfigurator<T, TBuilder>
        where T : ContextCondition
        where TBuilder : BaseContextConditionConfigurator<T, TBuilder>, new()
    {

    }

    public abstract class BaseGameActionConfigurator<T, TBuilder> : BaseElementConfigurator<T, TBuilder>
        where T : GameAction
        where TBuilder : BaseGameActionConfigurator<T, TBuilder>, new()
    {

    }

    public abstract class BaseContextActionConfigurator<T, TBuilder> : BaseGameActionConfigurator<T, TBuilder>
        where T : ContextAction
        where TBuilder : BaseContextActionConfigurator<T, TBuilder>, new()
    {

    }

    public class ContextConditionDifficultyHigherThanConfigurator : BaseContextConditionConfigurator<ContextConditionDifficultyHigherThan, ContextConditionDifficultyHigherThanConfigurator>
    {
        public static ContextConditionDifficultyHigherThanConfigurator New(BlueprintGameDifficulty difficulty)
        {
            ContextConditionDifficultyHigherThan instance = CreateInstance();
            instance.CheckedDifficulty = difficulty;
            return From(instance);
        }

        public ContextConditionDifficultyHigherThanConfigurator SetCheckedDifficulty(BlueprintGameDifficulty difficulty)
        {
            return AddOperation(i => 
            { 
                i.CheckedDifficulty = difficulty; 
            });
        }

        public ContextConditionDifficultyHigherThanConfigurator SetCheckOnlyForMonsterCaster(bool checkOnlyForMonsterCaster)
        {
            return AddOperation(i =>
            {
                i.CheckOnlyForMonsterCaster = checkOnlyForMonsterCaster;
            });;
        }
    }

    public class ConditionalConfigurator : BaseGameActionConfigurator<Conditional, ConditionalConfigurator>
    {
        public ConditionalConfigurator SetConditionsChecker(ConditionsChecker conditionsChecker)
        {
            return AddOperation(i =>
            {
                i.ConditionsChecker = conditionsChecker;
            });
        }

        public ConditionalConfigurator SetIfTrue(ActionList ifTrue)
        {
            return AddOperation(i =>
            {
                i.IfTrue = ifTrue;
            });
        }

        public ConditionalConfigurator SetIfFalse(ActionList ifFalse)
        {
            return AddOperation(i =>
            {
                i.IfFalse = ifFalse;
            });
        }
    }

    public class ContextActionDealDamageConfigurator : BaseContextActionConfigurator<ContextActionDealDamage, ContextActionDealDamageConfigurator>
    {
        public static ContextActionDealDamageConfigurator New(ContextActionDealDamage.Type Type, ContextDiceValue Value)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = Type;
            instance.Value = Value;
            return From(instance);
        }

        public static ContextActionDealDamageConfigurator NewPermanentEnergyDrain(ContextDiceValue Value)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = ContextActionDealDamage­.Type.EnergyDrain;
            instance.EnergyDrainType = EnergyDrainType.Permanent;
            instance.Value = Value;
            return From(instance);
        }

        public static ContextActionDealDamageConfigurator NewTemporaryEnergyDrain(ContextDiceValue value, ContextDurationValue duration)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = ContextActionDealDamage­.Type.EnergyDrain;
            instance.EnergyDrainType = EnergyDrainType.Temporary;
            instance.Value = value;
            instance.Duration = duration;
            return From(instance);
        }

        public ContextActionDealDamageConfigurator SetType(ContextActionDealDamage.Type Type)
        {
            return AddOperation(i =>
            {
                i.m_Type = Type;
            });
        }

        public ContextActionDealDamageConfigurator SetEnergyDrainType(EnergyDrainType drainType)
        {
            return AddOperation(i =>
            {
                i.EnergyDrainType = drainType;
            });
        }

        public ContextActionDealDamageConfigurator SetValue(ContextDiceValue Value)
        {
            return AddOperation(i =>
            {
                i.Value = Value;
            });
        }
    }

    public class ContextActionDispelMagicConfigurator : BaseContextActionConfigurator<ContextActionDispelMagic,  ContextActionDispelMagicConfigurator>
    {
        public static ContextActionDispelMagicConfigurator New(ContextActionDispelMagic.BuffType type)
        {
            ContextActionDispelMagic instance = CreateInstance();
            instance.m_BuffType = type;
            instance.m_MaxSpellLevel = ContextValueFactory.Simple(0);
            instance.m_MaxCasterLevel = ContextValueFactory.Simple(0);
            instance.OnSuccess = new ActionList();
            instance.OnFail = new ActionList();
            return From(instance);
        }

        public ContextActionDispelMagicConfigurator SetDescriptor(SpellDescriptorWrapper descriptor)
        {
            return AddOperation(i => { i.Descriptor = descriptor; });
        }

        public ContextActionDispelMagicConfigurator EditOnSuccess(Action<ActionList> action)
        {
            return AddOperation(i =>
            {
                action(i.OnSuccess);
            });
        }

        public ContextActionDispelMagicConfigurator AddOnSuccessAction(GameAction action)
        {
            return AddOperation(i =>
            {
                i.OnSuccess = ActionListFactory.Add(i.OnSuccess, action);
            });
        }
    }

    public class SetObjectiveStatusConfigurator : BaseGameActionConfigurator<SetObjectiveStatus, SetObjectiveStatusConfigurator>
    {
        public static SetObjectiveStatusConfigurator New(BlueprintQuestObjectiveGuid objectiveId, SummonPoolCountTrigger.ObjectiveStatus objectiveStatus)
        {
            if(!objectiveId.GetBlueprint(out BlueprintQuestObjective objective))
            {
                return new();
            }

            SetObjectiveStatus instance = CreateInstance();
            instance.Objective = objective;
            instance.Status = objectiveStatus;
            return From(instance);
        }
    }

    public class GiveObjectiveConfigurator : BaseGameActionConfigurator<GiveObjective, GiveObjectiveConfigurator>
    {        
        public static GiveObjectiveConfigurator New(BlueprintQuestObjectiveGuid objectiveId)
        {
            if (!objectiveId.GetBlueprint(out BlueprintQuestObjective objective))
            {
                return new();
            }

            GiveObjective instance = CreateInstance();
            instance.Objective = objective;
            return From(instance);
        }
    }

    public sealed class UnlockFlagConfigurator : BaseGameActionConfigurator<UnlockFlag, UnlockFlagConfigurator>
    {
        public static UnlockFlagConfigurator New(BlueprintUnlockableFlagGuid flagId, int value = 0)
        {
            if (!flagId.GetBlueprint(out BlueprintUnlockableFlag flag)) return new();

            UnlockFlag instance = CreateInstance();
            instance.flag = flag;
            instance.flagValue = value;
            return From(instance);
        }
    }

    public class BaseKingdomActionConfigurator<T, TBuilder> : BaseGameActionConfigurator<T, TBuilder>
        where T : KingdomAction
        where TBuilder : BaseKingdomActionConfigurator<T, TBuilder>, new()
    {

    }

    public class KingdomActionAddBuffConfigurator : BaseGameActionConfigurator<KingdomActionAddBuff, KingdomActionAddBuffConfigurator>
    {
        public static KingdomActionAddBuffConfigurator New(BlueprintKingdomBuffGuid buffId)
        {
            if (!buffId.GetBlueprint(out BlueprintKingdomBuff buff))
            {
                return new();
            }

            KingdomActionAddBuff instance = CreateInstance();
            instance.Blueprint = buff;
            return From(instance);
        }

        public static KingdomActionAddBuffConfigurator NewRegional(BlueprintKingdomBuffGuid buffId, BlueprintRegionGuid regionId)
        {
            if (!buffId.GetBlueprint(out BlueprintKingdomBuff buff))
            {
                return new();
            }

            if (!regionId.GetBlueprint(out BlueprintRegion region))
            {
                return new();
            }

            KingdomActionAddBuff instance = CreateInstance();
            instance.Blueprint = buff;
            instance.ApplyToRegion = true;
            instance.Region = region;
            return From(instance);
        }
    }
}
