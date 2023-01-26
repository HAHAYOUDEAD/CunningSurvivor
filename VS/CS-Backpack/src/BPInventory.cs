using Il2Cpp;

namespace CunningSurvivor
{
    internal static class BPInventory
    {

        public static GearItem? PlayerHasGearItem(String gearItemName, int minItemCount = 1)
        {
            GearItem gearItemObj = GameManager.GetInventoryComponent().GearInInventory(gearItemName, minItemCount);
            if (gearItemObj && gearItemObj.gameObject && !gearItemObj.IsWornOut())
            {
                if (
                    !gearItemObj.IsLitFlare() &&
                    !gearItemObj.IsLitFlashlight() &&
                    !gearItemObj.IsLitLamp() &&
                    !gearItemObj.IsLitLightsource() &&
                    !gearItemObj.IsLitMatch() &&
                    !gearItemObj.IsLitNoiseMaker() &&
                    !gearItemObj.IsLitTorch()
                    )
                {
                    BPUtils.DebugMsg("Player has item " + gearItemObj.name);
                    return gearItemObj;
                }
                else
                {
                    BPUtils.DebugMsg("Player has item BUT is lit" + gearItemObj.name);
                }
            }
            BPUtils.DebugMsg("Player does not have item " + gearItemName);

            return null;
        }

        public static void MoveFromPlayerToBackpack(GearItem gearItemObj, int quantity)
        {
            int unitsToKeep = 0;
            if (gearItemObj.m_StackableItem)
            {
                unitsToKeep = gearItemObj.m_StackableItem.m_Units - quantity;
            }
            string cloneText = gearItemObj.Serialize();
            GearItem clone = GearItem.InstantiateGearItem(gearItemObj.name);
            clone.Deserialize(cloneText);
            if (clone.m_StackableItem && clone.m_StackableItem.m_Units >= quantity)
            {
                clone.m_StackableItem.m_Units = quantity;
            }
            if (gearItemObj.m_StackableItem && unitsToKeep >= 1)
            {
                gearItemObj.m_StackableItem.m_Units = unitsToKeep;
            }
            else
            {

                GameManager.GetInventoryComponent().RemoveGear(gearItemObj.gameObject, true);
            }
            BPMain.backpackContainer.AddGear(clone);

        }

        public static void MoveFromBackpackToPlayer(GearItem gearItemObj)
        {

            BPMain.backpackContainer.RemoveGear(gearItemObj, true);
            GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(gearItemObj, true, false);
        }

        public static void PopulateBackpack()
        {
            List<GearItem> itemsToMove = new();
            List<String> playerHeld = new();
            List<String> movedItems = new();
            foreach (GearItemObject gearItemObj in GameManager.GetInventoryComponent().m_Items)
            {
                playerHeld.Add(gearItemObj.m_GearItem.name);
                if (gearItemObj.m_GearItem.m_ClothingItem)
                {
                    if (!gearItemObj.m_GearItem.m_ClothingItem.IsWearing())
                    {
                        itemsToMove.Add(gearItemObj.m_GearItem);
                    }
                }
                else
                {
                    itemsToMove.Add(gearItemObj.m_GearItem);
                }
            }
            foreach (GearItem moveItem in itemsToMove)
            {
                int quantity = 1;
                if (moveItem.m_StackableItem)
                {
                    quantity = moveItem.m_StackableItem.m_Units;
                }
                if (BPParams.itemsPlayerKeeps.ContainsKey(moveItem.name) && quantity >= BPParams.itemsPlayerKeeps[moveItem.name])
                {
                    quantity -= BPParams.itemsPlayerKeeps[moveItem.name];
                }
                if (
                    movedItems.Contains(moveItem.name) ||
                    (BPParams.itemsPlayerKeepsPriority.ContainsKey(moveItem.name) && playerHeld.Contains(BPParams.itemsPlayerKeepsPriority[moveItem.name]))
                    )
                {
                    if (moveItem.m_StackableItem)
                    {
                        quantity = moveItem.m_StackableItem.m_Units;
                    }
                    else
                    {
                        quantity = 1;
                    }
                }
                if (quantity > 0)
                {
                    movedItems.Add(moveItem.name);
                    BPUtils.DebugMsg("Item moved to backpack | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
                    MoveFromPlayerToBackpack(moveItem, quantity);
                }
            }
        }

        public static void UnpopulateBackpack()
        {
            List<GearItem> itemsToMove = new();
            foreach (GearItemObject gearItemObj in BPMain.backpackContainer.m_Items)
            {
                itemsToMove.Add(gearItemObj.m_GearItem);
            }

            foreach (GearItem moveItem in itemsToMove)
            {
                int quantity = 1;
                if (moveItem.m_StackableItem)
                {
                    quantity = moveItem.m_StackableItem.m_Units;
                }
                if (quantity > 0)
                {
                    BPUtils.DebugMsg("Item moved to player | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
                    MoveFromBackpackToPlayer(moveItem);
                }
            }
        }

        public static void InitBackpackContainer()
        {
            Container cloneFrom = ContainerManager.m_Containers[0];
            Il2CppSystem.Collections.Generic.List<GearItem> emptyList = new();
            BPMain.backpack.gameObject.AddComponent<Container>();
            BPMain.backpackContainer = BPMain.backpack.GetComponent<Container>();
            BPMain.backpackContainer.Deserialize(cloneFrom.Serialize(), emptyList);
        }


    }

}