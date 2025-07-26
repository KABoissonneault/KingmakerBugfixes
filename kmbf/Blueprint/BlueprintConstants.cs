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
