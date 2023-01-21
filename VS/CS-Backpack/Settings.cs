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
        [Choice(new string[]
        {
            "Based on current character",
            "Will",
            "Astrid"
        })]
        public int backPackVariant;



        protected override void OnConfirm()
        {
            switch (Settings.options.backPackVariant)
            { 
                case 0:
                    {
                        break;
                    }
                case 1:
                    {
                        BPMain.ChangeTex("will");
                        break;
                    }
                case 2:
                    {
                        BPMain.ChangeTex("astrid");
                        break;
                    }   
            }

            base.OnConfirm();
        }
    }
}
