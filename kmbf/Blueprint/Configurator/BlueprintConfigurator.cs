using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using UnityEngine;

namespace kmbf.Blueprint.Configurator
{
    public abstract class ObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : ObjectConfigurator<T, TBuilder>
    {
        protected T instance;
        protected TBuilder Self => (TBuilder)this;

        public ObjectConfigurator(T instance)
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

    public abstract class BlueprintObjectConfigurator<T, TBuilder> : ObjectConfigurator<T, TBuilder>
        where T : BlueprintScriptableObject
        where TBuilder : BlueprintObjectConfigurator<T, TBuilder>
    {
        public BlueprintObjectConfigurator(T instance)
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

    public abstract class BlueprintFactConfigurator<T, TBuilder> : BlueprintObjectConfigurator<T, TBuilder>
        where T : BlueprintFact
        where TBuilder : BlueprintFactConfigurator<T, TBuilder>
    {
        public BlueprintFactConfigurator(T instance)
            : base(instance)
        {

        }
    }



    public class BlueprintUnitFactConfigurator<T, TBuilder> : BlueprintFactConfigurator<T, TBuilder>
        where T : BlueprintUnitFact
        where TBuilder : BlueprintFactConfigurator<T, TBuilder>
    {
        public BlueprintUnitFactConfigurator(T instance)
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

    public class BlueprintBuffConfigurator : BlueprintUnitFactConfigurator<BlueprintBuff, BlueprintBuffConfigurator>
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

    public class BlueprintAbilityConfigurator : BlueprintUnitFactConfigurator<BlueprintAbility, BlueprintAbilityConfigurator>
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
    }

    public class BlueprintFeatureConfigurator : BlueprintUnitFactConfigurator<BlueprintFeature, BlueprintFeatureConfigurator>
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
}
