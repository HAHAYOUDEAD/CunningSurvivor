using Il2Cpp;

namespace CunningSurvivor
{
    internal static class BPInventory
    {

        public static GearItem? PlayerHasGearItem(String GearItemName, int MinItemCount = 1)
        {
            GearItem GearItemObj = GameManager.GetInventoryComponent().GearInInventory(GearItemName, MinItemCount);
            if (GearItemObj && GearItemObj.gameObject && !GearItemObj.IsWornOut())
            {
                if (
                    !GearItemObj.IsLitFlare() &&
                    !GearItemObj.IsLitFlashlight() &&
                    !GearItemObj.IsLitLamp() &&
                    !GearItemObj.IsLitLightsource() &&
                    !GearItemObj.IsLitMatch() &&
                    !GearItemObj.IsLitNoiseMaker() &&
                    !GearItemObj.IsLitTorch()
                    )
                {
                    BPMain.DebugMsg("Player has item " + GearItemObj.name);
                    return GearItemObj;
                }
                else
                {
                    BPMain.DebugMsg("Player has item BUT is lit" + GearItemObj.name);
                }
            }
            BPMain.DebugMsg("Player does not have item " + GearItemName);

            return null;
        }

        public static void MoveFromPlayerToBackpack(GearItem GearItemObj, int quantity)
        {
            int unitsToKeep = 0;
            if (GearItemObj.m_StackableItem)
            {
                unitsToKeep = GearItemObj.m_StackableItem.m_Units - quantity;
            }
            string cloneText = GearItemObj.Serialize();
            GearItem clone = GearItem.InstantiateGearItem(GearItemObj.name);
            clone.Deserialize(cloneText);
            if (clone.m_StackableItem && clone.m_StackableItem.m_Units >= quantity)
            {
                clone.m_StackableItem.m_Units = quantity;
            }
            if (GearItemObj.m_StackableItem && unitsToKeep >= 1)
            {
                GearItemObj.m_StackableItem.m_Units = unitsToKeep;
            }
            else
            {

                GameManager.GetInventoryComponent().RemoveGear(GearItemObj.gameObject, true);
            }
            BPMain.backpackContainer.AddGear(clone);

        }

        public static void MoveFromBackpackToPlayer(GearItem GearItemObj)
        {

            BPMain.backpackContainer.RemoveGear(GearItemObj, true);
            GameManager.GetPlayerManagerComponent().AddItemToPlayerInventory(GearItemObj, true, false);
        }

        public static void PopulateBackpack()
        {
            List<GearItem> itemsToMove = new();
            List<String> playerHeld = new();
            List<String> movedItems = new();
            foreach (GearItemObject GearItemObj in GameManager.GetInventoryComponent().m_Items)
            {
                playerHeld.Add(GearItemObj.m_GearItem.name);
                if (GearItemObj.m_GearItem.m_ClothingItem)
                {
                    if (!GearItemObj.m_GearItem.m_ClothingItem.IsWearing())
                    {
                        itemsToMove.Add(GearItemObj.m_GearItem);
                    }
                }
                else
                {
                    itemsToMove.Add(GearItemObj.m_GearItem);
                }
            }
            foreach (GearItem moveItem in itemsToMove)
            {
                int quantity = 1;
                if (moveItem.m_StackableItem)
                {
                    quantity = moveItem.m_StackableItem.m_Units;
                }
                if (BPMain.itemsPlayerKeeps.ContainsKey(moveItem.name) && quantity >= BPMain.itemsPlayerKeeps[moveItem.name])
                {
                    quantity -= BPMain.itemsPlayerKeeps[moveItem.name];
                }
                if (
                    movedItems.Contains(moveItem.name) ||
                    (BPMain.itemsPlayerKeepsPriority.ContainsKey(moveItem.name) && playerHeld.Contains(BPMain.itemsPlayerKeepsPriority[moveItem.name]))
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
                    BPMain.DebugMsg("Item moved to backpack | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
                    MoveFromPlayerToBackpack(moveItem, quantity);
                }
            }
        }

        public static void UnpopulateBackpack()
        {
            List<GearItem> itemsToMove = new();
            foreach (GearItemObject GearItemObj in BPMain.backpackContainer.m_Items)
            {
                itemsToMove.Add(GearItemObj.m_GearItem);
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
                    BPMain.DebugMsg("Item moved to player | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
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