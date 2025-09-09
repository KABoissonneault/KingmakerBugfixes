//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics;

namespace kmbf.Blueprint
{
    public static class Constants
    {
        public static class Empty
        {
            public static readonly ActionList Actions = new() { Actions = [] };

            public static readonly ContextDiceValue DiceValue = new()
            {
                DiceType = DiceType.Zero,
                DiceCountValue = 0,
                BonusValue = 0,
            };
        }
    }
}
