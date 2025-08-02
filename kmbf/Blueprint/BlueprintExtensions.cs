using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;

namespace kmbf.Blueprint
{
    static class BlueprintExtensions
    {
        public static bool Is(this BlueprintScriptableObject bp, BlueprintObjectGuid id)
        {
            return bp != null && bp.AssetGuid.Equals(id.guid);
        }

        public static bool GetDamageDiceRankConfig(this BlueprintAbilityGuid abilityId, out ContextRankConfig damageRankConfig)
        {
            if (!abilityId.GetBlueprint(out BlueprintAbility ability))
            {
                damageRankConfig = null;
                return false;
            }

            damageRankConfig = ability.ComponentsArray
                .Select(c => c as ContextRankConfig)
                .Where(c => c != null && c.Type == Kingmaker.Enums.AbilityRankType.DamageDice)
                .First();

            if (damageRankConfig == null)
            {
                Main.Log.Error($"Could not find damage dice rank config in ability blueprint '{abilityId.GetDebugName()}'");
                return false;
            }

            return true;
        }

        public static C GetComponentWhere<C>(this BlueprintScriptableObject bp, Predicate<C> pred)
        {
            return bp.Components.OfType<C>().FirstOrDefault(c => pred(c));
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

        public static bool Encompasses(this EventResult.MarginType Requirement, EventResult.MarginType Current)
        {
            switch(Requirement)
            {
                case EventResult.MarginType.GreatFail:
                case EventResult.MarginType.Fail:
                case EventResult.MarginType.Success:
                case EventResult.MarginType.GreatSuccess:
                    return Requirement == Current;
                case EventResult.MarginType.AnyFail:
                    return Current == EventResult.MarginType.GreatFail || Current == EventResult.MarginType.Fail || Current == EventResult.MarginType.AnyFail;
                case EventResult.MarginType.AnySuccess:
                    return Current == EventResult.MarginType.GreatSuccess || Current == EventResult.MarginType.Success || Current == EventResult.MarginType.AnySuccess;
            }

            throw new ArgumentException($"MarginType Requirement had invalid type {Requirement}");
        }
    }
}
