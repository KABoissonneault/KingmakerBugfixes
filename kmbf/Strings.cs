//  Copyright 2025 Kévin Alexandre Boissonneault. Distributed under the Boost
//  Software License, Version 1.0. (See accompanying file
//  LICENSE_1_0.txt or copy at http://www.boost.org/LICENSE_1_0.txt)

using HarmonyLib;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
using Newtonsoft.Json;

namespace kmbf
{
    public class DefaultLocalizedStringData
    {
        [JsonProperty] public string Key;
        [JsonProperty] public string Value;
        [JsonProperty] public string[] ModExclusion;
        [JsonProperty] public string SettingCondition;
        [JsonProperty] public string Remove; // If set, the contents are removed from the string
        [JsonProperty] public string Target; // Operand used in the following operations
        [JsonProperty] public string Replace; // If set, Target is replaced with the contents of Replace
        [JsonProperty] public string Add; // If set, the contents are added after Target
    }

    public class ModLocalizedStringData
    {
        [JsonProperty] public string Key;
        [JsonProperty] public string Value;
    }

    static class ModLocalizationManager
    {
        private static readonly string RootKey = "kab.bug-fixes";
        private static Locale lastLoadedLocale = Locale.Sound;

        public static string CreateKey(string subKey)
        {
            return $"{RootKey}.{subKey}";
        }

        static void LoadLocalization()
        {
            if (LocalizationManager.CurrentLocale == lastLoadedLocale)
                return;
            
            try
            {
                LoadDefaultStringOverrides();
                LoadModStrings();
            }
            catch (Exception e)
            {
                Main.Log.LogException(e);
            }
            finally
            {
                lastLoadedLocale = LocalizationManager.CurrentLocale;
            }
        }

        static void LoadDefaultStringOverrides()
        {
            var currentLocale = LocalizationManager.CurrentLocale.ToString();
            var fileName = Path.Combine($"{Main.ModEntry.Path}", "Localization", $"DefaultStrings_{currentLocale}.json");

            if (!File.Exists(fileName))
            {
                return;
            }

            DefaultLocalizedStringData[] allStrings;
            using (var reader = new StreamReader(fileName))
            {
                using (var textReader = new JsonTextReader(reader))
                {
                    allStrings = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings).Deserialize<DefaultLocalizedStringData[]>(textReader);
                }
            }

            foreach (DefaultLocalizedStringData stringData in allStrings)
            {
                if (LocalizationManager.CurrentPack.Strings.TryGetValue(stringData.Key, out string currentValue))
                {
                    if (!string.IsNullOrEmpty(stringData.Value))
                    {
                        LocalizationManager.CurrentPack.Strings[stringData.Key] = stringData.Value;
                    }
                    else if(!string.IsNullOrEmpty(stringData.Target) && !string.IsNullOrEmpty(stringData.Replace))
                    {
                        LocalizationManager.CurrentPack.Strings[stringData.Key] = currentValue.Replace(stringData.Target, stringData.Replace);
                    }
                    else if (!string.IsNullOrEmpty(stringData.Target) && !string.IsNullOrEmpty(stringData.Add))
                    {
                        int index = currentValue.IndexOf(stringData.Target);
                        if (index < 0)
                        {
                            Main.Log.Warning($"Could not find target string '{stringData.Target}' for '{stringData.Key}' in LocalizationManager");
                        }
                        else
                        {
                            LocalizationManager.CurrentPack.Strings[stringData.Key] = currentValue.Insert(index + stringData.Target.Length, stringData.Add);
                        }
                    }
                    else if (!string.IsNullOrEmpty(stringData.Remove))
                    {
                        LocalizationManager.CurrentPack.Strings[stringData.Key] = currentValue.Replace(stringData.Remove, "");
                    }
                    else
                    {
                        Main.Log.Warning($"No operations done on '{stringData.Key}' in LocalizationManager");
                    }
                }
                else
                {
                    Main.Log.Error($"Could not find string with key '{stringData.Key}' in LocalizationManager");
                }
            }
        }

