namespace CunningSurvivor
{
    internal static class BPInventory
    {

        public static GearItem? PlayerHasGearItem(String GearItemName, int MinItemCount = 1)
        {
            GearItem GearItemObject = GameManager.GetInventoryComponent().GearInInventory(GearItemName, MinItemCount);
            if (GearItemObject && GearItemObject.gameObject && !GearItemObject.IsWornOut())
            {
                if (
                    !GearItemObject.IsLitFlare() &&
                    !GearItemObject.IsLitFlashlight() &&
                    !GearItemObject.IsLitLamp() &&
                    !GearItemObject.IsLitLightsource() &&
                    !GearItemObject.IsLitMatch() &&
                    !GearItemObject.IsLitNoiseMaker() &&
                    !GearItemObject.IsLitTorch()
                    )
                {
                    if (Settings.options.backPackDebug)
                    {
                        MelonLogger.Msg("Player has item " + GearItemObject.name);
                    }
                    return GearItemObject;
                }
                else
                {
                    if (Settings.options.backPackDebug)
                    {
                        MelonLogger.Msg("Player has item BUT is lit" + GearItemObject.name);
                    }
                }
            }
            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Player does not have item " + GearItemName);
            }
            return null;
        }

        public static void PopulateBackpack()
        {



        }

        public static void UnpopulateBackpack()
        {

            foreach (GearItem GearItemObject in GameManager.GetInventoryComponent().m_Items)
            {
                // show/add item to inventory here
            }
        }

    }

}