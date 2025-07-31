using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
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

        public static T GetGameAction<T>(this ActionList actionList) where T : GameAction
        {
            return actionList.Actions.OfType<T>().FirstOrDefault();
        }

        public static IEnumerable<GameAction> GetGameActionsRecursive(this BlueprintComponent bpComponent)
        {
            switch(bpComponent)
            {
                case ActivateTrigger activateTrigger:
                    foreach (GameAction action in activateTrigger.Actions.GetGameActionsRecursive()) yield return action;
                    break;
            }
        }
            
        public static IEnumerable<GameAction> GetGameActionsRecursive(this ActionList actionList)
        {
            foreach (GameAction gameAction in actionList.Actions.EmptyIfNull())
            {
                yield return gameAction;

                switch (gameAction)
                {
                    case Conditional conditionalAction:
                        foreach (GameAction trueAction in conditionalAction.IfTrue.GetGameActionsRecursive()) yield return trueAction;
                        foreach (GameAction falseAction in conditionalAction.IfFalse.GetGameActionsRecursive()) yield return falseAction;
                        break;

                    case ContextActionSavingThrow savingThrowAction:
                        foreach (GameAction action in savingThrowAction.Actions.GetGameActionsRecursive()) yield return action;
                        break;

                    case ContextActionConditionalSaved conditionalSavedAction:
                        foreach (GameAction trueAction in conditionalSavedAction.Succeed.GetGameActionsRecursive()) yield return trueAction;
                        foreach (GameAction falseAction in conditionalSavedAction.Failed.GetGameActionsRecursive()) yield return falseAction;
                        break;

                    case RunActionHolder actionHolder:
                        foreach (GameAction action in actionHolder.Holder.Actions.GetGameActionsRecursive()) yield return action;
                        break;

                    case AlignmentSelector alignmentSelector:
                        foreach (GameAction action in alignmentSelector.LawfulGood.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.LawfulNeutral.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.LawfulEvil.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.NeutralGood.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.TrueNeutral.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.NeutralEvil.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.ChaoticGood.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.ChaoticNeutral.Action.GetGameActionsRecursive()) yield return action;
                        foreach (GameAction action in alignmentSelector.ChaoticEvil.Action.GetGameActionsRecursive()) yield return action;
                        break;
                }
            }
        }
    }
}
