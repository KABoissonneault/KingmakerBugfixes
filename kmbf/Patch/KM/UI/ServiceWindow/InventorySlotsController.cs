using HarmonyLib;
using Kingmaker;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UI;
using Kingmaker.UI.Common;
using Kingmaker.UI.GenericSlot;
using Kingmaker.UI.ServiceWindow;
using System.Reflection;

using static kmbf.Patch.PatchUtility;

namespace kmbf.Patch.KM.UI.ServiceWindow
{
    [HarmonyPatch(typeof(InventorySlotsController))]
    static class InventorySlotsController_Patch
    {
        [HarmonyPrepare]
        static bool Prepare(MethodBase original)
        {
            return StartPrepareBalancePatch("Amiri Oversized Sword with Offhand Shield", original, nameof(BalanceSettings.FixAmiriOversizedSwordPlusShield));
        }

        // We want to handle the case where we Put or Swap an item into a secondary weapon slot,
        // and check if the primary weapon slot is two-handed and locked
        [HarmonyPatch(nameof(InventorySlotsController.HandleSlotDragEnd)), HarmonyPrefix]
        static bool Prefix(InventorySlotsController __instance, Kingmaker.UI.ServiceWindow.ItemSlot from, Kingmaker.UI.ServiceWindow.ItemSlot to)
        {
            if(from.Item == null)
            {                
                return true;
            }

            switch (UIUtility.GetEndDragAction(from, to))
            {
                case UIUtility.EndDragAction.Split:
                case UIUtility.EndDragAction.HalfSplit:
                case UIUtility.EndDragAction.Merge:
                    return true;

                // We only want to handle this case
                // Replicate the start of the function, then apply our modification
                default:
                    // Function prefix
                    __instance.m_DragSlot = null;
                    __instance.m_Inventory.Doll.SetPossibleTargetsHighlighted(from.Item, highlighted: false);
                    __instance.m_Inventory.Filter.SetSorter();

                    // Put/Swap operation
                    Kingmaker.Items.Slots.ItemSlot itemSlot = (from as EquipSlotBase)?.Slot;
                    if (itemSlot != null && !itemSlot.CanRemoveItem())
                    {
                        itemSlot.Owner?.Unit?.View?.Asks?.RefuseUnequip.Schedule();
                        Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                        return false;
                    }

                    Kingmaker.Items.Slots.ItemSlot itemSlot2 = (to as EquipSlotBase)?.Slot;
                    if (itemSlot2 != null && !itemSlot2.CanRemoveItem())
                    {
                        itemSlot2.Owner?.Unit?.View?.Asks?.RefuseUnequip.Schedule();
                        Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                        return false;
                    }

                    if (itemSlot != null && to.Item != null && !itemSlot.CanInsertItem(to.Item))
                    {
                        itemSlot.Owner?.Unit?.View?.Asks?.RefuseEquip.Schedule();
                        Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                        return false;
                    }

                    if (itemSlot2 != null && !itemSlot2.CanInsertItem(from.Item))
                    {
                        itemSlot2.Owner?.Unit?.View?.Asks?.RefuseEquip.Schedule();
                        Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                        return false;
                    }

                    // Patch: Check for hand slot. If so, check the pair slot.
                    // If the pair is two-handed and cannot be removed, refuse to equip
                    HandSlot pairSlot = (itemSlot2 as HandSlot)?.PairSlot;
                    if (pairSlot != null && pairSlot.HasItem 
                        && pairSlot.MaybeItem is ItemEntityWeapon otherHandWeapon && (otherHandWeapon.Blueprint.IsTwoHanded || otherHandWeapon.Blueprint.Double)
                        && !pairSlot.CanRemoveItem())
                    {
                        itemSlot2.Owner?.Unit?.View?.Asks?.RefuseEquip.Schedule();
                        Game.Instance.UI.Common.UISound.Play(UISoundType.ErrorEquip);
                        return false;
                    }
                    // Patch end

                    ItemEntity item = from.Item;
                    ItemEntity item2 = to.Item;
                    if (itemSlot != null && itemSlot.HasItem)
                    {
                        itemSlot.RemoveItem(autoMerge: false);
                    }

                    if (itemSlot2 != null && itemSlot2.HasItem)
                    {
                        itemSlot2.RemoveItem(autoMerge: false);
                    }

                    if (to.Item != null)
                    {
                        itemSlot?.InsertItem(to.Item);
                    }

                    itemSlot2?.InsertItem(from.Item);
                    if (item2 != null && item2.TryMerge(item))
                    {
                        from.SetItem(null);
                    }
                    else if (itemSlot == null && itemSlot2 == null)
                    {
                        from.SetItem(item2);
                        to.SetItem(item);
                    }
                    else if (itemSlot != null && itemSlot2 == null)
                    {
                        from.SetItem(itemSlot.MaybeItem);
                        if (item2 == null || item2.HoldingSlot != null)
                        {
                            to.SetItem(item);
                        }
                        else
                        {
                            InventorySlotsController.TryAutoMerge(item);
                        }
                    }
                    else if (itemSlot == null && itemSlot2 != null)
                    {
                        to.SetItem(itemSlot2.MaybeItem);
                        if (item == null || item.HoldingSlot != null)
                        {
                            from.SetItem(item2);
                        }
                        else
                        {
                            InventorySlotsController.TryAutoMerge(item2);
                        }
                    }
                    else
                    {
                        to.SetItem(itemSlot2.MaybeItem);
                        from.SetItem(itemSlot.MaybeItem);
                    }

                    Game.Instance.UI.Common.UISound.PlayItemSound(SlotAction.Put, to.Item, itemSlot2 != null);

                    __instance.m_Inventory.Filter.SetSorter();
                    __instance.RefreshSlots();

                    return false;
            }
        }
    }
}
