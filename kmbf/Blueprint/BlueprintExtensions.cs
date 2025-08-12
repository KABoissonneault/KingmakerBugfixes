using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Events;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
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
            switch (bpComponent)
            {
                case ActivateTrigger activateTrigger:
                    foreach (GameAction action in activateTrigger.Actions.GetGameActionsRecursive()) yield return action;
                    break;

                case AbilityEffectRunAction runAction:
                    foreach (GameAction action in runAction.Actions.GetGameActionsRecursive()) yield return action;
                    break;
            }
        }

        public static IEnumerable<ActionList> GetChildActionLists(this GameAction action)
        {
            switch (action)
            {
                case Conditional conditionalAction:
                    return [conditionalAction.IfTrue, conditionalAction.IfFalse];

                case ContextActionSavingThrow savingThrowAction:
                    return [savingThrowAction.Actions];

                case ContextActionConditionalSaved conditionalSavedAction:
                    return [conditionalSavedAction.Succeed, conditionalSavedAction.Failed];

                case RunActionHolder actionHolder:
                    return [actionHolder.Holder.Actions];

                case AlignmentSelector alignmentSelector:
                    return [
                        alignmentSelector.LawfulGood.Action,
                        alignmentSelector.LawfulNeutral.Action,
                        alignmentSelector.LawfulEvil.Action,
                        alignmentSelector.NeutralGood.Action,
                        alignmentSelector.TrueNeutral.Action,
                        alignmentSelector.NeutralEvil.Action,
                        alignmentSelector.ChaoticGood.Action,
                        alignmentSelector.ChaoticNeutral.Action,
                        alignmentSelector.ChaoticEvil.Action
                        ];

                case ContextActionRandomize randomizeAction:
                    return randomizeAction.m_Actions.Select(w => w.Action);

                default:
                    return Enumerable.Empty<ActionList>();
            }
        }

        public static IEnumerable<GameAction> GetChildActions(this GameAction action)
        {
            foreach (ActionList list in action.GetChildActionLists())
                foreach (GameAction childAction in list.Actions)
                    yield return childAction;
        }

        public static IEnumerable<GameAction> GetGameActionsRecursive(this ActionList actionList)
        {
            foreach (GameAction gameAction in actionList.Actions.EmptyIfNull())
            {
                yield return gameAction;

                foreach (ActionList childList in gameAction.GetChildActionLists())
                    foreach (GameAction childAction in childList.GetGameActionsRecursive())
                        yield return childAction;
            }
        }

        public static bool Encompasses(this EventResult.MarginType Requirement, EventResult.MarginType Current)
        {
            switch (Requirement)
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

        public static void SetMultiplyByModifier(this ContextRankConfig config, int step, int? max)
        {
            config.m_Progression = ContextRankProgression.MultiplyByModifier;
            config.m_StepLevel = step;
            config.m_UseMax = max.HasValue;
            if (config.m_UseMax)
                config.m_Max = max.Value;
        }
    }
}
