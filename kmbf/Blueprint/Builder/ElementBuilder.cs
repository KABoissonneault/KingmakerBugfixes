using Kingmaker.Blueprints;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Conditions;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Action;
using UnityEngine;

namespace kmbf.Blueprint.Builder
{
    static class ElementBuilder
    {
        public static T CreateInstance<T>() where T : ScriptableObject
        {
            var instance = ScriptableObject.CreateInstance<T>();
            UnityEngine.Object.DontDestroyOnLoad(instance);
            return instance;
        }

        public static FlagUnlocked MakeConditionFlagUnlocked(BlueprintUnlockableFlagGuid flagId, params int[] values)
        {
            if (!flagId.GetBlueprint(out BlueprintUnlockableFlag flag)) return null;

            FlagUnlocked instance = CreateInstance<FlagUnlocked>();
            instance.ConditionFlag = flag;
            instance.SpecifiedValues = values;
            return instance;
        }

        public static ContextConditionHasConditions MakeContextConditionHasConditions(UnitCondition[] conditions, bool any = false, bool not = false)
        {
            ContextConditionHasConditions instance = CreateInstance<ContextConditionHasConditions>();
            instance.Conditions = conditions;
            instance.Any = any;
            instance.Not = not;
            return instance;
        }

        public static ContextConditionHasBuffFromCaster MakeContextConditionHasBuffFromCaster(BlueprintBuffGuid buffId, bool not = false)
        {
            if (!buffId.GetBlueprint(out BlueprintBuff buff)) return null;

            ContextConditionHasBuffFromCaster instance = CreateInstance<ContextConditionHasBuffFromCaster>();
            instance.Buff = buff;
            instance.Not = not;
            return instance;
        }

        public static ContextConditionHasBuffsFromCaster MakeContextConditionHasBuffsFromCaster(string captionName, BlueprintBuffGuid[] buffIds, int count)
        {
            if (BlueprintObjectGuid.GetBlueprintArray(buffIds, out BlueprintBuff[] buffs)) return null;

            ContextConditionHasBuffsFromCaster instance = CreateInstance<ContextConditionHasBuffsFromCaster>();
            instance.CaptionName = captionName;
            instance.Buffs = buffs;
            instance.Count = count;
            return instance;
        }

        public static ContextConditionHasSpellImmunityToContextDescriptors MakeContextConditionHasSpellImmunityToContextDescriptors()
        {
            ContextConditionHasSpellImmunityToContextDescriptors instance = CreateInstance<ContextConditionHasSpellImmunityToContextDescriptors>();
            return instance;
        }

        public static BuffConditionCheckRoundNumber MakeBuffConditionCheckRoundNumber(int roundNumber)
        {
            var instance = CreateInstance<BuffConditionCheckRoundNumber>();
            instance.RoundNumber = roundNumber;
            return instance;
        }

        public static ContextActionRemoveSelf MakeContextActionRemoveSelf()
        {
            return CreateInstance<ContextActionRemoveSelf>();
        }

        public static Conditional MakeGameActionConditional(ConditionsChecker conditionsChecker, ActionList ifTrue = null, ActionList ifFalse = null)
        {
            var instance = CreateInstance<Conditional>();
            instance.ConditionsChecker = conditionsChecker;
            instance.IfTrue = ifTrue ?? Constants.Empty.Actions;
            instance.IfFalse = ifFalse ?? Constants.Empty.Actions;
            return instance;
        }

        public static ContextActionApplyBuff MakeContextActionApplyBuff(BlueprintBuffGuid buffId, ContextDurationValue duration, bool canBeDispeled = true)
        {
            if (!buffId.GetBlueprint(out BlueprintBuff buff)) return null;

            var instance = CreateInstance<ContextActionApplyBuff>();
            instance.Buff = buff;
            instance.DurationValue = duration;
            instance.IsNotDispelable = !canBeDispeled;
            return instance;
        }

        public static ContextActionApplyBuff MakeContextActionApplyUndispelableBuff(BlueprintBuffGuid buffId, ContextDurationValue duration)
        {
            return MakeContextActionApplyBuff(buffId, duration, canBeDispeled: false);
        }

        public static ContextActionRemoveTargetBuffIfInitiatorNotActive MakeContextActionRemoveTargetBuffIfInitiatorNotActive(BlueprintBuffGuid buffId, BlueprintBuffGuid activeId)
        {
            if (!buffId.GetBlueprint(out BlueprintBuff buff)) return null;
            if (!activeId.GetBlueprint(out BlueprintBuff active)) return null;

            var instance = CreateInstance<ContextActionRemoveTargetBuffIfInitiatorNotActive>();
            instance.Buff = buff;
            instance.Active = active;
            return instance;
        }

        public static ContextActionRemoveBuffFromCaster MakeContextActionRemoveBuffFromCaster(BlueprintBuffGuid buffId, bool toCaster = false)
        {
            if (!buffId.GetBlueprint(out BlueprintBuff buff)) return null;

            var instance = CreateInstance<ContextActionRemoveBuffFromCaster>();
            instance.Buff = buff;
            instance.ToCaster = toCaster;
            return instance;
        }
    }
}
