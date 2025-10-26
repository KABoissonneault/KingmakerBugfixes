//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using HarmonyLib;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Classes.Prerequisites;
using Kingmaker.Blueprints.Classes.Selection;
using Kingmaker.Blueprints.Classes.Spells;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Blueprints.Items;
using Kingmaker.Blueprints.Items.Ecnchantments;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Loot;
using Kingmaker.Blueprints.Quests;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.Globalmap.Blueprints;
using Kingmaker.Kingdom;
using Kingmaker.Kingdom.Blueprints;
using Kingmaker.Kingdom.Settlements;
using Kingmaker.Kingdom.Settlements.BuildingComponents;
using Kingmaker.Kingdom.Tasks;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.AreaEffects;
using Kingmaker.UnitLogic.ActivatableAbilities;
using Kingmaker.UnitLogic.Alignments;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Components;
using Kingmaker.Utility;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine;

namespace kmbf.Blueprint.Configurator
{
    public abstract class BaseObjectConfigurator<T, TBuilder> 
        where T : ScriptableObject 
        where TBuilder : BaseObjectConfigurator<T, TBuilder>, new()
    {
        private bool configured = false;
        private List<Action<T>> onConfigureOperations = new();

        private T instance;
        protected TBuilder Self => (TBuilder)this;

        public T Instance { get => instance; }

        protected void SetInstance(T i) { instance = i; }

        ~BaseObjectConfigurator()
        {
            if(!configured)
            {
                Main.Log.Error($"Configurator for object {GetSafeName()} was not configured");
            }
        }

        public static TBuilder From(T instance)
        {
            TBuilder builder = new();
            builder.instance = instance;
            return builder;
        }
                
        protected static T CreateInstance()
        {
            var instance = ScriptableObject.CreateInstance<T>();
            UnityEngine.Object.DontDestroyOnLoad(instance);
            return instance;
        }

        protected TBuilder AddOperation(Action<T> op)
        {
            onConfigureOperations.Add(op);
            return Self;
        }

        public T Configure()
        {
            if (instance == null)
            {
                return null;
            }

            if(configured)
            {
                Main.Log.Error($"Object '{GetDebugName()}' was already configured");
                return instance;
            }

            configured = true;
            foreach(Action<T> action in onConfigureOperations)
            {
                action(instance);
            }
            OnConfigure();
            return instance;
        }

        protected virtual void OnConfigure() { }
        public virtual string GetDebugName()
        {
            if (instance != null)
            {
                return instance.name;
            }

            return "<Invalid instance>";
        }

        public virtual string GetSafeName() { return "<Unknown instance>"; }
    }

    public abstract class BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder> : BaseObjectConfigurator<BPType, TBuilder>
        where BPType : BlueprintScriptableObject
        where GuidType : BlueprintObjectGuid, new()
        where TBuilder : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>, new()
    {
        private List<BlueprintComponent> addedComponents = new();
        private List<BlueprintComponent> componentsToRemove = new();
        
        protected static BPType CreateInstance(GuidType id, string objectName)
        {
            if(ResourcesLibrary.LibraryObject.BlueprintsByAssetId.ContainsKey(id.guid))
            {
                Main.Log.Error($"Blueprint with GUID '{id.guid}' is already registered in Resources Library");
                return null;
            }

            BPType bp = CreateInstance();
            bp.AssetGuid = id.guid;
            bp.name = objectName;
            bp.Components = [];

            ResourcesLibrary.LibraryObject.m_AllBlueprints.Add(bp);
            ResourcesLibrary.LibraryObject.BlueprintsByAssetId.Add(id.guid, bp);

            id.AssignNewInstance(bp);
                        
            return bp;
        }

        public static TBuilder From(GuidType id)
        {
            id.GetBlueprint(out BlueprintScriptableObject bp);
            if (bp != null && !(bp is BPType))
                Main.Log.Error($"Blueprint with GUID '{id.guid}' did not have expected type {id.BlueprintTypeName}");

            TBuilder builder = new();
            builder.SetInstance(bp as BPType);
            return builder;
        }

        public GuidType GetId()
        {
            if (Instance != null)
            {
                var id = new GuidType();
                id.guid = Instance.AssetGuid.ToString();
                id.AssignNewInstance(Instance);
                return id;
            }

            return null;
        }

        public override string GetDebugName()
        {
            GuidType id = GetId();
            if(id != null)
            {
                return id.GetDebugName();
            }

            return "<Invalid Instance>";
        }

        public override string GetSafeName()
        {
            if(Instance)
            {
                return Instance.AssetGuid;
            }
            else
            {
                return "<Invalid instance>";
            }
        }

        public TBuilder AddComponent<C>(C c) where C : BlueprintComponent
        {
            addedComponents.Add(c);
            return Self;
        }

        public TBuilder AddComponent<C>(Action<C> init = null) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = ScriptableObject.CreateInstance<C>();
                UnityEngine.Object.DontDestroyOnLoad(c);
                if (init != null)
                    init(c);
                addedComponents.Add(c);
            }

