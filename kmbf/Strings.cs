using HarmonyLib;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization;
using Newtonsoft.Json;
using System.IO;

namespace kmbf
{     
    public class ModLocalizedStringData
    {
        [JsonProperty] public string Key;
        [JsonProperty] public string Value;
    }

    static class ModLocalizationManager
    {
        private static readonly string RootKey = "kab.bug-fixes";
        
        public static string CreateKey(string subKey)
        {
            return $"{RootKey}.{subKey}";
        }

        static void LoadLocalization()
        {
            var currentLocale = LocalizationManager.CurrentLocale.ToString();
            var fileName = $"{Main.ModEntry.Path}/Localization/Strings_{currentLocale}.json";

            if (!File.Exists(fileName))
            {
                Main.Log.Warning($"Localised text for current local \"{currentLocale}\" not found, falling back on enGB.");
                currentLocale = "enGB";
                fileName = $"{Main.ModEntry.Path}/Localization/Strings_enGB.json";
            }

            try
            {
                ModLocalizedStringData[] allStrings;
                using (var reader = new StreamReader(fileName))
                {
                    using (var textReader = new JsonTextReader(reader))
                    {
                        allStrings = JsonSerializer.Create(DefaultJsonSettings.DefaultSettings).Deserialize<ModLocalizedStringData[]>(textReader);
                    }
                }

                foreach(ModLocalizedStringData stringData in allStrings)
                {
                    LocalizationManager.CurrentPack.Strings[CreateKey(stringData.Key)] = stringData.Value;
                }
            }
            catch(Exception e)
            {
                Main.Log.LogException(e);
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
    }

    static class KMBFLocalizedStrings
    {        
        private static LocalizedString CreateString(string key)
        {
            return new LocalizedString() 
            {
                m_Key = ModLocalizationManager.CreateKey(key)
            };
        }

        public static LocalizedString OozeSpitDescription = CreateString("ooze-spit-description");
        public static LocalizedString Tongue = CreateString("tongue");
        public static LocalizedString GiantslugTongueDescription = CreateString("giant-slug-tongue-description");
    }
}
