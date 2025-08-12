using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;

namespace kmbf.Action
{
    internal class ContextConditionHasSpellImmunityToContextDescriptors : ContextCondition
    {
        public override bool CheckCondition()
        {
            var unitSpellImmunity = Target.Unit.Get<UnitPartSpellResistance>();
            if (unitSpellImmunity != null)
            {
                BlueprintAbility blueprintAbility = Context.SourceAbility;
                foreach (var immunity in unitSpellImmunity.Immunities)
                {
                    if(immunity.Type == SpellImmunityType.SpellDescriptor)
                    {
                        if(blueprintAbility != null && blueprintAbility.SpellDescriptor.HasAnyFlag(immunity.SpellDescriptor) && !immunity.Exceptions.HasItem(blueprintAbility))
                        {
                            return true;
                        }
                        else if(Context.SpellDescriptor.HasAnyFlag(immunity.SpellDescriptor))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override string GetConditionCaption()
        {
            return "Check if target has Spell Immunity to Context";
        }
    }
}
