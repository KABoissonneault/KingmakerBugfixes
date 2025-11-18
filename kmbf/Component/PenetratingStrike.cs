using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;

namespace kmbf.Component;

// The Kingmaker component 1) listens to target events rather than initiator events 2) has the flag logic inverted
// Just replace it entirely
[ComponentName("Reduces DR against weapons with Focus")]
[AllowedOn(typeof(BlueprintUnitFact))]
public class PenetratingStrike : RuleInitiatorLogicComponent<RuleCalculateDamage>
{
    public int ReductionPenalty;

    public override void OnEventAboutToTrigger(RuleCalculateDamage evt)
    {
        if (evt.DamageBundle.Weapon == null || evt.DamageBundle.WeaponDamage == null)
        {
            return;
        }

        bool flag = false;
        foreach (WeaponFocusParametrized item in base.Owner.Progression.Features.SelectFactComponents<WeaponFocusParametrized>())
        {
            if ((item.Fact as Feature)?.Param == evt.DamageBundle.Weapon.Blueprint.Category)
            {
                flag = true;
                break;
            }
        }
        if (flag)
        {
            evt.DamageBundle.WeaponDamage.SetReductionPenalty(ReductionPenalty);
        }
    }

    public override void OnEventDidTrigger(RuleCalculateDamage evt)
    {
    }
}
