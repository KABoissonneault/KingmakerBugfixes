//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using Kingmaker.ElementsSystem;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics;

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

    public static class ContextValueFactory
    {
        public static ContextValue Simple(int value)
        {
            return new() { Value = value };
        }
        
        // Actual values derived from ContextRankConfig component
        public static ContextValue Rank(AbilityRankType rankType = AbilityRankType.Default)
        {
            return new() { ValueType = ContextValueType.Rank, ValueRank = rankType };
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

        public static ContextDurationValue RankRounds(AbilityRankType rankType = AbilityRankType.Default)
        {
            return new() { Rate = DurationRate.Rounds, DiceCountValue = 0, BonusValue = ContextValueFactory.Rank(rankType) };
        }

        public static ContextDurationValue RankTenMinutes(AbilityRankType rankType = AbilityRankType.Default)
        {
            return new() { Rate = DurationRate.TenMinutes, DiceCountValue = 0, BonusValue = ContextValueFactory.Rank(rankType) };
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

        public static ConditionsChecker WithCondition(ConditionsChecker current, Condition condition)
        {
            return new() { Operation = current.Operation, Conditions = [.. current.Conditions, condition] };
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
