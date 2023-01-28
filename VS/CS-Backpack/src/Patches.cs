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

    }
}
