using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.GameDifficulties;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using UnityEngine;

namespace kmbf.Blueprint
{
    public abstract class ObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : ObjectConfigurator<T, TBuilder>
    {
        protected T instance;
        protected TBuilder Self => (TBuilder)this;

        public ObjectConfigurator(T instance)
        {
            this.instance = instance;
        }

        public T Configure()
        {
            return instance;
        }

        protected static T CreateInstance()
        {
            var instance = ScriptableObject.CreateInstance<T>();
            UnityEngine.Object.DontDestroyOnLoad(instance);
            return instance;
        }
    }

    public abstract class ElementConfigurator<T, TBuilder> : ObjectConfigurator<T, ElementConfigurator<T, TBuilder>> 
        where T : Element
        where TBuilder : ElementConfigurator<T, TBuilder>
    {
        public ElementConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class ConditionConfigurator<T, TBuilder> : ElementConfigurator<T, TBuilder>
        where T : Condition
        where TBuilder : ConditionConfigurator<T, TBuilder>
    {
        public ConditionConfigurator(T instance)
            : base(instance)
        {

        }

        public ConditionConfigurator<T, TBuilder> SetNot(bool Not)
        {
            instance.Not = Not;
            return this;
        }
    }

    public abstract class ContextConditionConfigurator<T, TBuilder> : ConditionConfigurator<T, TBuilder>
        where T : ContextCondition
        where TBuilder : ContextConditionConfigurator<T, TBuilder>
    {
        public ContextConditionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class GameActionConfigurator<T, TBuilder> : ElementConfigurator<T, TBuilder>
        where T : GameAction
        where TBuilder : GameActionConfigurator<T, TBuilder>
    {
        public GameActionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class ContextActionConfigurator<T, TBuilder> : GameActionConfigurator<T, TBuilder>
        where T : ContextAction
        where TBuilder : ContextActionConfigurator<T, TBuilder>
    {
        public ContextActionConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class BlueprintObjectConfigurator<T, TBuilder> : ObjectConfigurator<T, TBuilder>
        where T : BlueprintScriptableObject
        where TBuilder : BlueprintObjectConfigurator<T, TBuilder>
    {
        public BlueprintObjectConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public abstract class BlueprintFactConfigurator<T, TBuilder> : BlueprintObjectConfigurator<T, TBuilder>
        where T : BlueprintFact
        where TBuilder : BlueprintFactConfigurator<T, TBuilder>
    {
        public BlueprintFactConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public class BlueprintUnitFactConfigurator<T, TBuilder> : BlueprintFactConfigurator<T, TBuilder>
        where T : BlueprintUnitFact
        where TBuilder : BlueprintFactConfigurator<T, TBuilder>
    {
        public BlueprintUnitFactConfigurator(T instance)
            : base(instance)
        {

        }

        public TBuilder SetIcon(Sprite icon)
        {
            instance.m_Icon = icon;
            return Self;
        }
    }

    public class BlueprintBuffConfigurator : BlueprintUnitFactConfigurator<BlueprintBuff, BlueprintBuffConfigurator>
    {
        public BlueprintBuffConfigurator(BlueprintBuff instance) 
            : base(instance)
        {

        }

        public static BlueprintBuffConfigurator From(BlueprintBuff instance)
        {
            return new(instance);
        }
    }

    public class ContextConditionDifficultyHigherThanConfigurator : ContextConditionConfigurator<ContextConditionDifficultyHigherThan, ContextConditionDifficultyHigherThanConfigurator>
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

    public class ConditionalConfigurator : GameActionConfigurator<Conditional, ConditionalConfigurator>
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

    public class ContextActionDealDamageConfigurator : ContextActionConfigurator<ContextActionDealDamage, ContextActionDealDamageConfigurator>
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

        public static ContextActionDealDamageConfigurator NewEnergyDrain(EnergyDrainType drainType, ContextDiceValue Value)
        {
            ContextActionDealDamage instance = CreateInstance();
            instance.m_Type = ContextActionDealDamage­.Type.EnergyDrain;
            instance.EnergyDrainType = drainType;
            instance.Value = Value;
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
}
