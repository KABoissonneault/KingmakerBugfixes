using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BlueprintComponentConfigurator<T, TBuilder> : ObjectConfigurator<T, BlueprintComponentConfigurator<T, TBuilder>>
        where T : BlueprintComponent
        where TBuilder : BlueprintComponentConfigurator<T, TBuilder>
    {
        public BlueprintComponentConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public class AbitlityEffectRunActionConfigurator : BlueprintComponentConfigurator<AbilityEffectRunAction, AbitlityEffectRunActionConfigurator>
    {
        public AbitlityEffectRunActionConfigurator(AbilityEffectRunAction instance)
            : base(instance)
        {

        }

        public static AbitlityEffectRunActionConfigurator From(AbilityEffectRunAction instance)
        {
            return new(instance);
        }
    }
}
