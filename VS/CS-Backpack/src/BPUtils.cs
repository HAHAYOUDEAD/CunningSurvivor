namespace CunningSurvivor
{
    internal static class BPUtils
    {
        public static void DebugMsg(string msg = "")
        {
            if (Settings.options.backPackDebug == true && msg != "")
            {
                MelonLogger.Msg(msg);
            }
        }
        public static float? GetBackpackDistance()
        {
            if (BPMain.backpackPlaced == false)
            {
                return null;
            }
            float distance = Utils.DistanceToMainCamera(BPMain.backpack.position);
            DebugMsg("Backpack distance " + distance + " required " + Settings.options.backPackMinDistance);
            return distance;
        }

        public static bool IsBackpackInRange()
        {
            if (BPMain.backpackPlaced == true)
            {
                float? distance = GetBackpackDistance();
                if (distance != null && distance <= Settings.options.backPackMinDistance)
                {
                    DebugMsg("Backpack IN range");                    
                    return true;
                }
            }
            DebugMsg("Backpack NOT in range");
            return false;
        }



    }

}