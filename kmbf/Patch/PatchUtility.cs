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

        static bool CheckMod(string patchName, ModExclusionFlags modExclusionFlags)
        {
            if ((modExclusionFlags & ModExclusionFlags.CallOfTheWild) == ModExclusionFlags.CallOfTheWild && Main.RunsCallOfTheWild)
            {
                Main.Log.Log($"Skip patching '{patchName}': Running Call of the Wild");
                return false;
            }

            return true;
        }
        
        public static bool StartPatch(string patchName, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
        {
            if (!CheckMod(patchName, modExclusionFlags))
            {
                return false;
            }

            Main.Log.Log($"Patching '{patchName}'");
            return true;
        }

        public static bool StartBalancePatch(string patchName, string balanceFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
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
                Main.Log.Log($"Skip patching '{patchName}': '{balanceFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags))
            {
                return false;
            }

            Main.Log.Log($"Patching '{patchName}': '{balanceFlag}' on");
            return true;
        }

        public static bool StartQualityOfLifePatch(string patchName, string qolFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
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
                Main.Log.Log($"Skip patching '{patchName}': '{qolFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags))
            {
                return false;
            }

            Main.Log.Log($"Patching '{patchName}': '{qolFlag}' on");
            return true;
        }

        public static bool StartEventPatch(string patchName, string eventFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
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
                Main.Log.Log($"Skip patching '{patchName}': '{eventFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags))
            {
                return false;
            }

            Main.Log.Log($"Patching '{patchName}': '{eventFlag}' on");
            return true;
        }

        public static bool StartAddedContentPatch(string patchName, string addedContentFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
        {
            FieldInfo addedContentField = typeof(AddedContentSettings).GetField(addedContentFlag);
            if (addedContentField == null)
            {
                Main.Log.Error($"Invalid flag value '{addedContentField}'");
                return false;
            }

            bool addedContentValue = (bool)addedContentField.GetValue(Main.UMMSettings.AddedContentSettings);
            if (!addedContentValue)
            {
                Main.Log.Log($"Skip patching '{patchName}': '{addedContentFlag}' off");
                return false;
            }

            if (!CheckMod(patchName, modExclusionFlags))
            {
                return false;
            }

            Main.Log.Log($"Patching '{patchName}': '{addedContentFlag}' on");
            return true;
        }

        // Utility for the Prepare method of Harmony patches
        // Prepare is called multiple times per patch, but the initial call has the "original" parameter as null
        // Use this to identify the first call
        public static bool StartPreparePatch(string patchName, MethodBase original, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
        {
            if(original != null)
            {
                return true;
            }

            return StartPatch(patchName, modExclusionFlags);
        }

        public static bool StartPrepareBalancePatch(string patchName, MethodBase original, string balanceFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
        {
            if(original != null)
            {
                return true;
            }

            return StartBalancePatch(patchName, balanceFlag, modExclusionFlags);
        }

        public static bool StartPrepareQualityOfLifePatch(string patchName, MethodBase original, string qolFlag, ModExclusionFlags modExclusionFlags = ModExclusionFlags.None)
        {
            if (original != null)
            {
                return true;
            }

            return StartQualityOfLifePatch(patchName, qolFlag, modExclusionFlags);
        }
    }
}
