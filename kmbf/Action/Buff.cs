using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Blueprint.Configurator;

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

    public class ContextConditionHasBuffsFromCasterConfigurator : ContextConditionConfigurator<ContextConditionHasBuffsFromCaster, ContextConditionHasBuffsFromCasterConfigurator>
    {
        public ContextConditionHasBuffsFromCasterConfigurator(ContextConditionHasBuffsFromCaster instance)
            : base(instance)
        {

        }

        public static ContextConditionHasBuffsFromCasterConfigurator New(string captionName, BlueprintBuff[] buffs, int count)
        {
            ContextConditionHasBuffsFromCaster instance = CreateInstance();
            instance.CaptionName = captionName;
            instance.Buffs = buffs;
            instance.Count = count;
            return new ContextConditionHasBuffsFromCasterConfigurator(instance);
        }
        
        public ContextConditionHasBuffsFromCasterConfigurator SetCaptionName(string captionName)
        {
            instance.CaptionName = captionName;
            return this;
        }

        public ContextConditionHasBuffsFromCasterConfigurator SetBuffs(BlueprintBuff[] buffs)
        {
            instance.Buffs = buffs;
            return this;
        }

        public ContextConditionHasBuffsFromCasterConfigurator SetCount(int count)
        {
            instance.Count = count;
            return this;
        }
    }

    public class ContextActionRemoveTargetBuffIfInitiatorNotActive : ContextAction
    {
        public BlueprintBuff Buff;
        public BlueprintBuff Active;

        public override string GetCaption()
        {
            return $"Remove Target Buff \"{Buff.Name}\" If Not Caster Active \"{Active.Name}\"";
        }

        public override void RunAction()
        {
            MechanicsContext mechanicsContext = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
            if (mechanicsContext == null)
            {
                Main.Log.Error("Unable to remove buff: no context found");
            }
            else if (!mechanicsContext.MaybeCaster.Buffs.HasFact(Active))
            {                
                Target.Unit.Buffs.RemoveFact(Buff);
            }
        }
    }

    public class ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator : ContextActionConfigurator<ContextActionRemoveTargetBuffIfInitiatorNotActive, ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator>
    {
        public ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator(ContextActionRemoveTargetBuffIfInitiatorNotActive instance)
            : base(instance)
        {

        }

        public static ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator New(BlueprintBuff buff, BlueprintBuff active)
        {
            ContextActionRemoveTargetBuffIfInitiatorNotActive instance = CreateInstance();
            instance.Buff = buff;
            instance.Active = active;
            return new ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator(instance);
        }

        public ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator SetBuff(BlueprintBuff buff)
        {
            instance.Buff = buff;
            return this;
        }

        public ContextActionRemoveTargetBuffIfInitiatorNotActiveConfigurator SetActive(BlueprintBuff active)
        {
            instance.Active = active;
            return this;
        }
    }
}
