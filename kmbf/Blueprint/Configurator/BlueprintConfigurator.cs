using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using UnityEngine;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : BaseObjectConfigurator<T, TBuilder>, new()
    {
        protected T instance;
        protected TBuilder Self => (TBuilder)this;

        public T Instance { get => instance; }

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
        protected static BPType CreateInstance(GuidType id, string objectName)
        {
            BPType bp = CreateInstance();
            bp.AssetGuid = id.guid;
            bp.name = objectName;
            bp.Components = [];

            id.AssignNewInstance(bp);

            return bp;
        }

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
                id.AssignNewInstance(instance);
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

        public TBuilder AddComponent<C>(C c) where C : BlueprintComponent
        {
            if (instance != null)
            {
                instance.Components = [.. instance.Components, c];
            }

            return Self;
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

        public TBuilder EditComponentGameAction<C, A>(string actionId, Action<A> action)
            where C : BlueprintComponent
            where A : GameAction
        {
            if (instance != null)
            {
                var components = instance.Components.OfType<C>();
                if(components.Empty())
                    Main.Log.Error($"Could not find a a component of type \"{typeof(C).Name}\" in {GetDebugName()}");

                bool foundAny = false;
                foreach (var c in instance.Components.OfType<C>())
                {
                    A foundAction = c.GetGameActionsRecursive()
                        .OfType<A>()
                        .FirstOrDefault(a => a.name == actionId);

                    if (foundAction != null)
                    {
                        action(foundAction);
                        foundAny = true;
                        break;
                    }
                }

                if(!foundAny)
                    Main.Log.Error($"Could not find a game action under a component of type \"{typeof(C).Name}\" with name \"{actionId}\" in {GetDebugName()}");
            }

            return Self;
        }

        public TBuilder EditFirstGameActionWhere<A>(Predicate<A> pred, Action<A> action) where A : GameAction
        {
            if (instance != null)
            {
                bool foundAny = false;
                foreach(var c in instance.Components)
                {
                    A foundAction = c.GetGameActionsRecursive()
                        .OfType<A>()
                        .FirstOrDefault(a => pred(a));
                    if (foundAction != null)
                    {
                        action(foundAction);
                        foundAny = true;
                        break;
                    }
                }

                if(!foundAny)
                    Main.Log.Error($"Could not find a game action with a condition");
            }

            return Self;
        }

        public TBuilder EditAllComponents<C>(Action<C> action) where C : BlueprintComponent
        {
            if (instance != null)
            {
                foreach(C c in instance.GetComponents<C>())
                    action(c);
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

    public abstract class BaseBlueprintUnitFactConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>
        where BPType : BlueprintUnitFact
         where GuidType : BlueprintUnitFactGuid, new()
        where TBuilder : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>, new()
    {
        protected static BPType CreateInstance(GuidType id, string objectName, LocalizedString displayName, LocalizedString description, Sprite icon)
        {
            BPType bp = CreateInstance(id, objectName);
            bp.m_DisplayName = displayName;
            bp.m_Description = description;
            bp.m_Icon = icon;
            return bp;
        }

        public TBuilder SetDisplayName(LocalizedString text)
        {
            if (instance != null)
                instance.m_DisplayName = text;
            return Self;
        }

        public TBuilder SetDescription(LocalizedString text)
        {
            if (instance != null)
                instance.m_Description = text;
            return Self;
        }

        public TBuilder SetIcon(Sprite icon)
        {
            if(instance != null)
                instance.m_Icon = icon;
            return Self;
        }
    }

    public sealed class BlueprintUnitFactConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintUnitFact, BlueprintUnitFactGuid, BlueprintUnitFactConfigurator>
    {

    }

    public class BlueprintBuffConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintBuff, BlueprintBuffGuid, BlueprintBuffConfigurator>
    {
        public static BlueprintBuffConfigurator New(BlueprintBuffGuid buffId, string objectName, LocalizedString displayName, LocalizedString description, Sprite icon)
        {
            BlueprintBuff buff = CreateInstance(buffId, objectName, displayName: displayName, description: description, icon: icon);
            return From(buff);
        }

        public static BlueprintBuffConfigurator NewHidden(BlueprintBuffGuid buffId, string objectName)
        {
            BlueprintBuff buff = CreateInstance(buffId, objectName, displayName: new LocalizedString(), description: new LocalizedString(), icon: null);
            buff.m_Flags = BlueprintBuff.Flags.HiddenInUi;
            return From(buff);
        }

        public BlueprintBuffConfigurator AddFlag(BlueprintBuff.Flags flag)
        {
            if(instance != null)
            {
                instance.m_Flags |= flag;
            }

            return this;
        }

        public BlueprintBuffConfigurator SetStacking(StackingType stackingType)
        {
            if(instance != null)
            {
                instance.Stacking = stackingType;
            }

            return this;
        }
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

    public abstract class BaseBlueprintItemConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItem
        where GuidType : BlueprintItemGuid, new()
        where TBuilder : BaseBlueprintItemConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder SetDisplayName(LocalizedString name)
        {
            if (instance != null)
            {
                instance.m_DisplayNameText = name;
            }

            return Self;
        }

        public TBuilder SetFlavorText(LocalizedString text)
        {
            if (instance != null)
            {
                instance.m_FlavorText = text;
            }

            return Self;
        }
    }

    public sealed class BlueprintItemConfigurator : BaseBlueprintItemConfigurator<BlueprintItem, BlueprintItemGuid, BlueprintItemConfigurator>
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
                var result = c.Results.FirstOrDefault(r => r.Margin.Encompasses(MarginType) && (r.LeaderAlignment & LeaderType) == LeaderType);
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
        public BlueprintKingdomBuffConfigurator AddBuildingCostModifierBuilding(BlueprintSettlementBuildingGuid buildingId)
        {
            if(instance != null)
            {
                var costModifier = instance.GetComponent<BuildingCostModifier>();
                if(costModifier != null)
                {
                    if(buildingId.GetBlueprint(out BlueprintSettlementBuilding building) && !costModifier.Buildings.Contains(building))
                    {
                        costModifier.Buildings = [.. costModifier.Buildings, building];
                    }
                }
                else
                {
                    Main.Log.Error($"Could not find Building Cost Modifier component in Blueprint {GetDebugName()}");
                }
            }

            return this;
        }
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

        public BlueprintSettlementBuildingConfigurator AddAdjacencyBonusBuildings(KingdomStats.Type type, params BlueprintSettlementBuildingGuid[] buildingIds)
        {
            if(instance != null)
            {
                EditComponentWhere<BuildingAdjacencyBonus>(c => c.Stats[type] > 0, c =>
                {
                    foreach(var buildingId in buildingIds)
                    {
                        if (buildingId.GetBlueprint(out BlueprintSettlementBuilding bp) && !c.Buildings.Contains(bp))
                        {
                            c.Buildings.Add(bp);
                        }
                    }
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

    public sealed class BlueprintCueConfigurator : BaseBlueprintObjectConfigurator<BlueprintCue, BlueprintCueGuid, BlueprintCueConfigurator>
    {
        public BlueprintCueConfigurator AddOnStopAction(GameAction action)
        {
            if (instance != null)
            {
                instance.OnStop = ActionListFactory.Add(instance.OnStop, action);
            }

            return this;
        }

        public BlueprintCueConfigurator EditOnStopActionWhere<ActionType>(Predicate<ActionType> pred, Action<ActionType> editAction)
            where ActionType : GameAction
        {
            if(instance != null)
            {
                var action = instance.OnStop.Actions.OfType<ActionType>().FirstOrDefault(a => pred(a));
                if (action != null)
                    editAction(action);
                else
                    Main.Log.Error($"Could not find a component of type \"{(typeof(ActionType).Name)}\" with condition on {GetDebugName()}");
            }

            return this;
        }
    }

    public sealed class BlueprintCheckConfigurator : BaseBlueprintObjectConfigurator<BlueprintCheck, BlueprintCheckGuid, BlueprintCheckConfigurator>
    {
        public BlueprintCheckConfigurator EditDCModifierAt(int index, Action<DCModifier> action)
        {
            if (instance != null)
            {
                if (index > 0 && index < instance.DCModifiers.Length)
                {
                    action(instance.DCModifiers[index]);
                }
                else
                {
                    Main.Log.Error($"Invalid DCModifier index {index}, length was {instance.DCModifiers.Length}");
                }
            }

            return this;
        }
    }

    public sealed class BlueprintCharacterClassConfigurator : BaseBlueprintObjectConfigurator<BlueprintCharacterClass, BlueprintCharacterClassGuid, BlueprintCharacterClassConfigurator>
    {
        public BlueprintCharacterClassConfigurator SetAlignmentRestriction(AlignmentMaskType alignmentMask)
        {
            if (instance != null)
            {
                EditOrAddComponent<PrerequisiteAlignment>(c => c.Alignment = alignmentMask);
            }

            return Self;
        }
    }

    public sealed class BlueprintWeaponTypeConfigurator : BaseBlueprintObjectConfigurator<BlueprintWeaponType, BlueprintWeaponTypeGuid, BlueprintWeaponTypeConfigurator>
    {
        public BlueprintWeaponTypeConfigurator SetTypeName(LocalizedString typeName)
        {
            if (instance != null)
                instance.m_TypeNameText = typeName;

            return Self;
        }

        public BlueprintWeaponTypeConfigurator SetDefaultName(LocalizedString defaultName)
        {
            if (instance != null)
                instance.m_DefaultNameText = defaultName;

            return Self;
        }
    }
}
