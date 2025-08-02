using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using UnityEngine;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : BaseObjectConfigurator<T, TBuilder>, new()
    {
        protected T instance;
        protected TBuilder Self => (TBuilder)this;

        public static TBuilder From(T instance)
        {
            TBuilder builder = new();
            builder.instance = instance;
            return builder;
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

    public abstract class BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder> : BaseObjectConfigurator<BPType, TBuilder>
        where BPType : BlueprintScriptableObject
        where GuidType : BlueprintObjectGuid, new()
        where TBuilder : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public static TBuilder From(GuidType id)
        {
            id.GetBlueprint(out BlueprintScriptableObject bp);
            TBuilder builder = new();
            builder.instance = bp as BPType;
            return builder;
        }

        public GuidType GetId()
        {
            if (instance != null)
            {
                var id = new GuidType();
                id.guid = instance.AssetGuid.ToString();
                return id;
            }

            return null;
        }

        public string GetDebugName()
        {
            GuidType id = GetId();
            if(id != null)
            {
                return id.GetDebugName();
            }

            return "<Invalid Instance>";
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
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" in \"{GetDebugName()}\"");
            }

            return Self;
        }

        public TBuilder EditComponentWhere<C>(Predicate<C> pred, Action<C> action) where C : BlueprintComponent
        {
            if (instance != null)
            {
                C c = instance.GetComponentWhere(pred);
                if (c != null)
                    action(c);
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" with condition in \"{GetDebugName()}\"");
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

    public sealed class BlueprintObjectConfigurator : BaseBlueprintObjectConfigurator<BlueprintScriptableObject, BlueprintObjectGuid, BlueprintObjectConfigurator>
    {

    }

    public abstract class BaseBlueprintFactConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintFact
        where GuidType : BlueprintFactGuid, new()
        where TBuilder : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>, new()
    {

    }

    public class BaseBlueprintUnitFactConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>
        where BPType : BlueprintUnitFact
         where GuidType : BlueprintUnitFactGuid, new()
        where TBuilder : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>, new()
    {
        public TBuilder SetIcon(Sprite icon)
        {
            if(instance != null)
                instance.m_Icon = icon;
            return Self;
        }
    }

    public class BlueprintBuffConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintBuff, BlueprintBuffGuid, BlueprintBuffConfigurator>
    {

    }

    public class BlueprintAbilityConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintAbility, BlueprintAbilityGuid, BlueprintAbilityConfigurator>
    {
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

        public BlueprintAbilityConfigurator EditDamageDiceRankConfig(Action<ContextRankConfig> action)
        {
            return EditComponentWhere(c => c.Type == Kingmaker.Enums.AbilityRankType.DamageDice, action);
        }
    }

    public class BlueprintFeatureConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintFeature, BlueprintFeatureGuid, BlueprintFeatureConfigurator>
    {

    }

    public abstract class BaseBlueprintKingdomProjectConfigurator<BPType, GuidType,TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintKingdomProject
        where GuidType : BlueprintObjectGuid, new()
        where TBuilder : BaseBlueprintKingdomProjectConfigurator<BPType, GuidType,TBuilder>, new()
    {
        public TBuilder EditEventFinalResults(Action<EventFinalResults> action) => EditComponent(action);

        public TBuilder EditEventFinalResult(EventResult.MarginType MarginType, AlignmentMaskType LeaderType, Action<EventResult> action)
        {
            return EditEventFinalResults(c =>
            {
                var result = c.Results.FirstOrDefault(r => r.Margin.Encompasses(MarginType) && (r.LeaderAlignment & LeaderType) != LeaderType);
                if (result != null)
                {
                    action(result);
                }
                else
                {
                    Main.Log.Error($"Missing result of margin \"{MarginType}\" and LeaderType \"{LeaderType}\" in EventFinalResults component for Blueprint {GetDebugName()}");
                }
            });
        }
    }

    public class BlueprintKingdomUpgradeConfigurator : BaseBlueprintKingdomProjectConfigurator<BlueprintKingdomUpgrade, BlueprintKingdomUpgradeGuid, BlueprintKingdomUpgradeConfigurator>
    {
        public BlueprintKingdomUpgradeConfigurator EditEventSuccessAnyFinalResult(Action<EventResult> action)
        {
            return EditEventFinalResult(EventResult.MarginType.Success, AlignmentMaskType.Any, action);
        }
    }

    public class BlueprintKingdomBuffConfigurator : BaseBlueprintFactConfigurator<BlueprintKingdomBuff, BlueprintKingdomBuffGuid, BlueprintKingdomBuffConfigurator>
    {

    }

    public class BlueprintSettlementBuildingConfigurator : BaseBlueprintFactConfigurator<BlueprintSettlementBuilding, BlueprintSettlementBuildingGuid, BlueprintSettlementBuildingConfigurator>
    {
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

    public class BlueprintKingdomEventConfigurator : BaseBlueprintObjectConfigurator<BlueprintKingdomEvent, BlueprintKingdomEventGuid, BlueprintKingdomEventConfigurator>
    {
        public BlueprintKingdomEventConfigurator EditPossibleSolution(LeaderType leaderType, Action<PossibleEventSolution> action)
        {
            if (instance != null)
            {
                PossibleEventSolution solution = instance.Solutions.Entries.FirstOrDefault(e => e.Leader == leaderType);
                if (solution != null)
                    action(solution);
                else
                    Main.Log.Error($"Could not find a solution with leader \"{leaderType}\" in \"{GetDebugName()}\"");
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

    public sealed class BlueprintRandomEncounterConfigurator : BaseBlueprintObjectConfigurator<BlueprintRandomEncounter, BlueprintRandomEncounterGuid, BlueprintRandomEncounterConfigurator>
    {
        public BlueprintRandomEncounterConfigurator SetPool(EncounterPool pool)
        {
            if (instance != null)
            {
                instance.Pool = pool;
            }

            return this;
        }
    }
}