        static void LoadModStrings()
        {
            var currentLocale = LocalizationManager.CurrentLocale.ToString();
            var fileName = Path.Combine($"{Main.ModEntry.Path}", "Localization", $"Strings_{currentLocale}.json");

            if (!File.Exists(fileName))
            {
                Main.Log.Warning($"Localised text for current local \"{currentLocale}\" not found, falling back on enGB.");
                currentLocale = "enGB";
                fileName = $"{Main.ModEntry.Path}/Localization/Strings_enGB.json";
            }

            ModLocalizedStringData[] allStrings;
            using (var reader = new StreamReader(fileName))
            {
                using (var textReader = new JsonTextReader(reader))
                {
                    allStrings = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings).Deserialize<ModLocalizedStringData[]>(textReader);
                }
            }

            foreach (ModLocalizedStringData stringData in allStrings)
            {
                LocalizationManager.CurrentPack.Strings[CreateKey(stringData.Key)] = stringData.Value;
            }
        }


        // Hot loading the mod misses locale assignment. Try on mod load if the systems are already on and ready
        public static void TryLoadingStringsOnLoad()
        {
            if(LocalizationManager.CurrentLocale != Locale.Sound && LocalizationManager.CurrentPack != null && LocalizationManager.CurrentPack.Strings != null)
            {
                LoadLocalization();
            }
        }

        [HarmonyPatch(typeof(LocalizationManager), "CurrentLocale", MethodType.Setter)]
        static class LocalizationManager_CurrentLocale_Patch
        {
            [HarmonyPostfix]
            static void Postfix()
            {
                LoadLocalization();
            }
        }
    }

    static class KMLocalizedStrings
    {
        public static LocalizedString Spit = new LocalizedString() { m_Key = "7063a6bf-494d-4c74-9e62-e35aa0a78215" };
        public static LocalizedString BiteTypeName = new LocalizedString() { m_Key = "9d57a71c-033d-4879-b6bf-d5eee7a6a7b5" };
        public static LocalizedString BiteDefaultName = new LocalizedString() { m_Key = "3f2e2ab3-c69d-4fb7-afa5-7d2f3c02b238" };
        public static LocalizedString TouchTypeName = new LocalizedString() { m_Key = "c58950db-269e-4aa9-b984-f05e1ef31302" };
        public static LocalizedString TouchDefaultName = new LocalizedString() { m_Key = "c7e2893d-dcb5-4c8c-8415-a908b048b329" };
        public static LocalizedString RockTypeName = new LocalizedString() { m_Key = "cd8f12e9-36db-498c-b3a3-f52a7f4b5f52" };
        public static LocalizedString RockDefaultName = new LocalizedString() { m_Key = "b685d2aa-a2b3-4ba1-81de-7867fa10434d" };
        
        public static LocalizedString AmethystRing = new LocalizedString() { m_Key = "09c2b1ec-ffb5-4e26-91a0-3840221676bf" };
        public static LocalizedString GarnetRing = new LocalizedString() { m_Key = "3fa6ae88-bc24-44d5-ac74-109d250df757" };
        public static LocalizedString TormentorDisplayName = new LocalizedString() { m_Key = "8ea2ec9c-970f-4960-ae92-ffda5fd5b414" };
        public static LocalizedString TormentorDescription = new LocalizedString() { m_Key = "c1c88d7c-35d1-4456-8f0a-0b5e71a951ef" };
        public static LocalizedString GameKeeperOfTheFirstWorldName = new LocalizedString() { m_Key = "92d1a8a9-e259-4957-b89a-b90836939755" };
        public static LocalizedString GameKeeperOfTheFirstWorldDescription = new LocalizedString() { m_Key = "2188c76f-2c99-49aa-806a-5e06f64e2b7a" };

    }

    static class KMBFLocalizedStrings
    {        
        public static LocalizedString CreateString(string key)
        {
            return new LocalizedString() 
            {
                m_Key = ModLocalizationManager.CreateKey(key)
            };
        }

        public static LocalizedString Tongue = CreateString("tongue");
    }
}
