using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;

namespace kmbf.Action
{
    public class ContextConditionHasBuffsFromCaster : ContextCondition
    {
        public BlueprintBuff[] Buffs;
        public int Count = 1;
        public string CaptionName;

        public override string GetConditionCaption()
        {
            return string.IsNullOrEmpty(CaptionName) ? $"Check if target has {Count} of multiple buffs from caster" : $"Check if target has {Count} {CaptionName} from caster";
        }

        public override bool CheckCondition()
        {
            int ContainedCount = 0;
            UnitEntityData maybeCaster = base.Context.MaybeCaster;
            foreach (Buff buff in base.Target.Unit.Buffs)
            {
                if (Buffs.Contains(buff.Blueprint) && buff.Context.MaybeCaster == maybeCaster)
                {
                    ++ContainedCount;
                    if(ContainedCount >= Count)
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
