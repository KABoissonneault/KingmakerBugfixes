using HarmonyLib;
using Kingmaker;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using kmbf.Patch;
using kmbf.Patch.BP;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace kmbf;

class JsonTraceWriter : ITraceWriter
{
    public TraceLevel LevelFilter => TraceLevel.Error;

    public void Trace(TraceLevel level, string message, Exception ex)
    {
        Main.Log.Log($"Json [{level}]: {message}");
    }
}

public static class Main 
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;
    internal static UMMSettings UMMSettings;
    internal static UnityModManager.ModEntry ModEntry;
    
    static bool runsCallOfTheWild = false;
    internal static bool RunsCallOfTheWild { get => runsCallOfTheWild; }

    static bool loadedGui = false;
    static GUIStyle guiStyleWarning;

    public static bool Load(UnityModManager.ModEntry modEntry) 
    {
#if DEBUG
        Harmony.DEBUG = true;
        modEntry.OnUpdate += OnUpdate;
#endif

        ModEntry = modEntry;
        Log = modEntry.Logger;
        UMMSettings = UnityModManager.ModSettings.Load<UMMSettings>(modEntry);
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;

        runsCallOfTheWild = UnityModManager.modEntries.Any(mod => mod.Info.Id.Equals("CallOfTheWild") && mod.Enabled && !mod.ErrorOnLoading);

        // This helps debug issues with loading saves
        if (DefaultJsonSettings.DefaultSettings.TraceWriter == null)
        {
            DefaultJsonSettings.DefaultSettings.TraceWriter = new JsonTraceWriter();
        }

        ModLocalizationManager.TryLoadingStringsOnLoad();

        HarmonyInstance = new Harmony(modEntry.Info.Id);

        try
        {
            Main.Log.Log("Running code patches");
            HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        }
        catch 
        {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        
        return true;
    }

#if DEBUG
    static void OnUpdate(UnityModManager.ModEntry modEntry, float z)
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            var rule = new Kingmaker.RuleSystem.Rules.RuleSkillCheck(Game.Instance.Player.MainCharacter.Value, Kingmaker.EntitySystem.Stats.StatType.Charisma, 25);
            Kingmaker.RuleSystem.Rulebook.Trigger(rule);
        }
    }
#endif

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
        if (!loadedGui)
        {
            guiStyleWarning = new GUIStyle(GUI.skin.label);
            guiStyleWarning.normal.textColor = Color.red;

            loadedGui = true;
        }

        if (UMMSettings.FixesDirty)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(KMBFLocalizedStrings.CreateString("mod-dirty-warning"), guiStyleWarning);
            GUILayout.EndHorizontal();
        }

        if (Game.Instance.Player.ControllableCharacters.Any())
        {
            if (!LibraryScriptableObject_LoadDictionary_Patch.Loaded)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(KMBFLocalizedStrings.CreateString("mod-load-warning"), guiStyleWarning);
                GUILayout.EndHorizontal();
            }
        }

        UMMSettings.Draw(modEntry);

        if (Game.Instance.Player.ControllableCharacters.Any())
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(KMBFLocalizedStrings.CreateString("saves-fixes"));
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(KMBFLocalizedStrings.CreateString("apply-fixes"), GUILayout.ExpandWidth(false), GUILayout.MinHeight(20.0f)))
            {
                SaveFixes.Apply();
            }
            GUILayout.EndHorizontal();
        }
    }

    static void OnSaveGUI(UnityModManager.ModEntry modEntry)
    {
        UMMSettings.Save(modEntry);
    }
}
