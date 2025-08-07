using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Mechanics.Actions;
using kmbf.Blueprint;
using kmbf.Blueprint.Configurator;

namespace kmbf.Patch
{
    static class AbilitiesFixes
    {
        // Joyful Rapture is supposed to free all allies from any "emotion effects", but the base game only includes Petrified
        // Add Fear, Shaken, Frightened, and NegativeEmotion, which include the "Unbreakable Heart" descriptors, plus overall
        // negative emotion
        public static void FixJoyfulRapture()
        {
            BlueprintAbilityConfigurator.From(BlueprintAbilityGuid.JoyfulRapture)
                .EditComponentGameAction<AbilityEffectRunAction, ContextActionDispelMagic>("$ContextActionDispelBuffs$b4781573-55ad-4e71-9dd9-75a0c38652e0", a =>
                {
                    a.Descriptor |= SpellDescriptor.Fear | SpellDescriptor.Shaken | SpellDescriptor.Frightened | SpellDescriptor.NegativeEmotion;
                })
                .Configure();
        }
    }
}
