using Il2Cpp;

namespace CunningSurvivor
{
    internal class Patches
    {
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData), new Type[] { typeof(string), typeof(string) })]
        private static class SaveGameSystem_LoadSceneData
        {
            private static void Postfix(string name, string sceneSaveName)
            {
                BPUtils.DebugMsg("SaveGameSystem_LoadSceneData (" + name + "|" + sceneSaveName + ")");
                BPCarryCapacity.Update();
            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGame), new Type[] { typeof(string), typeof(string) })]
        private static class SaveGameSystem_SaveGame
        {
            private static void Prefix(string name, string sceneSaveName)
            {
                BPMain.PickupBackpack(true);
                BPUtils.DebugMsg("SaveGameSystem_SaveGame (" + name + "|" + sceneSaveName + ")");

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.CancelPlaceMesh))]
        private static class PlayerManager_CancelPlaceMesh
        {
            private static void Postfix()
            {
                if (BPMain.backpackPlacing == true)
                {
                    BPUtils.DebugMsg("PlayerManager_CancelPlaceMesh");
                    BPMain.PickupBackpack(true);
                    BPMain.backpackPlacing = false;
                }

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.AttemptToPlaceMesh))]
        private static class PlayerManager_AttemptToPlaceMesh
        {
            private static void Postfix(PlayerManager __instance)
            {
                if (!__instance.m_ObjectToPlace)
                {
                    return;
                }
                if (!__instance.CanPlaceCurrentPlaceable())
                {
                    return;
                }

                if (BPMain.backpackPlacing == true)
                {
                    BPMain.PlaceBackpackComplete();
                }

            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.InteractiveObjectsProcessAltFire))]
        internal class PlayerManager_InteractiveObjectsProcessAltFire
        {
            private static bool Prefix()
            {
                GameObject ItemUnderCrosshair = Utility.GetObjectUnderCrosshairST();

                if (ItemUnderCrosshair)
                {
                    MelonLogger.Msg(ItemUnderCrosshair);
                    MelonLogger.Msg(ItemUnderCrosshair.name);
                    if (ItemUnderCrosshair == BPMain.backpackParent)
                    {
                        BPUtils.DebugMsg("Item Under Crosshair = " + ItemUnderCrosshair.name);

                        GameManager.GetPlayerManagerComponent().StartPlaceMesh(ItemUnderCrosshair, PlaceMeshFlags.None);
                        return false;
                    }
                }
                return true;
            }

        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.PutOnClothingItem), new Type[] { typeof(GearItem), typeof(ClothingLayer) })]
        internal class PlayerManager_PutOnClothingItem
        {
            private static void Postfix(GearItem gi)
            {
                BPUtils.DebugMsg("PlayerManager_PutOnClothingItem | " + gi.m_ClothingItem.name + "|" + gi.m_ClothingItem.m_Region + "|" + gi.m_ClothingItem.m_EquippedLayer);
                BPCarryCapacity.Update();
            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.TakeOffClothingItem))]
        internal class PlayerManager_TakeOffClothingItem
        {
            private static void Postfix(GearItem gi)
            {
                BPUtils.DebugMsg("PlayerManager_TakeOffClothingItem | " + gi.m_ClothingItem.name + "|" + gi.m_ClothingItem.m_Region + "|" + gi.m_ClothingItem.m_EquippedLayer + " | " + gi.m_ClothingItem.IsWearing());
                BPCarryCapacity.Update();
            }
        }

        [HarmonyPatch(typeof(Inventory), nameof(Inventory.GetNonRuinedItem), new Type[] { typeof(string) })]
        internal class Inventory_GetNonRuinedItem
        {
            private static void Postfix(string gearName, ref GearItem? __result)
            {
                if (!BPMain.backpackPlaced)
                {
                    return;
                }

                bool isFound = false;
                BPUtils.DebugMsg("Inventory_GetNonRuinedItem_Postfix | checking backpack for " + gearName);
                for (int i = 0; i < BPMain.backpackContainer.m_Items.Count; i++)
                {
                    GearItem newGearItem = BPMain.backpackContainer.m_Items[i];
                    if (newGearItem.name != gearName)
                    {
                        continue;
                    }
                    if ((bool)newGearItem && (bool)newGearItem.m_BreakDownItem && !Utils.IsZero(newGearItem.CurrentHP))
                    {
                        isFound = true;
                        if (!BPUtils.IsBackpackInRange())
                        {
                            BPUtils.DebugMsg("Inventory_GetNonRuinedItem_Postfix | backpack had " + gearName + " | " + isFound + " | NOT IN RANGE");
                            HUDMessage.AddMessage("Backpack out of range", 1, true, true);
                            return;
                        }
                        __result = newGearItem;
                        BPUtils.DebugMsg("Inventory_GetNonRuinedItem_Postfix | backpack had " + gearName + " | " + isFound);
                        return;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Panel_BreakDown), nameof(Panel_BreakDown.RefreshTools))]
        internal class Panel_BreakDown_RefreshTools
        {
            private static void Postfix(Panel_BreakDown __instance)
            {
                if (!BPMain.backpackPlaced || !BPUtils.IsBackpackInRange())
                {
                    return;
                }
                for (int i = 0; i < BPMain.backpackContainer.m_Items.Count; i++)
                {
                    GearItem gearItem = BPMain.backpackContainer.m_Items[i];
                    if ((bool)gearItem && (bool)gearItem.m_BreakDownItem && !Utils.IsZero(gearItem.CurrentHP))
                    {
                        BPUtils.DebugMsg("Panel_BreakDown_RefreshTools | added " + gearItem.name + " to Panel_Breakdown.m_Tools");
                        __instance.m_Tools.Add(gearItem);
                    }
                }
            }
        }
    }
}
