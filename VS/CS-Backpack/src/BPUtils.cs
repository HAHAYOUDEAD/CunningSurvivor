namespace CunningSurvivor
{
    internal static class BPUtils
    {
        public static float? GetBackpackDistance()
        {
            if (BPMain.BackpackPlaced == false)
            {
                return null;
            }
            float distance = Utils.DistanceToMainCamera(BPMain.Backpack.position);
            BPMain.DebugMsg("Backpack distance " + distance + " required " + Settings.options.backPackMinDistance);
            return distance;
        }

        public static bool IsBackpackInRange()
        {
            if (BPMain.BackpackPlaced == true)
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