            return Self;
        }

        public TBuilder RemoveComponent<C>() where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = Instance.Components.OfType<C>().FirstOrDefault();
                if (c != null)
                    componentsToRemove.Add(c);
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" in \"{GetDebugName()}\"");
            }

            return Self;
        }

        public TBuilder RemoveComponentWhere<C>(Predicate<C> pred) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = Instance.Components.OfType<C>().Where(c => pred(c)).FirstOrDefault();
                if (c != null)
                    componentsToRemove.Add(c);
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" with condition in \"{GetDebugName()}\"");
            }

            return Self;
        }


        public TBuilder RemoveComponents<C>() where C : BlueprintComponent
        {
            if(Instance != null)
            {
                componentsToRemove.AddRange(Instance.Components.OfType<C>());
            }

            return Self;
        }

        public TBuilder RemoveComponentsWhere<C>(Predicate<C> pred) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                componentsToRemove.AddRange(Instance.Components.OfType<C>().Where(c => pred(c)));
            }

            return Self;
        }

        public TBuilder EditComponent<C>(Action<C> action) where C : BlueprintComponent
        {
            if(Instance != null)
            {
                C c = Instance.GetComponent<C>();
                if (c != null)
                    AddOperation(_ => action(c));
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" in \"{GetDebugName()}\"");
            }

            return Self;
        }

        public TBuilder EditComponentWhere<C>(Predicate<C> pred, Action<C> action) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = Instance.GetComponentWhere<C>(pred);
                if (c != null)
                    AddOperation(_ => action(c));
                else
                    Main.Log.Error($"Could not find component of type \"{typeof(C).Name}\" with condition in \"{GetDebugName()}\"");
            }

            return Self;
        }

        public TBuilder EditOrAddComponent<C>(Action<C> action) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = Instance.GetComponent<C>();
                if (c != null)
                    AddOperation(_ => action(c));
                else
                    AddComponent(action);
            }

            return Self;
        }

        // Make sure that "action" initializes the component in such as way to make that predicate true
        public TBuilder EditOrAddComponentWhere<C>(Predicate<C> pred, Action<C> action) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                C c = Instance.GetComponentWhere<C>(pred);
                if (c != null)
                    AddOperation(_ => action(c));
                else
                    AddComponent(action);
            }

            return Self;
        }

        public TBuilder EditComponentGameAction<C, A>(string actionId, Action<A> action)
            where C : BlueprintComponent
            where A : GameAction
        {
            if (Instance != null)
            {
                var components = Instance.Components.OfType<C>();
                if(components.Empty())
                    Main.Log.Error($"Could not find a a component of type \"{typeof(C).Name}\" in {GetDebugName()}");

                bool foundAny = false;
                foreach (var c in Instance.Components.OfType<C>())
                {
                    A foundAction = c.GetGameActionsRecursive()
                        .OfType<A>()
                        .FirstOrDefault(a => a.name == actionId);

                    if (foundAction != null)
                    {
                        AddOperation(_ => action(foundAction));
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
            if (Instance != null)
            {
                bool foundAny = false;
                foreach(var c in Instance.Components)
                {
                    A foundAction = c.GetGameActionsRecursive()
                        .OfType<A>()
                        .FirstOrDefault(a => pred(a));
                    if (foundAction != null)
                    {
                        AddOperation(_ => action(foundAction));
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
            if (Instance != null)
            {
                var components = Instance.GetComponents<C>().ToList();
                if (!components.Empty())
                {
                    AddOperation(_ =>
                    {
                        foreach (C c in components)
                            action(c);
                    });
                }
            }

            return Self;
        }

        public TBuilder ReplaceAllComponentsWithSource(BlueprintObjectGuid source)
        {
            if (Instance != null)
            {
                if (source.GetBlueprint(out BlueprintScriptableObject sourceInstance))
                {
                    componentsToRemove.AddRange(Instance.Components);
                    addedComponents.AddRange(sourceInstance.Components);
                }
            }

            return Self;
        }

        public TBuilder ReplaceComponentsWithSource<C>(BlueprintObjectGuid source) where C : BlueprintComponent
        {
            if (Instance != null)
            {
                if(source.GetBlueprint(out BlueprintScriptableObject sourceInstance))
                {
                    componentsToRemove.AddRange(Instance.Components.OfType<C>());
                    addedComponents.AddRange(sourceInstance.Components.OfType<C>());
                }
            }

            return Self;
        }

        public TBuilder ReplaceComponents<CurrentType, NewType>(Action<CurrentType, NewType> action) 
            where CurrentType : BlueprintComponent
            where NewType : BlueprintComponent
        {
            if (Instance != null)
            {
                foreach(var cc in Instance.Components.OfType<CurrentType>())
                {
                    NewType nc = ScriptableObject.CreateInstance<NewType>();
                    UnityEngine.Object.DontDestroyOnLoad(nc);
                    action(cc, nc);
                    addedComponents.Add(nc);
                    componentsToRemove.Add(cc);
                }
            }

            return Self;
        }
        
        protected override void OnConfigure()
        {
            Instance.Components = Instance.Components.Except(componentsToRemove).Concat(addedComponents).ToArray();

            // Ensure unique names
            // Components with JsonPropertyAttribute/SerializableAttribute should have a fixed name!
            var names = new HashSet<string>();
            foreach(var c in Instance.Components)
            {
                if (string.IsNullOrEmpty(c.name))
                {
#if DEBUG
                    if (IsStatefulComponent(c))
                    {
                        Main.Log.Error($"Stateful component of type '{c.GetType().Name}' has no fixed name in Blueprint '{GetDebugName()}'");
                    }
#endif

                    for (int i = 0; !names.Add(c.name = $"${c.GetType().Name}${i}"); ++i) ;

                }
                else if(!names.Add(c.name))
                {
#if DEBUG
                    if (IsStatefulComponent(c))
                    {
                        Main.Log.Error($"Stateful component of type '{c.GetType().Name}' has duplicated name in Blueprint '{GetDebugName()}'");
                    }
#endif

                    for (int i = 0; !names.Add(c.name = $"${c.name}${i}"); ++i) ;
                }
            }
        }

#if DEBUG
        static Dictionary<Type, bool> typeStateCheck = new Dictionary<Type, bool>();

        private static bool IsStatefulComponent(BlueprintComponent c)
        {
            Type derivedType = c.GetType();

            if (typeStateCheck.TryGetValue(derivedType, out bool stateful)) return stateful;

            for(var t = derivedType; t != null; t = t.BaseType)
            {
                if(t.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                    .Any(f => f.CustomAttributes.Any(a => a.AttributeType == typeof(JsonPropertyAttribute) || a.AttributeType == typeof(SerializableAttribute))))
                {
                    typeStateCheck.Add(derivedType, true);
                    return true;
                }
            }

            typeStateCheck.Add(derivedType, false);
            return false;
        }
#endif
    }

    public sealed class BlueprintObjectConfigurator : BaseBlueprintObjectConfigurator<BlueprintScriptableObject, BlueprintObjectGuid, BlueprintObjectConfigurator>
    {

    }

    public abstract class BaseBlueprintFactConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintFact
        where GuidType : BlueprintFactGuid, new()
        where TBuilder : BaseBlueprintFactConfigurator<BPType, GuidType,TBuilder>, new()
    {
        public TBuilder EditOrAddDefaultContextRankConfig(Action<ContextRankConfig> action)
        {
            return EditOrAddComponentWhere<ContextRankConfig>(c => c.m_Type == AbilityRankType.Default, c =>
            {
                c.m_Type = AbilityRankType.Default;
                action(c);
            });
        }
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
            return AddOperation(i =>
            {
                i.m_DisplayName = text;
            });
        }

        public TBuilder SetDescription(LocalizedString text)
        {
            return AddOperation(i =>
            {
                i.m_Description = text;
            });
        }

        public TBuilder SetIcon(Sprite icon)
        {
            return AddOperation(i =>
            {
                i.m_Icon = icon;
            });
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
            return AddOperation(i =>
            {
                i.m_Flags |= flag;
            });
        }

        public BlueprintBuffConfigurator RemoveFlag(BlueprintBuff.Flags flag)
        {
            return AddOperation(i =>
            {
                i.m_Flags &= ~flag;
            });
        }

        public BlueprintBuffConfigurator RemoveSpellDescriptor(SpellDescriptor descriptor)
        {
            return EditComponent<SpellDescriptorComponent>(c =>
            {
                c.Descriptor &= ~descriptor;
            });
        }

        public BlueprintBuffConfigurator SetStacking(StackingType stackingType)
        {
            return AddOperation(i =>
            {
                i.Stacking = stackingType;
            });
        }

        public BlueprintBuffConfigurator SetCasterLevel(ContextValue value)
        {
            return EditOrAddComponent<ContextSetAbilityParams>(c =>
            {
                c.CasterLevel = value;
            });
        }
    }

    public sealed class BlueprintAbilityConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintAbility, BlueprintAbilityGuid, BlueprintAbilityConfigurator>
    {
        public static BlueprintAbilityConfigurator New(BlueprintAbilityGuid guid, string objectName)
        {
            var instance = CreateInstance(guid, objectName);
            return From(instance);
        }

        public BlueprintAbilityConfigurator SetSpellSchool(SpellSchool school)
        {
            return EditOrAddComponent<SpellComponent>(c =>
            {
                c.School = school;
            });
        }

        public BlueprintAbilityConfigurator AddAbilityEffectRunAction(GameAction action)
        {
            return EditOrAddComponent<AbilityEffectRunAction>(c =>
            {
                c.Actions = ActionListFactory.Add(c.Actions, action);
            });
        }

        public BlueprintAbilityConfigurator SetSpellResistance(bool value)
        {
            return AddOperation(i =>
            {
                i.SpellResistance = value;
            });
        }

        public BlueprintAbilityConfigurator SetFullRoundAction(bool fullRoundAction)
        {
            return AddOperation(i =>
            {
                i.m_IsFullRoundAction = fullRoundAction;
            });
        }

        public BlueprintAbilityConfigurator AddSpellDescriptor(SpellDescriptor descriptor)
        {
            return EditOrAddComponent<SpellDescriptorComponent>(c =>
            {
                c.Descriptor.m_IntValue |= (long)descriptor;
            });
        }

        public BlueprintAbilityConfigurator EditDamageDiceRankConfig(Action<ContextRankConfig> action)
        {
            return EditComponentWhere(c => c.Type == Kingmaker.Enums.AbilityRankType.DamageDice, action);
        }

        public BlueprintAbilityConfigurator AddDamageDiceRankConfigClass(BlueprintCharacterClassGuid characterClassId)
        {
            return EditDamageDiceRankConfig(damageDiceRankConfig =>
            {
                if (!characterClassId.GetBlueprint(out BlueprintCharacterClass characterClass)) return;

                if (!damageDiceRankConfig.m_Class.Any(c => c.AssetGuid == characterClass.AssetGuid))
                {
                    damageDiceRankConfig.m_Class = damageDiceRankConfig.m_Class.AddItem(characterClass).ToArray();
                }
            });
        }
    }

    public sealed class BlueprintActivatableAbilityConfigurator : BaseBlueprintUnitFactConfigurator<BlueprintActivatableAbility, BlueprintActivatableAbilityGuid, BlueprintActivatableAbilityConfigurator>
    {
        public BlueprintActivatableAbilityConfigurator SetIsOnByDefault(bool onByDefault)
        {
            return AddOperation(a =>
            {
                a.IsOnByDefault = onByDefault;
            });
        }
    }

    public abstract class BaseBlueprintFeatureConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintUnitFactConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintFeature
        where GuidType : BlueprintFeatureGuid, new()
        where TBuilder : BaseBlueprintFeatureConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder AddGroup(FeatureGroup group)
        {
            return AddOperation(f =>
            {
                f.Groups = [.. f.Groups, group];
            });
        }

        public TBuilder SetRanks(int ranks)
        {
            return AddOperation(f =>
            {
                f.Ranks = ranks;
            });
        }

        public TBuilder SetIsClassFeature(bool isClassFeature)
        {
            return AddOperation(f =>
            {
                f.IsClassFeature = isClassFeature;
            });
        }

        public TBuilder SetHideInUI(bool hideInUI)
        {
            return AddOperation(f =>
            {
                f.HideInUI = hideInUI;
            });
        }
    }

    public sealed class BlueprintFeatureConfigurator : BaseBlueprintFeatureConfigurator<BlueprintFeature, BlueprintFeatureGuid, BlueprintFeatureConfigurator>
    {
        public static BlueprintFeatureConfigurator New(BlueprintFeatureGuid guid, string objectName)
        {
            var instance = CreateInstance(guid, objectName);
            return From(instance);
        }
    }

    public sealed class BlueprintRaceConfigurator : BaseBlueprintFeatureConfigurator<BlueprintRace, BlueprintRaceGuid, BlueprintRaceConfigurator>
    {
        public BlueprintRaceConfigurator AddFeature(BlueprintFeature feature)
        {
            return AddOperation(r =>
            {
                r.Features = [.. r.Features, feature];
            });
        }
    }

    public sealed class BlueprintFeatureSelectionConfigurator : BaseBlueprintFactConfigurator<BlueprintFeatureSelection, BlueprintFeatureSelectionGuid, BlueprintFeatureSelectionConfigurator>
    {
        public BlueprintFeatureSelectionConfigurator SetMode(SelectionMode mode)
        {
            return AddOperation(f =>
            {
                f.Mode = mode;
            });
        }

        public BlueprintFeatureSelectionConfigurator AddAllFeature(BlueprintFeature feature)
        {
            return AddOperation(f =>
            {
                f.AllFeatures = [.. f.AllFeatures, feature];
            });
        }
    }

    public sealed class BlueprintProgressionConfigurator : BaseBlueprintFeatureConfigurator<BlueprintProgression, BlueprintProgressionGuid, BlueprintProgressionConfigurator>
    {
        public BlueprintProgressionConfigurator EditLevelEntries(Func<LevelEntry[], LevelEntry[]> func)
        {
            return AddOperation(p =>
            {
                p.LevelEntries = func(p.LevelEntries);
            });
        }
    }

    public abstract class BaseBlueprintItemConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItem
        where GuidType : BlueprintItemGuid, new()
        where TBuilder : BaseBlueprintItemConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder SetDisplayName(LocalizedString name)
        {
            return AddOperation(i =>
            {
                i.m_DisplayNameText = name;
            });
        }

        public TBuilder SetFlavorText(LocalizedString text)
        {
            return AddOperation(i =>
            {
                i.m_FlavorText = text;
            });
        }
    }

    public sealed class BlueprintItemConfigurator : BaseBlueprintItemConfigurator<BlueprintItem, BlueprintItemGuid, BlueprintItemConfigurator>
    {

    }

    public abstract class BaseBlueprintItemEquipmentConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintItemConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItemEquipment
        where GuidType : BlueprintItemEquipmentGuid, new()
        where TBuilder : BaseBlueprintItemEquipmentConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder SetAbility(BlueprintAbilityGuid abilityId)
        {
            return AddOperation(i =>
            {
                if (abilityId.GetBlueprint(out BlueprintAbility ability))
                {
                    i.Ability = ability;
                }
            });
        }

        // Seems to be used to evaluate loot importance. The higher, the rarer
        public TBuilder SetCR(int cr)
        {
            return AddOperation(i =>
            {
                i.CR = cr;
            });
        }

        public TBuilder SetDC(int dc)
        {
            return AddOperation(i =>
            {
                i.DC = dc;
            });
        }
    }

    public sealed class BlueprintItemEquipmentConfigurator : BaseBlueprintItemEquipmentConfigurator<BlueprintItemEquipment, BlueprintItemEquipmentGuid, BlueprintItemEquipmentConfigurator>
    {

    }

    public abstract class BaseBlueprintItemEquipmentSimpleConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintItemEquipmentConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItemEquipmentSimple
        where GuidType : BlueprintItemEquipmentSimpleGuid, new()
        where TBuilder : BaseBlueprintItemEquipmentSimpleConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder AddEnchantment(BlueprintEquipmentEnchantmentGuid enchantmentId)
        {
            return AddOperation(i =>
            {
                if (enchantmentId.GetBlueprint(out BlueprintEquipmentEnchantment enchantment))
                {
                    i.m_Enchantments = [.. i.m_Enchantments, enchantment];
                }
            });
        }
    }

    public sealed class BlueprintItemEquipmentSimpleConfigurator : BaseBlueprintItemEquipmentSimpleConfigurator<BlueprintItemEquipmentSimple, BlueprintItemEquipmentSimpleGuid, BlueprintItemEquipmentSimpleConfigurator>
    {

    }

    public abstract class BaseBlueprintItemEquipmentHandConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintItemEquipmentConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItemEquipmentHand
        where GuidType : BlueprintItemEquipmentHandGuid, new()
        where TBuilder : BaseBlueprintItemEquipmentHandConfigurator<BPType, GuidType, TBuilder>, new()
    {
        public TBuilder EditVisualParameters(Action<WeaponVisualParameters> action)
        {
            return AddOperation(h =>
            {
                action(h.m_VisualParameters);
            });
        }
    }

    public sealed class BlueprintItemWeaponConfigurator : BaseBlueprintItemEquipmentHandConfigurator<BlueprintItemWeapon, BlueprintItemWeaponGuid,  BlueprintItemWeaponConfigurator>
    {
        public BlueprintItemWeaponConfigurator AddEnchantment(BlueprintWeaponEnchantmentGuid enchantmentId)
        {
            return AddOperation(i =>
            {
                if (enchantmentId.GetBlueprint(out BlueprintWeaponEnchantment enchantment))
                {
                    i.m_Enchantments = [.. i.m_Enchantments, enchantment];
                }
            });
        }

        public BlueprintItemWeaponConfigurator AddTrashCategory(TrashLootType lootType)
        {
            return AddOperation(i =>
            {
                i.TrashLootTypes = [.. i.TrashLootTypes, lootType];
            });
        }

    }

    public abstract class BaseBlueprintItemEnchantmentConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintFactConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintItemEnchantment
        where GuidType : BlueprintItemEnchantmentGuid, new()
        where TBuilder : BaseBlueprintItemEnchantmentConfigurator<BPType, GuidType, TBuilder>, new()
    {

    }

    public sealed class BlueprintWeaponEnchantmentConfigurator : BaseBlueprintItemEnchantmentConfigurator<BlueprintWeaponEnchantment, BlueprintWeaponEnchantmentGuid, BlueprintWeaponEnchantmentConfigurator>
    {
        public BlueprintWeaponEnchantmentConfigurator SetDC(ContextValue value, bool add10ToDC)
        {
            return EditOrAddComponent<ContextSetAbilityParams>(c =>
            {
                c.Add10ToDC = add10ToDC;
                c.DC = value;
            });
        }

        // AddInitiatorAttackRollTrigger is missing some features that AddInitiatorAttackWithWeaponTrigger has, such as Wait for Resolve
        // For some weapon enchantments, we want to replace with the equivalent Weapon Trigger
        public BlueprintWeaponEnchantmentConfigurator ReplaceAttackRollTriggerWithWeaponTrigger(Action<AddInitiatorAttackWithWeaponTrigger> action)
        {
            return ReplaceComponents<AddInitiatorAttackRollTrigger, AddInitiatorAttackWithWeaponTrigger>((attackRollTrigger, weaponTrigger) =>
            {
                weaponTrigger.name = attackRollTrigger.name;
                weaponTrigger.OnlyHit = attackRollTrigger.OnlyHit;
                weaponTrigger.CriticalHit = attackRollTrigger.CriticalHit;
                weaponTrigger.OnlySneakAttack = attackRollTrigger.SneakAttack;
                weaponTrigger.CheckWeaponCategory = attackRollTrigger.CheckWeapon;
                weaponTrigger.Category = attackRollTrigger.WeaponCategory;
                weaponTrigger.Action = attackRollTrigger.Action;

                action(weaponTrigger);
            });
        }
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
            return EditComponent<BuildingCostModifier>(costModifier =>
            {
                if (buildingId.GetBlueprint(out BlueprintSettlementBuilding building) && !costModifier.Buildings.Contains(building))
                {
                    costModifier.Buildings = [.. costModifier.Buildings, building];
                }
            });
        }
    }

    public sealed class BlueprintSettlementBuildingConfigurator : BaseBlueprintFactConfigurator<BlueprintSettlementBuilding, BlueprintSettlementBuildingGuid, BlueprintSettlementBuildingConfigurator>
    {
        public BlueprintSettlementBuildingConfigurator SetAlignmentRestriction(AlignmentMaskType alignment)
        {
            RemoveComponents<AlignmentRestriction>(); // Some BPs have multiple, which is just invalid
            AddComponent<AlignmentRestriction>(c => c.Allowed = alignment);
            return this;
        }

        public BlueprintSettlementBuildingConfigurator SetOtherBuildRestriction(IEnumerable<BlueprintSettlementBuildingGuid> buildings, bool requireAll=false, bool inverted=false)
        {
            return EditOrAddComponent<OtherBuildingRestriction>(c =>
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

        public BlueprintSettlementBuildingConfigurator AddAdjacencyBonusBuildings(KingdomStats.Type type, params BlueprintSettlementBuildingGuid[] buildingIds)
        {
            return EditComponentWhere<BuildingAdjacencyBonus>(c => c.Stats[type] > 0, c =>
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
    }

    public class BlueprintKingdomEventConfigurator : BaseBlueprintObjectConfigurator<BlueprintKingdomEvent, BlueprintKingdomEventGuid, BlueprintKingdomEventConfigurator>
    {
        public BlueprintKingdomEventConfigurator EditPossibleSolution(LeaderType leaderType, Action<PossibleEventSolution> action)
        {
            if (Instance != null)
            {
                PossibleEventSolution solution = Instance.Solutions.Entries.FirstOrDefault(e => e.Leader == leaderType);
                if (solution != null)
                    AddOperation(_ => action(solution));
                else
                    Main.Log.Error($"Could not find a solution with leader \"{leaderType}\" in \"{GetDebugName()}\"");
            }

            return this;
        }

        public BlueprintKingdomEventConfigurator CopyPossibleSolutionResolutions(LeaderType fromLeader, LeaderType toLeader)
        {
            return AddOperation(_ =>
            {
                PossibleEventSolution fromSolution = Instance.Solutions.Entries.FirstOrDefault(e => e.Leader == fromLeader);
                PossibleEventSolution toSolution = Instance.Solutions.Entries.FirstOrDefault(e => e.Leader == toLeader);

                toSolution.Resolutions = fromSolution.Resolutions;
            });
        }

        public BlueprintKingdomEventConfigurator SetAutoResolve(EventResult.MarginType margin)
        {
            return AddOperation(i =>
            {
                i.AutoResolveResult = margin;
            });
        }
    }

    public sealed class BlueprintRandomEncounterConfigurator : BaseBlueprintObjectConfigurator<BlueprintRandomEncounter, BlueprintRandomEncounterGuid, BlueprintRandomEncounterConfigurator>
    {
        public BlueprintRandomEncounterConfigurator SetPool(EncounterPool pool)
        {
            return AddOperation(i =>
            {
                i.Pool = pool;
            });
        }
    }

    public sealed class BlueprintCueConfigurator : BaseBlueprintObjectConfigurator<BlueprintCue, BlueprintCueGuid, BlueprintCueConfigurator>
    {
        public BlueprintCueConfigurator AddOnShowAction(GameAction action)
        {
            return AddOperation(i =>
            {
                i.OnShow = ActionListFactory.Add(i.OnShow, action);
            });
        }

        public BlueprintCueConfigurator EditOnShowActionRecursive<ActionType>(string actionName, Action<ActionType> editAction)
            where ActionType : GameAction
        {
            if (Instance != null)
            {
                var action = Instance.OnShow.GetGameActionsRecursive().OfType<ActionType>().FirstOrDefault(a => a.name == actionName);
                if (action != null)
                    AddOperation(_ => editAction(action));
                else
                    Main.Log.Error($"Could not find an action of type \"{(typeof(ActionType).Name)}\" with name '{actionName}' on {GetDebugName()}");
            }

            return this;
        }

        public BlueprintCueConfigurator AddOnStopAction(GameAction action)
        {
            return AddOperation(i =>
            {
                i.OnStop = ActionListFactory.Add(i.OnStop, action);
            });
        }

        public BlueprintCueConfigurator EditOnStopActionWhere<ActionType>(Predicate<ActionType> pred, Action<ActionType> editAction)
            where ActionType : GameAction
        {
            if (Instance != null)
            {
                var action = Instance.OnStop.Actions.OfType<ActionType>().FirstOrDefault(a => pred == null || pred(a));
                if (action != null)
                    AddOperation(_ => editAction(action));
                else
                    Main.Log.Error($"Could not find an action of type \"{(typeof(ActionType).Name)}\" with condition on {GetDebugName()}");
            }

            return this;
        }

        public BlueprintCueConfigurator EditOnStopActionRecursiveWhere<ActionType>(Predicate<ActionType> pred, Action<ActionType> editAction)
            where ActionType : GameAction
        {
            if (Instance != null)
            {
                var action = Instance.OnStop.GetGameActionsRecursive().OfType<ActionType>().FirstOrDefault(a => pred == null || pred(a));
                if (action != null)
                    AddOperation(_ => editAction(action));
                else
                    Main.Log.Error($"Could not find an action of type \"{(typeof(ActionType).Name)}\" with condition on {GetDebugName()}");
            }

            return this;
        }

        public BlueprintCueConfigurator EditConditions(Action<ConditionsChecker> action)
        {
            return AddOperation(i =>
            {
                action(i.Conditions);
            });
        }
    }

    public sealed class BlueprintCheckConfigurator : BaseBlueprintObjectConfigurator<BlueprintCheck, BlueprintCheckGuid, BlueprintCheckConfigurator>
    {
        public BlueprintCheckConfigurator SetSkillType(StatType stat)
        {
            return AddOperation(i =>
            {
                i.Type = stat;
            });
        }

        public BlueprintCheckConfigurator EditDCModifierAt(int index, Action<DCModifier> action)
        {
            if (Instance != null)
            {
                if (index > 0 && index < Instance.DCModifiers.Length)
                {
                    var modifier = Instance.DCModifiers[index];
                    AddOperation(_ => action(modifier));
                }
                else
                {
                    Main.Log.Error($"Invalid DCModifier index {index}, length was {Instance.DCModifiers.Length}");
                }
            }

            return this;
        }
    }

    public sealed class BlueprintAnswerConfigurator : BaseBlueprintObjectConfigurator<BlueprintAnswer, BlueprintAnswerGuid, BlueprintAnswerConfigurator>
    {
        public BlueprintAnswerConfigurator EditOnSelectActions(Action<ActionList> action)
        {
            return AddOperation(i =>
            {
                action(i.OnSelect);
            });
        }
    }

    public sealed class BlueprintCharacterClassConfigurator : BaseBlueprintObjectConfigurator<BlueprintCharacterClass, BlueprintCharacterClassGuid, BlueprintCharacterClassConfigurator>
    {
        public BlueprintCharacterClassConfigurator SetAlignmentRestriction(AlignmentMaskType alignmentMask)
        {
            return EditOrAddComponent<PrerequisiteAlignment>(c =>
            {
                c.Alignment = alignmentMask; 
            });
        }

        public BlueprintCharacterClassConfigurator AddStartingItem(BlueprintItem itemBp)
        {
            return AddOperation(c =>
            {
                c.StartingItems = [.. c.StartingItems, itemBp];
            });
        }
    }

    public sealed class BlueprintWeaponTypeConfigurator : BaseBlueprintObjectConfigurator<BlueprintWeaponType, BlueprintWeaponTypeGuid, BlueprintWeaponTypeConfigurator>
    {
        public BlueprintWeaponTypeConfigurator SetTypeName(LocalizedString typeName)
        {
            return AddOperation(i =>
            {
                i.m_TypeNameText = typeName;
            });
        }

        public BlueprintWeaponTypeConfigurator SetDefaultName(LocalizedString defaultName)
        {
            return AddOperation(i =>
            {
                i.m_DefaultNameText = defaultName;
            });
        }

        public BlueprintWeaponTypeConfigurator SetIsLight(bool value)
        {
            return AddOperation(i =>
            {
                i.m_IsLight = value;
            });
        }

        public BlueprintWeaponTypeConfigurator SetFighterGroup(WeaponFighterGroup fighterGroup)
        {
            return AddOperation(i =>
            {
                i.m_FighterGroup = fighterGroup;
            });
        }

        public BlueprintWeaponTypeConfigurator EditVisualParameters(Action<WeaponVisualParameters> action)
        {
            return AddOperation(i =>
            {
                action(i.m_VisualParameters);
            });
        }
    }

    public sealed class BlueprintAbilityAreaEffectConfigurator : BaseBlueprintObjectConfigurator<BlueprintAbilityAreaEffect, BlueprintAbilityAreaEffectGuid, BlueprintAbilityAreaEffectConfigurator>
    {
        public BlueprintAbilityAreaEffectConfigurator SetAggroEnemies(bool value)
        {
            return AddOperation(i =>
            {
                i.AggroEnemies = value;
            });
        }

        public BlueprintAbilityAreaEffectConfigurator EditRoundActions(Action<ActionList> action)
        {
            return EditComponent<AbilityAreaEffectRunAction>(c =>
            {
                action(c.Round);
            });
        }

        public BlueprintAbilityAreaEffectConfigurator AddSpellDescriptor(SpellDescriptor descriptor)
        {
            return EditOrAddComponent<SpellDescriptorComponent>(c =>
            {
                c.Descriptor |= descriptor;
            });
        }
    }

    public class BlueprintQuestObjectiveConfigurator : BaseBlueprintFactConfigurator<BlueprintQuestObjective, BlueprintQuestObjectiveGuid, BlueprintQuestObjectiveConfigurator>
    {
        public BlueprintQuestObjectiveConfigurator SetFinishParent(bool value)
        {
            return AddOperation(i =>
            {
                i.m_FinishParent = value;
            });
        }
    }

    public class BlueprintCategoryDefaultsConfigurator : BaseBlueprintObjectConfigurator<BlueprintCategoryDefaults, BlueprintCategoryDefaultsGuid, BlueprintCategoryDefaultsConfigurator>
    {
        public BlueprintCategoryDefaultsConfigurator AddEntry(BlueprintCategoryDefaults.CategoryDefaultEntry entry)
        {
            return AddOperation(c =>
            {
                c.Entries = [.. c.Entries, entry];
            });
        }
    }

    public class BlueprintTrashLootSettingsConfigurator : BaseBlueprintObjectConfigurator<TrashLootSettings, BlueprintTrashLootSettingsGuid, BlueprintTrashLootSettingsConfigurator>
    {
        public BlueprintTrashLootSettingsConfigurator AddTypeEntry(TrashLootType type, params BlueprintItem[] items)
        {
            return AddOperation(s =>
            {
                var typeEntry = s.Types.FirstOrDefault(t => t.Type == type);
                if(typeEntry == null)
                {
                    typeEntry = new TrashLootSettings.TypeSettings { Type = type, Items = new() };
                    s.Types.Add(typeEntry);
                }

                typeEntry.Items.AddRange(items);
            });
        }
    }

    public abstract class BaseBlueprintUnitLootConfigurator<BPType, GuidType, TBuilder> : BaseBlueprintObjectConfigurator<BPType, GuidType, TBuilder>
        where BPType : BlueprintUnitLoot
        where GuidType : BlueprintUnitLootGuid, new()
        where TBuilder : BaseBlueprintUnitLootConfigurator<BPType, GuidType, TBuilder>, new()
    {

    }

    public sealed class BlueprintSharedVendorTableConfigurator : BaseBlueprintUnitLootConfigurator<BlueprintSharedVendorTable, BlueprintSharedVendorTableGuid, BlueprintSharedVendorTableConfigurator>
    {
        public BlueprintSharedVendorTableConfigurator AddItem(string persistenceGuid, BlueprintItem item, int count = 1)
        {
            return AddComponent<LootItemsPackFixed>(c =>
            {
                c.name = $"$LootItemsPackFixed${persistenceGuid}";
                c.m_Item = new LootItem
                {
                    m_Type = LootItemType.Item,
                    m_Item = item
                };
                c.m_Count = count;
            });
        }
    }
}
