using Harmony;
using Il2Cpp;
using Il2CppTLD.Gear;
using System.Linq;

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
            BPMain.BackpackContainer.AddGear(clone);

        }

        public static void MoveFromBackpackToPlayer(GearItem GearItemObj)
        {
            string cloneText = GearItemObj.Serialize();
            GearItem clone = GearItem.InstantiateGearItem(GearItemObj.name);
            clone.Deserialize(cloneText);
            BPMain.BackpackContainer.RemoveGear(GearItemObj, true);
            int quantity = 1;
            if (clone.m_StackableItem)
            {
                quantity = clone.m_StackableItem.m_Units;
            }
            if (!GameManager.GetPlayerManagerComponent().TryAddToExistingStackable(clone, quantity, out GearItem existingStack))
            {
                GameManager.GetInventoryComponent().AddGear(clone, false);
            }
            
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
                if (BPMain.ItemsPlayerKeeps.ContainsKey(moveItem.name) && quantity >= BPMain.ItemsPlayerKeeps[moveItem.name])
                {
                    quantity -= BPMain.ItemsPlayerKeeps[moveItem.name];
                }
                if (
                    movedItems.Contains(moveItem.name) ||
                    (BPMain.ItemsPlayerKeepsPriority.ContainsKey(moveItem.name) && playerHeld.Contains(BPMain.ItemsPlayerKeepsPriority[moveItem.name]))
                    )
                {
                    if (moveItem.m_StackableItem)
                    {
                        quantity = moveItem.m_StackableItem.m_Units;
                    } else
                    {
                        quantity = 1;
                    }
                }
                movedItems.Add(moveItem.name);
                BPMain.DebugMsg("Item moved to backpack | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
                MoveFromPlayerToBackpack(moveItem, quantity);
            }
        }

        public static void UnpopulateBackpack()
        {
            List<GearItem> itemsToMove = new();
            foreach (GearItemObject GearItemObj in BPMain.BackpackContainer.m_Items)
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
                BPMain.DebugMsg("Item moved to player | " + moveItem.name + " (" + moveItem.GetNormalizedCondition() * 100 + "%) | " + quantity);
                MoveFromBackpackToPlayer(moveItem);
            }
        }

    }

}