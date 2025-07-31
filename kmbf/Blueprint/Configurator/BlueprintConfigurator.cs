using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
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

        public TBuilder RemoveComponents<C>() where C : BlueprintComponent
        {
            if(instance != null)
            {
                instance.Components = instance.Components.Where(c => !(c is C)).ToArray();
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
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" in \"{instance.GetDebugName()}\"");
            }

            return Self;
        }

        public TBuilder EditOrAddComponent<C>(Action<C> action) where C : BlueprintComponent
        {
            if (instance != null)
            {
                C c = instance.GetComponent<C>();
                if (c != null)
                    action(c);
                else
                    AddComponent<C>(action);
            }

            return Self;
        }

        public TBuilder EditFirstGameActionWhere<A>(Predicate<A> pred, Action<A> action) where A : GameAction
        {
            if (instance != null)
            {
                foreach(var c in instance.Components)
                {
                    A foundAction = c.GetGameActionsRecursive()
                        .OfType<A>()
                        .FirstOrDefault(a => pred(a));
                    if (foundAction != null)
                    {
                        action(foundAction);
                        break;
                    }
                }
            }

            return Self;
        }
    }

    public class BlueprintObjectConfigurator : BaseBlueprintObjectConfigurator<BlueprintScriptableObject, BlueprintObjectConfigurator>
    {
        public BlueprintObjectConfigurator(BlueprintScriptableObject instance) 
            : base(instance)
        {

        }

        public static BlueprintObjectConfigurator From(BlueprintObjectGuid bpId)
        {
            if (bpId.GetBlueprint(out BlueprintScriptableObject instance))
                return new(instance);
            else
                return new(null);
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

    public class BlueprintSettlementBuildingConfigurator : BaseBlueprintFactConfigurator<BlueprintSettlementBuilding, BlueprintSettlementBuildingConfigurator>
    {
        public BlueprintSettlementBuildingConfigurator(BlueprintSettlementBuilding instance) 
            : base(instance)
        {

        }

        public static BlueprintSettlementBuildingConfigurator From(BlueprintSettlementBuildingGuid buffId)
        {
            if (buffId.GetBlueprint(out BlueprintSettlementBuilding instance))
                return new(instance);
            else
                return new(null);
        }

        public BlueprintSettlementBuildingConfigurator SetAlignmentRestriction(AlignmentMaskType alignment)
        {
            if(instance != null)
            {
                RemoveComponents<AlignmentRestriction>(); // Some BPs have multiple, which is just invalid
                AddComponent<AlignmentRestriction>(c => c.Allowed = alignment);
            }

            return this;
        }

        public BlueprintSettlementBuildingConfigurator SetOtherBuildRestriction(IEnumerable<BlueprintSettlementBuildingGuid> buildings, bool requireAll=false, bool inverted=false)
        {
            if(instance != null)
            {
                EditOrAddComponent<OtherBuildingRestriction>(c =>
                {
                    c.Buildings = new List<BlueprintSettlementBuilding>();
                    foreach(var buildingId in buildings)
                    {
                        if(buildingId.GetBlueprint(out BlueprintSettlementBuilding building))
                        {
                            c.Buildings.Add(building);
                        }
                    }
                    c.RequireAll = requireAll;
                    c.Invert = inverted;
                });
            }

            return this;
        }
    }

    public class BlueprintKingdomEventConfigurator : BaseBlueprintObjectConfigurator<BlueprintKingdomEvent, BlueprintKingdomEventConfigurator>
    {
        public BlueprintKingdomEventConfigurator(BlueprintKingdomEvent instance) 
            : base(instance)
        {

        }

        public static BlueprintKingdomEventConfigurator From(BlueprintKingdomEventGuid eventId)
        {
            if (eventId.GetBlueprint(out BlueprintKingdomEvent instance))
                return new(instance);
            else
                return new(null);
        }

        public BlueprintKingdomEventConfigurator EditPossibleSolution(LeaderType leaderType, Action<PossibleEventSolution> action)
        {
            if (instance != null)
            {
                PossibleEventSolution solution = instance.Solutions.Entries.FirstOrDefault(e => e.Leader == leaderType);
                if (solution != null)
                    action(solution);
                else
                    Main.Log.Error($"Could not find a solution with leader \"{leaderType}\" in \"{instance.GetDebugName()}\"");
            }

            return this;
        }

        public BlueprintKingdomEventConfigurator SetAutoResolve(EventResult.MarginType margin)
        {
            if (instance != null)
            {
                instance.AutoResolveResult = margin;
            }

            return this;
        }
    }
}
