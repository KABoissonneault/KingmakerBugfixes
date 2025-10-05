using System.Reflection;

namespace kmbf.Patch
{
    internal static class PatchUtility
    {
        public enum ModExclusionFlags
        {
            None,
            CallOfTheWild = 1 << 0,
        }

        static HashSet<string> logOncePatches = new HashSet<string>();

        static void DoLog(string patchName, bool logOnce, string log)
        {
            if (logOnce)
            {
                if (!logOncePatches.Add(patchName))
                    return;
            }

            Main.Log.Log(log);
        }

        static bool CheckMod(string patchName, ModExclusionFlags modExclusionFlags, bool logOnce)
        {
            if ((modExclusionFlags & ModExclusionFlags.CallOfTheWild) == ModExclusionFlags.CallOfTheWild && Main.RunsCallOfTheWild)
            {
                DoLog(patchName, logOnce, $"Skip patching '{patchName}': Running Call of the Wild");
                return false;
            }

            return true;
        }
        
        public static bool StartPatch(string patchName, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None, bool logOnce = false)
        {
            if (!CheckMod(patchName, modExclusionFlags, logOnce))
            {
                return false;
            }

            DoLog(patchName, logOnce, $"Patching '{patchName}'");
            return true;
        }

        public static bool StartBalancePatch(string patchName, string balanceFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None, bool logOnce = false)
        {
            FieldInfo balanceFlagField = typeof(BalanceSettings).GetField(balanceFlag);
            if (balanceFlagField == null)
            {
                Main.Log.Error($"Invalid flag value '{balanceFlag}'");
                return false;
            }

            bool balanceFlagValue = (bool)balanceFlagField.GetValue(Main.UMMSettings.BalanceSettings);
            if (!balanceFlagValue)
            {
                DoLog(patchName, logOnce, $"Skip patching '{patchName}': '{balanceFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags, logOnce))
            {
                return false;
            }

            DoLog(patchName, logOnce, $"Patching '{patchName}': '{balanceFlag}' on");
            return true;
        }

        public static bool StartQualityOfLifePatch(string patchName, string qolFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None, bool logOnce = false)
        {
            FieldInfo qolField = typeof(QualityOfLifeSettings).GetField(qolFlag);
            if (qolField == null)
            {
                Main.Log.Error($"Invalid flag value '{qolFlag}'");
                return false;
            }

            bool qolValue = (bool)qolField.GetValue(Main.UMMSettings.QualityOfLifeSettings);
            if (!qolValue)
            {
                DoLog(patchName, logOnce, $"Skip patching '{patchName}': '{qolFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags, logOnce))
            {
                return false;
            }

            DoLog(patchName, logOnce, $"Patching '{patchName}': '{qolFlag}' on");
            return true;
        }

        public static bool StartEventPatch(string patchName, string eventFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None, bool logOnce = false)
        {
            FieldInfo eventField = typeof(EventSettings).GetField(eventFlag);
            if (eventField == null)
            {
                Main.Log.Error($"Invalid flag value '{eventFlag}'");
                return false;
            }

            bool eventValue = (bool)eventField.GetValue(Main.UMMSettings.EventSettings);
            if (!eventValue)
            {
                DoLog(patchName, logOnce, $"Skip patching '{patchName}': '{eventFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags, logOnce))
            {
                return false;
            }

            DoLog(patchName, logOnce, $"Patching '{patchName}': '{eventFlag}' on");
            return true;
        }
    }
}
