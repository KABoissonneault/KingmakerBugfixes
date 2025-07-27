using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace kmbf.Blueprint.Configurator
{
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
            if (instance != null)
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
}
