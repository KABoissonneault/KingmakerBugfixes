using HarmonyLib;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic;
using System.Reflection;
using System.Reflection.Emit;

namespace kmbf.Patch
{
    [HarmonyPatch(typeof(UnitBody), "CurrentHandsEquipmentSet", MethodType.Getter)]
    static class UnitBody_CurrentHandsEquipmentSet_Patch
    {
        [HarmonyPostfix]
        public static HandsEquipmentSet CurrentHandsEquipmentSet(HandsEquipmentSet normalSet, UnitBody __instance)
        {
            return __instance.m_PolymoprphHandsEquipmentSet ?? normalSet;
        }
    }

    [HarmonyPatch(typeof(UnitBody), "GetHandsEquipmentSet")]
    static class UnitBody_GetHandsEquipmentSet_Patch
    {
        [HarmonyPrefix]
        public static bool GetHandsEquipmentSet(UnitBody __instance, HandSlot slot, ref HandsEquipmentSet __result)
        {
            if (__instance.IsPolymorphed && (__instance.m_PolymoprphHandsEquipmentSet.PrimaryHand == slot || __instance.m_PolymoprphHandsEquipmentSet.SecondaryHand == slot))
            {
                __result = __instance.m_PolymoprphHandsEquipmentSet;
                return false;
            }
            for (int i = 0; i < __instance.m_HandsEquipmentSets.Length; i++)
            {
                HandsEquipmentSet handsEquipmentSet = __instance.m_HandsEquipmentSets[i];
                if (handsEquipmentSet.PrimaryHand == slot || handsEquipmentSet.SecondaryHand == slot)
                {
                    __result = handsEquipmentSet;
                    return false;
                }
            }
            return false;
        }
    }

    [HarmonyPatch(typeof(UnitBody), "PreSave")]
    static class UnitBody_PreSave_Patch
    {
        [HarmonyPostfix]
        public static void PreSave(UnitBody __instance)
        {
            if (__instance.m_PolymoprphHandsEquipmentSet != null)
            {
                __instance.m_PolymoprphHandsEquipmentSet.PrimaryHand?.PreSave();
                __instance.m_PolymoprphHandsEquipmentSet.SecondaryHand?.PreSave();
            }

            if (__instance.m_AdditionalLimbs != null)
            {
                foreach (var slot in __instance.m_AdditionalLimbs)
                    slot.PreSave();
            }
        }
    }

    [HarmonyPatch(typeof(UnitBody), "PostLoad")]
    static class UnitBody_PostLoad_Patch
    {
        [HarmonyPostfix]
        public static void PostLoad(UnitBody __instance)
        {
            if (__instance.m_PolymoprphHandsEquipmentSet != null)
            {
                __instance.m_PolymoprphHandsEquipmentSet.PrimaryHand?.PostLoad();
                __instance.m_PolymoprphHandsEquipmentSet.SecondaryHand?.PostLoad();
            }

            if (__instance.m_AdditionalLimbs != null)
            {
                foreach (var slot in __instance.m_AdditionalLimbs)
                    slot.PostLoad();
            }
        }
    }

    [HarmonyPatch(typeof(UnitBody), "Dispose")]
    static class UnitBody_Dispose_Patch
    {
        [HarmonyPostfix]
        public static void Dispose(UnitBody __instance)
        {
            if (__instance.m_PolymoprphHandsEquipmentSet != null)
            {
                __instance.m_PolymoprphHandsEquipmentSet.PrimaryHand?.Dispose();
                __instance.m_PolymoprphHandsEquipmentSet.SecondaryHand?.Dispose();
            }

            if (__instance.m_AdditionalLimbs != null)
            {
                foreach (var slot in __instance.m_AdditionalLimbs)
                    slot.Dispose();
            }
        }
    }

    [HarmonyPatch(typeof(ItemEntityWeapon), "Dispose")]
    static class ItemEntityWeapon_Dispose_Patch
    {
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> Dispose(IEnumerable<CodeInstruction> instructions)
        {
            MethodInfo ItemEntity_PostLoad = AccessTools.Method(typeof(ItemEntity), "PostLoad");
            MethodInfo ItemEntity_Dispose = AccessTools.Method(typeof(ItemEntity), "Dispose");

            List<CodeInstruction> newInstructions = new List<CodeInstruction>();

            foreach (var instr in instructions)
            {
                if(instr.Calls(ItemEntity_PostLoad))
                {
                    var newInstr = new CodeInstruction(OpCodes.Callvirt, ItemEntity_Dispose);
                    newInstr.labels = instr.labels;
                    newInstructions.Add(newInstr);
                }
                else
                {
                    newInstructions.Add(instr);
                }
            }

            return newInstructions;
        }
    }

    [HarmonyPatch(typeof(UnitDescriptor), "TurnOn")]
    static class UnitDescriptor_TurnOn_Patch
    {
        [HarmonyPostfix]
        public static void TurnOn(UnitDescriptor __instance)
        {
            if(__instance.Body.m_PolymoprphHandsEquipmentSet != null)
            {
                __instance.Body.m_PolymoprphHandsEquipmentSet.PrimaryHand?.MaybeItem?.TurnOn();
                __instance.Body.m_PolymoprphHandsEquipmentSet.SecondaryHand?.MaybeItem?.TurnOn();
            }

            if (__instance.Body.m_AdditionalLimbs != null)
            {
                foreach (var slot in __instance.Body.m_AdditionalLimbs)
                    slot.MaybeItem?.TurnOn();
            }

            // Could be necessary for the Feral Mutagen fix. WotR does it
            // Not sufficient for now, so not enabling it
            //foreach (var emptyHand in __instance.Body.AllEmptyHands)
            //    emptyHand?.TurnOn();
        }
    }

    [HarmonyPatch(typeof(UnitDescriptor), "TurnOff")]
    static class UnitDescriptor_TurnOff_Patch
    {
        [HarmonyPostfix]
        public static void TurnOff(UnitDescriptor __instance, bool force)
        {
            if (__instance.Body.m_PolymoprphHandsEquipmentSet != null)
            {
                __instance.Body.m_PolymoprphHandsEquipmentSet.PrimaryHand?.MaybeItem?.TurnOff(force);
                __instance.Body.m_PolymoprphHandsEquipmentSet.SecondaryHand?.MaybeItem?.TurnOff(force);
            }

            if (__instance.Body.m_AdditionalLimbs != null)
            {
                foreach (var slot in __instance.Body.m_AdditionalLimbs)
                    slot.MaybeItem?.TurnOff(force);
            }

            // Could be necessary for the Feral Mutagen fix. WotR does it
            // Not sufficient for now, so not enabling it
            //foreach (var emptyHand in __instance.Body.AllEmptyHands)
            //    emptyHand?.TurnOff(force);
        }
    }
}
