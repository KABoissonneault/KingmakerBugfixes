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

        public static ContextDurationValue ConstantRounds(int value)
        {
            return new() { Rate = DurationRate.Rounds, DiceCountValue = 0, BonusValue = value };
        }
    }

    public static class ConditionsCheckerFactory
    {
        public static ConditionsChecker Single(Condition condition)
        {
            return new() { Conditions = [condition] };
        }

        public static ConditionsChecker From(Operation operation, params Condition[] conditions)
        {
            return new() { Operation = operation, Conditions =  conditions };
        }
    }

    public static class ActionListFactory
    {
        public static ActionList Enumerable(IEnumerable<GameAction> actions)
        {
            return new() { Actions = actions.ToArray() };
        }

        public static ActionList Single(GameAction action)
        {
            return new() { Actions = [action] };
        }

        public static ActionList Add(ActionList current, GameAction action)
        {
            return new() { Actions = [.. current.Actions, action] };
        }

        public static ActionList From(params GameAction[] actions)
        {
            return new() { Actions = actions };
        }
    }
}
