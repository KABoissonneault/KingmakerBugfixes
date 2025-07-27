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
            return Value(DiceType.Zero, diceCount: null, bonus: value);
        }
    }

    public static class ContextDurationFactory
    {
        public static ContextDurationValue ConstantDays(int value)
        {
            return new() { Rate = DurationRate.Days, DiceCountValue = 0, BonusValue = value };
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
