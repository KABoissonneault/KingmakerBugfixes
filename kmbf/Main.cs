using HarmonyLib;
using Kingmaker;
using kmbf.Patch;
using System.Reflection;
using UnityEngine;
using UnityModManagerNet;

namespace kmbf;

public static class Main 
{
    internal static Harmony HarmonyInstance;
    internal static UnityModManager.ModEntry.ModLogger Log;
    internal static UMMSettings UMMSettings;
    internal static UnityModManager.ModEntry ModEntry;

    internal static bool RunsCallOfTheWild = false;

    public static bool Load(UnityModManager.ModEntry modEntry) {
        ModEntry = modEntry;
        Log = modEntry.Logger;
        UMMSettings = UnityModManager.ModSettings.Load<UMMSettings>(modEntry);
        modEntry.OnGUI = OnGUI;
        modEntry.OnSaveGUI = OnSaveGUI;

        RunsCallOfTheWild = UnityModManager.modEntries.Any(mod => mod.Info.Id.Equals("CallOfTheWild") && mod.Enabled && !mod.ErrorOnLoading);

        try {
            HarmonyInstance = new Harmony(modEntry.Info.Id);
        } catch {
            HarmonyInstance.UnpatchAll(HarmonyInstance.Id);
            throw;
        }
        HarmonyInstance.PatchAll(Assembly.GetExecutingAssembly());
        return true;
    }

    static void OnGUI(UnityModManager.ModEntry modEntry)
    {
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
