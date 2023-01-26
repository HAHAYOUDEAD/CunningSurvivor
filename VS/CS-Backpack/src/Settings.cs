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

        [Name("Minimum backpack distance")]
        [Description("Adjust the minimum distance required for backpack interactions")]
        [Slider(1f, 50f)]
        public float backPackMinDistance = 3f;

        [Section("DEBUG")]

        [Name("Check Inventory Items")]
        [Description("Don't check for player has items in inventory")]
        public bool backPackCheckInv = true;

        [Name("Output")]
        [Description("Output debug messages")]
        public bool backPackDebug = false;
    }
}
