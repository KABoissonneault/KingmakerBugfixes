using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : BaseObjectConfigurator<T, TBuilder>
    {
        protected T instance;
        protected TBuilder Self => (TBuilder)this;

        public BaseObjectConfigurator(T instance)
        {
            this.instance = instance;
        }

        public T Configure()
        {
            return instance;
        }

        protected static T CreateInstance()
        {
            var instance = ScriptableObject.CreateInstance<T>();
            UnityEngine.Object.DontDestroyOnLoad(instance);
            return instance;
        }
    }

    public abstract class BaseBlueprintObjectConfigurator<T, TBuilder> : BaseObjectConfigurator<T, TBuilder>
        where T : BlueprintScriptableObject
        where TBuilder : BaseBlueprintObjectConfigurator<T, TBuilder>
    {
        public BaseBlueprintObjectConfigurator(T instance)
            : base(instance)
        {

        }

        public TBuilder AddComponent<C>(Action<C> init = null) where C : BlueprintComponent
        {
            if (instance != null)
            {
                C c = ScriptableObject.CreateInstance<C>();
                UnityEngine.Object.DontDestroyOnLoad(c);
                if (init != null)
                    init(c);
                instance.Components = [.. instance.Components, c];
            }

            return Self;
        }

        public TBuilder RemoveComponent<C>() where C : BlueprintComponent
        {
            if(instance != null)
            {
                List<BlueprintComponent> components = new(instance.Components);
                int index = Array.FindIndex(instance.Components, c => c is C);
                components.RemoveAt(index);
                instance.Components = components.ToArray();
            }

            return Self;
        }

        public TBuilder EditComponent<C>(Action<C> action) where C : BlueprintComponent
        {
            if(instance != null)
            {
                C c = instance.GetComponent<C>();
                if (c != null)
                    action(c);
            }

            return Self;
        }
    }

    public abstract class BaseBlueprintFactConfigurator<T, TBuilder> : BaseBlueprintObjectConfigurator<T, TBuilder>
        where T : BlueprintFact
        where TBuilder : BaseBlueprintFactConfigurator<T, TBuilder>
    {
        public BaseBlueprintFactConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public class BaseBlueprintUnitFactConfigurator<T, TBuilder> : BaseBlueprintFactConfigurator<T, TBuilder>
        where T : BlueprintUnitFact
        where TBuilder : BaseBlueprintFactConfigurator<T, TBuilder>
    {
        public BaseBlueprintUnitFactConfigurator(T instance)
            : base(instance)
        {

        }

        public TBuilder SetIcon(Sprite icon)
        {
            if(instance != null)
                instance.m_Icon = icon;
            return Self;
        }
    }

    public class BlueprintBuffConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintBuff, BlueprintBuffConfigurator>
    {
        public BlueprintBuffConfigurator(BlueprintBuff instance) 
            : base(instance)
        {

        }

        public static BlueprintBuffConfigurator From(BlueprintBuff instance)
        {
            return new(instance);
        }
    }

    public class BlueprintAbilityConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintAbility, BlueprintAbilityConfigurator>
    {
        public BlueprintAbilityConfigurator(BlueprintAbility instance)
            : base(instance)
        {

        }

        public static BlueprintAbilityConfigurator From(BlueprintAbilityGuid instanceId)
        {
            if (instanceId.GetBlueprint(out BlueprintAbility instance))
                return new(instance);
            else
                return new(null);
        }

        public BlueprintAbilityConfigurator SetFullRoundAction(bool fullRoundAction)
        {
            if(instance)
                instance.m_IsFullRoundAction = fullRoundAction;
            return this;
        }

        public BlueprintAbilityConfigurator AddSpellDescriptor(SpellDescriptor descriptor)
        {
            if(instance != null)
            {
                SpellDescriptorComponent spellDescriptorComponent = instance.GetComponent<SpellDescriptorComponent>();
                if(spellDescriptorComponent != null)
                {
                    spellDescriptorComponent.Descriptor.m_IntValue |= (long)descriptor;
                }
                else
                {
                    AddComponent<SpellDescriptorComponent>(c => c.Descriptor = descriptor);
                }
            }

            return this;
        }
    }

    public class BlueprintFeatureConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintFeature, BlueprintFeatureConfigurator>
    {
        public BlueprintFeatureConfigurator(BlueprintFeature instance)
            : base(instance)
        {

        }

        public static BlueprintFeatureConfigurator From(BlueprintFeatureGuid featureId)
        {
            if (featureId.GetBlueprint(out BlueprintFeature instance))
                return new(instance);
            else
                return new(null);
        }
    }

    public abstract class BaseBlueprintKingdomProjectConfigurator<T, TBuilder> : BaseBlueprintObjectConfigurator<T, TBuilder>
        where T : BlueprintKingdomProject
        where TBuilder : BaseBlueprintKingdomProjectConfigurator<T, TBuilder>
    {
        public BaseBlueprintKingdomProjectConfigurator(T instance)
            : base(instance)
        {

        }

        public TBuilder EditEventFinalResults(Action<EventFinalResults> action) => EditComponent(action);
    }

    public class BlueprintKingdomUpgradeConfigurator : BaseBlueprintKingdomProjectConfigurator<BlueprintKingdomUpgrade, BlueprintKingdomUpgradeConfigurator>
    {
        public BlueprintKingdomUpgradeConfigurator(BlueprintKingdomUpgrade instance)
            : base(instance)
        {

        }

        public static BlueprintKingdomUpgradeConfigurator From(BlueprintKingdomUpgradeGuid upgradeId)
        {
            if (upgradeId.GetBlueprint(out BlueprintKingdomUpgrade instance))
                return new(instance);
            else
                return new(null);
        }

        public BlueprintKingdomUpgradeConfigurator EditFirstResult(Action<EventResult> action)
        {
            return EditEventFinalResults(c =>
            {
                if(c.Results.Length > 0)
                {
                    action(c.Results[0]);
                }
                else
                {
                    Main.Log.Error($"Missing result in EventFinalResults component for Blueprint {instance.GetDebugName()}");
                }
            });
        }
    }

    public class BlueprintKingdomBuffConfigurator : BaseBlueprintFactConfigurator<BlueprintKingdomBuff, BlueprintKingdomBuffConfigurator>
    {
        public BlueprintKingdomBuffConfigurator(BlueprintKingdomBuff instance)
            : base(instance)
        {

        }

        public static BlueprintKingdomBuffConfigurator From(BlueprintKingdomBuffGuid buffId)
        {
            if (buffId.GetBlueprint(out BlueprintKingdomBuff instance))
                return new(instance);
            else
                return new(null);
        }
    }
}
