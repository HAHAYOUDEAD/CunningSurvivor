namespace CunningSurvivor
{
    internal static class BPCarryCapacity
    {
        public static void Update()
        {
            float baseValue = BPParams.carryCapacityBase;

            if (BPMain.backpackPlaced == false)
            {
                baseValue += BPParams.backpackBaseCapacity;
            }

            BPUtils.DebugMsg("base carry weight | " + baseValue);

            // add equipped clothing weight
            baseValue += GetEquippedClothingWeight();

            // add requipped clothing weight bonus
            baseValue += GetEquippedClothingWeightBonus();

            BPUtils.DebugMsg("final carry weight | " + baseValue);

            GameManager.GetEncumberComponent().m_MaxCarryCapacityKG = baseValue;
            GameManager.GetEncumberComponent().m_MaxCarryCapacityWhenExhaustedKG = baseValue * 0.8f;
            GameManager.GetEncumberComponent().m_NoSprintCarryCapacityKG = baseValue * 0.8f;
            GameManager.GetEncumberComponent().m_NoWalkCarryCapacityKG = baseValue;
            GameManager.GetEncumberComponent().m_EncumberLowThresholdKG = baseValue * 0.8f;
            GameManager.GetEncumberComponent().m_EncumberMedThresholdKG = baseValue * 0.8f;
            GameManager.GetEncumberComponent().m_EncumberHighThresholdKG = baseValue * 0.8f;
        }


        public static float GetEquippedClothingWeight()
        {
            float clothingWeight = 0;

            foreach (GearItemObject gearItemObj in GameManager.GetInventoryComponent().m_Items)
            {
                if (gearItemObj.m_GearItem.m_ClothingItem)
                {
                    if (gearItemObj.m_GearItem.m_ClothingItem.IsWearing())
                    {
                        // add the weight after requipped bonus applied
                        clothingWeight += gearItemObj.m_GearItem.GetSingleItemWeightKG(false);
                    }
                }
            }
            BPUtils.DebugMsg("Clothing weight | " + clothingWeight);
            return clothingWeight;
        }

        public static float GetEquippedClothingWeightBonus()
        {
            float clothingWeightBonus = 0;
            foreach (GearItemObject gearItemObj in GameManager.GetInventoryComponent().m_Items)
            {
                if (gearItemObj.m_GearItem.m_ClothingItem)
                {
                    if (gearItemObj.m_GearItem.m_ClothingItem.IsWearing())
                    {
                        // only give the bonus if is Outer Jacket
                        if (
                        gearItemObj.m_GearItem.m_ClothingItem.m_Region == ClothingRegion.Chest && gearItemObj.m_GearItem.m_ClothingItem.m_EquippedLayer == ClothingLayer.Top2
                        )
                        {
                            clothingWeightBonus += 0.25f;
                        }
                        // only give the bonus if is Outer Pants
                        if (
                        gearItemObj.m_GearItem.m_ClothingItem.m_Region == ClothingRegion.Legs && gearItemObj.m_GearItem.m_ClothingItem.m_EquippedLayer == ClothingLayer.Top2
                        )
                        {
                            clothingWeightBonus += 0.25f;
                        }
                    }
                }
            }
            BPUtils.DebugMsg("Clothing weight bonus | " + clothingWeightBonus);
            return clothingWeightBonus;
        }
    }
}