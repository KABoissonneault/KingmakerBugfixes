using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;

namespace kmbf.Blueprint
{
    static class BlueprintExtensions
    {
        public static string GetDebugName(this BlueprintScriptableObject bp)
        {
            return $"Blueprint:{bp.AssetGuid}:{bp.name}";
        }

        public static string GetDebugName(this BlueprintAbility ability)
        {
            return $"BlueprintAbility:{ability.AssetGuid}:{ability.name}";
        }

        public static bool GetDamageDiceRankConfig(this BlueprintAbility ability, out ContextRankConfig damageRankConfig)
        {
            damageRankConfig = ability.ComponentsArray
                .Select(c => c as ContextRankConfig)
                .Where(c => c != null && c.Type == Kingmaker.Enums.AbilityRankType.DamageDice)
                .First();

            if (damageRankConfig == null)
            {
                Main.Log.Error($"Could not find damage dice rank config in ability blueprint '{ability.GetDebugName()}'");
                return false;
            }

            return true;
        }

        public static IEnumerable<GameAction> GetGameActionsRecursive(this ActionList actionList)
        {
            foreach (GameAction gameAction in actionList.Actions.EmptyIfNull())
            {
                yield return gameAction;

                if (gameAction is Conditional)
                {
                    var conditionalAction = (Conditional)gameAction;
                    foreach (GameAction trueAction in conditionalAction.IfTrue.GetGameActionsRecursive())
                    {
                        yield return trueAction;
                    }

                    foreach (GameAction falseAction in conditionalAction.IfFalse.GetGameActionsRecursive())
                    {
                        yield return falseAction;
                    }
                }
                else if (gameAction is ContextActionSavingThrow)
                {
                    var savingThrowAction = (ContextActionSavingThrow)gameAction;
                    foreach (GameAction action in savingThrowAction.Actions.GetGameActionsRecursive())
                    {
                        yield return action;
                    }
                }
                else if (gameAction is ContextActionConditionalSaved)
                {
                    var conditionalAction = (ContextActionConditionalSaved)gameAction;
                    foreach (GameAction trueAction in conditionalAction.Succeed.GetGameActionsRecursive())
                    {
                        yield return trueAction;
                    }

                    foreach (GameAction falseAction in conditionalAction.Failed.GetGameActionsRecursive())
                    {
                        yield return falseAction;
                    }
                }
            }
        }
    }
}
