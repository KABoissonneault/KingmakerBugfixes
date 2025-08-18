using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.Utility;

namespace kmbf.Component
{
    public class AdditionalBonusOnDamage : GameLogicComponent, IInitiatorRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, IInitiatorRulebookSubscriber
    {
        public bool CheckSpellDescriptor;

        [ShowIf("CheckSpellDescriptor")]
        public SpellDescriptorWrapper SpellDescriptorsList;

        public bool IgnoreDamageFromThisFact = true;

        public int BonusOnDamage;

        public void OnEventAboutToTrigger(RuleDealDamage evt)
        {
            if (FullCheck(evt))
            {
                SetDamage(evt);
            }
        }

        public void OnEventDidTrigger(RuleDealDamage evt)
        {
        }

        private void SetDamage(RuleDealDamage evt)
        {
            DamageTypeDescription damageTypeDescription = evt.DamageBundle.First.CreateTypeDescription();
            SetDamageEntity(evt,  damageTypeDescription);
        }

        private void SetDamageEntity(RuleDealDamage evt, DamageTypeDescription damageType)
        {
            BaseDamage baseDamage = new DamageDescription
            {
                TypeDescription = damageType,
                Dice = new DiceFormula(),
                Bonus = BonusOnDamage
            }.CreateDamage();

            evt.DamageBundle.Add(baseDamage);
        }

        private bool FullCheck(RuleDealDamage evt)
        {
            if (CheckSpellDescriptor && (evt.Reason.Ability == null || !evt.Reason.Ability.Blueprint.SpellDescriptor.HasFlag((SpellDescriptor)SpellDescriptorsList)))
            {
                return false;
            }
            if ((bool)evt.SourceArea)
            {
                return false;
            }
            if (IgnoreDamageFromThisFact && evt.Reason.Fact == base.Fact)
            {
                return false;
            }
            return true;
        }
    }
}
