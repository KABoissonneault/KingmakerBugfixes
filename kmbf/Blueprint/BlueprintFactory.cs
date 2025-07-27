using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace kmbf.Blueprint
{
    public static class ContextDiceFactory
    {
        // Null dice count defaults to 1
        public static ContextDiceValue Value(DiceType diceType, ContextValue diceCount = null, ContextValue bonus = null)
        {
            return new() { DiceType = diceType, DiceCountValue = diceCount ?? 1, BonusValue = bonus ?? 0 };
        }

        // 0d + value
        public static ContextDiceValue BonusConstant(int value)
        {
            return Value(DiceType.Zero, diceCount: null, bonus: ContextValueFactory.Constant(value));
        }
    }

    public static class ContextValueFactory
    {
        public static ContextValue Constant(int value)
        {
            return new() { ValueType = ContextValueType.Simple, Value = value };
        }
    }

    public static class ConditionsCheckerFactory
    {
        public static ConditionsChecker Single(Condition condition)
        {
            return new() { Conditions = [condition] };
        }
    }

    public static class ActionListFactory
    {
        public static ActionList Enumerable(IEnumerable<GameAction> actions)
        {
            return new() { Actions = actions.ToArray() };
        }

        public static ActionList From(params GameAction[] actions)
        {
            return new() { Actions = actions };
        }
    }
}
