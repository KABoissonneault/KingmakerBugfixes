using Kingmaker.Blueprints.GameDifficulties;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.Quests.Common;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseElementConfigurator<T, TBuilder> : BaseObjectConfigurator<T, TBuilder>
        where T : Element
        where TBuilder : BaseElementConfigurator<T, TBuilder>
    {
        public BaseElementConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class BaseConditionConfigurator<T, TBuilder> : BaseElementConfigurator<T, TBuilder>
        where T : Condition
        where TBuilder : BaseConditionConfigurator<T, TBuilder>
    {
        public BaseConditionConfigurator(T instance)
            : base(instance)
        {

        }

        public BaseConditionConfigurator<T, TBuilder> SetNot(bool Not)
        {
            if (instance != null)
                instance.Not = Not;
            return this;
        }
    }

    public abstract class BaseContextConditionConfigurator<T, TBuilder> : BaseConditionConfigurator<T, TBuilder>
        where T : ContextCondition
        where TBuilder : BaseContextConditionConfigurator<T, TBuilder>
    {
        public BaseContextConditionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class BaseGameActionConfigurator<T, TBuilder> : BaseElementConfigurator<T, TBuilder>
        where T : GameAction
        where TBuilder : BaseGameActionConfigurator<T, TBuilder>
    {
        public BaseGameActionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class BaseContextActionConfigurator<T, TBuilder> : BaseGameActionConfigurator<T, TBuilder>
        where T : ContextAction
        where TBuilder : BaseContextActionConfigurator<T, TBuilder>
    {
        public BaseContextActionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public class ContextConditionDifficultyHigherThanConfigurator : BaseContextConditionConfigurator<ContextConditionDifficultyHigherThan, ContextConditionDifficultyHigherThanConfigurator>
    {
        public ContextConditionDifficultyHigherThanConfigurator(ContextConditionDifficultyHigherThan instance)
            : base(instance)
        {

        }

        public static ContextConditionDifficultyHigherThanConfigurator New(BlueprintGameDifficulty difficulty)
        {
            ContextConditionDifficultyHigherThan instance = CreateInstance();
            instance.CheckedDifficulty = difficulty;
            return new ContextConditionDifficultyHigherThanConfigurator(instance);
        }

        public ContextConditionDifficultyHigherThanConfigurator SetCheckedDifficulty(BlueprintGameDifficulty difficulty)
        {
            instance.CheckedDifficulty = difficulty;
            return this;
        }

        public ContextConditionDifficultyHigherThanConfigurator SetCheckOnlyForMonsterCaster(bool checkOnlyForMonsterCaster)
        {
            instance.CheckOnlyForMonsterCaster = checkOnlyForMonsterCaster;
            return this;
        }
    }

    public class ConditionalConfigurator : BaseGameActionConfigurator<Conditional, ConditionalConfigurator>
    {
        public ConditionalConfigurator(Conditional instance)
            : base(instance)
        {

        }

        public static ConditionalConfigurator New(ConditionsChecker conditionsChecker, ActionList ifTrue = null, ActionList ifFalse = null)
        {
            Conditional instance = CreateInstance();
            instance.ConditionsChecker = conditionsChecker;
            instance.IfTrue = ifTrue ?? Constants.Empty.Actions;
            instance.IfFalse = ifFalse ?? Constants.Empty.Actions;
            return new ConditionalConfigurator(instance);
        }

        public ConditionalConfigurator SetConditionsChecker(ConditionsChecker conditionsChecker)
        {
            instance.ConditionsChecker = conditionsChecker;
            return this;
        }

        public ConditionalConfigurator SetIfTrue(ActionList ifTrue)
        {
            instance.IfTrue = ifTrue;
            return this;
        }

        public ConditionalConfigurator SetIfFalse(ActionList ifFalse)
        {
            instance.IfFalse = ifFalse;
            return this;
        }
    }

    public class ContextActionDealDamageConfigurator : BaseContextActionConfigurator<ContextActionDealDamage, ContextActionDealDamageConfigurator>
    {
        public ContextActionDealDamageConfigurator(ContextActionDealDamage instance)
            : base(instance)
        {

        }

        public static ContextActionDealDamageConfigurator New(ContextActionDealDamage.Type Type, ContextDiceValue Value)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = Type;
            instance.Value = Value;
            return new ContextActionDealDamageConfigurator(instance);
        }

        public static ContextActionDealDamageConfigurator NewPermanentEnergyDrain(ContextDiceValue Value)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = ContextActionDealDamage­.Type.EnergyDrain;
            instance.EnergyDrainType = EnergyDrainType.Permanent;
            instance.Value = Value;
            return new ContextActionDealDamageConfigurator(instance);
        }

        public static ContextActionDealDamageConfigurator NewTemporaryEnergyDrain(ContextDiceValue value, ContextDurationValue duration)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = ContextActionDealDamage­.Type.EnergyDrain;
            instance.EnergyDrainType = EnergyDrainType.Temporary;
            instance.Value = value;
            instance.Duration = duration;
            return new ContextActionDealDamageConfigurator(instance);
        }

        public ContextActionDealDamageConfigurator SetType(ContextActionDealDamage.Type Type)
        {
            instance.m_Type = Type;
            return this;
        }

        public ContextActionDealDamageConfigurator SetEnergyDrainType(EnergyDrainType drainType)
        {
            instance.EnergyDrainType = drainType;
            return this;
        }

        public ContextActionDealDamageConfigurator SetValue(ContextDiceValue Value)
        {
            instance.Value = Value;
            return this;
        }
    }

    public class SetObjectiveStatusConfigurator : BaseGameActionConfigurator<SetObjectiveStatus, SetObjectiveStatusConfigurator>
    {
        public SetObjectiveStatusConfigurator(SetObjectiveStatus instance) 
            : base(instance)
        {

        }

        public static SetObjectiveStatusConfigurator New(BlueprintQuestObjectiveGuid objectiveId, SummonPoolCountTrigger.ObjectiveStatus objectiveStatus)
        {
            if(!objectiveId.GetBlueprint(out BlueprintQuestObjective objective))
            {
                return new(null);
            }

            SetObjectiveStatus instance = CreateInstance();
            instance.Objective = objective;
            instance.Status = objectiveStatus;
            return new(instance);
        }
    }

    public class GiveObjectiveConfigurator : BaseGameActionConfigurator<GiveObjective, GiveObjectiveConfigurator>
    {
        public GiveObjectiveConfigurator(GiveObjective instance) 
            : base(instance)
        {

        }

        public static GiveObjectiveConfigurator New(BlueprintQuestObjectiveGuid objectiveId)
        {
            if (!objectiveId.GetBlueprint(out BlueprintQuestObjective objective))
            {
                return new(null);
            }

            GiveObjective instance = CreateInstance();
            instance.Objective = objective;
            return new(instance);
        }
    }
}
