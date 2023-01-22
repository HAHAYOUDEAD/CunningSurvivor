using ModSettings;
using System.Reflection;
using Il2Cpp;
using Il2CppNewtonsoft.Json.Linq;


namespace CunningSurvivor
{
    internal static class Settings
    {
        public static BPSettings options;

        public static void OnLoad()
        {
            options = new BPSettings();
            options.AddToModSettings("[Cunning Survivor]");
        }


    }

    internal class BPSettings : JsonModSettings
    {
        [Section("Backpack")]

        [Name("Variant")]
        [Description("Choose how your backpack looks")]
        [Choice("Based on current character", "Will", "Astrid")]
        public int backPackVariant = 0;

        [Name("Check Inventory Items")]
        [Description("Don't check for player has items in inventory")]
        public bool backPackCheckInv = true;

        [Name("Debug Output")]
        [Description("Output debug messages")]
        public bool backPackDebug = false;
    }
}
