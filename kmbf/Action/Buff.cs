//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using kmbf.Blueprint.Configurator;

namespace kmbf.Action
{
    public class ContextConditionHasConditions : ContextCondition
    {
        public UnitCondition[] Conditions;
        public bool Any;

        public override string GetConditionCaption()
        {
            return $"Check if target has one or multiple conditions";
        }

        public override bool CheckCondition()
        {
            if (Any)
            {
                return Conditions.Any(Target.Unit.Descriptor.State.HasCondition);
            }
            else
            {
                return Conditions.All(Target.Unit.Descriptor.State.HasCondition);
            }
        }
    }

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

    public class ContextConditionHasBuffsFromCasterConfigurator : BaseContextConditionConfigurator<ContextConditionHasBuffsFromCaster, ContextConditionHasBuffsFromCasterConfigurator>
    {
        public ContextConditionHasBuffsFromCasterConfigurator SetCaptionName(string captionName)
        {
            return AddOperation(i =>
            {
                i.CaptionName = captionName;
            });
        }

        public ContextConditionHasBuffsFromCasterConfigurator SetBuffs(BlueprintBuff[] buffs)
        {
            return AddOperation(i =>
            {
                i.Buffs = buffs;
            });
        }

        public ContextConditionHasBuffsFromCasterConfigurator SetCount(int count)
        {
            return AddOperation(i =>
            {
                i.Count = count;
            });
        }
    }

    public class ContextActionRemoveBuffFromCaster : ContextAction
    {
        public BlueprintBuff Buff;
        public bool ToCaster;

        public override string GetCaption()
        {
            return $"Remove Target Buff \"{Buff.Name}\" coming from Caster";
        }

        public override void RunAction()
        {
            MechanicsContext mechanicsContext = ElementsContext.GetData<MechanicsContext.Data>()?.Context;
            if (mechanicsContext == null)
            {
                Main.Log.Error("Unable to remove buff: no context found");
                return;
            }

            UnitEntityData unitEntityData = ToCaster ? Context.MaybeCaster : Target.Unit;
            if (unitEntityData == null)
            {
                Main.Log.Error("Unable to remove buff: no target found");
                return;
            }

            var buff = unitEntityData.Buffs.Enumerable.FirstOrDefault(b => b.Blueprint == Buff && b.Context.MaybeCaster == Context.MaybeCaster);
            if (buff != null)
            {
                buff.Remove();
            }
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
}
