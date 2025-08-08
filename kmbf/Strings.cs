using HarmonyLib;
using Kingmaker.EntitySystem.Persistence.JsonUtility;
using Kingmaker.Localization;
using Kingmaker.Localization.Shared;
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
        private static Locale lastLoadedLocale = Locale.Sound;

        public static string CreateKey(string subKey)
        {
            return $"{RootKey}.{subKey}";
        }

        static void LoadLocalization()
        {
            if (LocalizationManager.CurrentLocale == lastLoadedLocale)
                return;

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
            finally
            {
                lastLoadedLocale = LocalizationManager.CurrentLocale;
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
        public static LocalizedString AmethystRing = new LocalizedString() { m_Key = "09c2b1ec-ffb5-4e26-91a0-3840221676bf" };
        public static LocalizedString GarnetRing = new LocalizedString() { m_Key = "3fa6ae88-bc24-44d5-ac74-109d250df757" };
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

        public static LocalizedString OozeSpitDescription = CreateString("ooze-spit-description");
        public static LocalizedString Tongue = CreateString("tongue");
    }
}
