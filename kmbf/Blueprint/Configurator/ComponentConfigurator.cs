using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseBlueprintComponentConfigurator<T, TBuilder> : BaseObjectConfigurator<T, TBuilder>
        where T : BlueprintComponent
        where TBuilder : BaseBlueprintComponentConfigurator<T, TBuilder>
    {
        public BaseBlueprintComponentConfigurator(T instance)
            : base(instance)
        {

        }
    }

    public class AbilityEffectRunActionConfigurator : BaseBlueprintComponentConfigurator<AbilityEffectRunAction, AbilityEffectRunActionConfigurator>
    {
        public AbilityEffectRunActionConfigurator(AbilityEffectRunAction instance)
            : base(instance)
        {

        }

        public static AbilityEffectRunActionConfigurator From(AbilityEffectRunAction instance)
        {
            return new(instance);
        }
    }
}
