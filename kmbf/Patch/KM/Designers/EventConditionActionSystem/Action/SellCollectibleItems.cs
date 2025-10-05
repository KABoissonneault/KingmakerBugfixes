using HarmonyLib;
using Kingmaker;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;

namespace kmbf.Patch.KM.Designers.EventConditionActionSystem.Action
{
    [HarmonyPatch(typeof(SellCollectibleItems), nameof(SellCollectibleItems.RunAction))]
    static class SellCollectibleItems_RunAction_Prefix
    {
        [HarmonyPrepare]
        static bool Prepare()
        {
            return PatchUtility.StartPatch("Storyteller Stacked Artifacts", logOnce: true);
        }

        [HarmonyPrefix]
        static bool RunAction(SellCollectibleItems __instance)
        {
            int itemCost = ((!__instance.HalfPrice) ? __instance.ItemToSell.Cost : (__instance.ItemToSell.Cost / 2));
            int itemCount = GameHelper.GetPlayerCharacter().Inventory.Filter(__instance.ItemToSell).Sum(i => i.Count);
            if (itemCount > 0)
            {
                GameHelper.GetPlayerCharacter().Inventory.Remove(__instance.ItemToSell, itemCount);
                GameHelper.GetPlayerCharacter().Inventory.Add(Game.Instance.BlueprintRoot.CoinItem, itemCount * itemCost);
            }

            return false;
        }
    }
}
