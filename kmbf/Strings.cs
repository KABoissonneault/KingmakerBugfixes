using Kingmaker.Localization;

namespace kmbf
{ 
    static class KMLocalizedStrings
    {
        public static LocalizedString Spit = new LocalizedString() { m_Key = "7063a6bf-494d-4c74-9e62-e35aa0a78215" };
    }

    static class KMBFLocalizedStrings
    {
        private static readonly string RootKey = "kab.bug-fixes";

        private static string CreateKey(string partialKey)
        {
            return $"{RootKey}.{partialKey}";
        }

        private static LocalizedString CreateString(string key, string value)
        {
            LocalizedString result = new()
            {
                m_Key = key
            };
            LocalizationManager.CurrentPack.Strings[key] = value;
            return result;
        }

        public static LocalizedString OozeSpitDescription = CreateString(CreateKey("ooze-spit-description"), "Ranged touch attack which deals Acid damage and may cause Nauseated.");
    }
}
