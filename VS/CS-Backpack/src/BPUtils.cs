namespace CunningSurvivor
{
    internal static class BPUtils
    {
        public static float? GetBackpackDistance()
        {
            if (BPMain.backpackPlaced == false)
            {
                return null;
            }
            float distance = Utils.DistanceToMainCamera(BPMain.backpack.position);
            BPMain.DebugMsg("Backpack distance " + distance + " required " + Settings.options.backPackMinDistance);
            return distance;
        }

        public static bool IsBackpackInRange()
        {
            if (BPMain.backpackPlaced == true)
            {
                float? distance = GetBackpackDistance();
                if (distance != null && distance <= Settings.options.backPackMinDistance)
                {
                    BPMain.DebugMsg("Backpack IN range");                    
                    return true;
                }
            }
            BPMain.DebugMsg("Backpack NOT in range");
            return false;
        }



    }

}