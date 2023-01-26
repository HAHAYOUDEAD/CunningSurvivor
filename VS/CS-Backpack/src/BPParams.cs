namespace CunningSurvivor
{
    internal class BPParams
    {
        public static Dictionary<String, String> attachableGearItems = new()
        {
            { "GEAR_BearSkinBedRoll", "attachPoint_bedroll" },
            { "GEAR_BedRoll", "attachPoint_bedroll" },
            { "GEAR_Bow", "attachPoint_bow" },
            { "GEAR_RecycledCan", "attachPoint_can" },
            { "GEAR_BlueFlare", "attachPoint_flareBlue" },
            { "GEAR_FlareA", "attachPoint_flareRed" },
            { "GEAR_Flashlight", "attachPoint_flashlight" },
            { "GEAR_Hatchet", "attachPoint_hatchet" },
            { "GEAR_HatchetImprovised", "attachPoint_hatchet" },
            { "GEAR_KeroseneLampB", "attachPoint_lantern"},
            { "GEAR_CookingPot", "attachPoint_pot" },
            { "GEAR_Prybar", "attachPoint_prybar" },
            { "GEAR_Rope", "attachPoint_rope" },
            { "GEAR_SprayPaintCan", "attachPoint_sprayCan" },
            { "GEAR_Torch", "attachPoint_torch" },
            { "GEAR_Arrow|3", "attachPoint_arrow" }
        };
        // item to keep, quantity
        public static Dictionary<String, int> itemsPlayerKeeps = new()
        {
            { "GEAR_WoodMatches", 6 },
            { "GEAR_PackMatches", 6 },
            { "GEAR_Knife", 1 },
            { "GEAR_KnifeImprovised", 1 },
            { "GEAR_Rifle", 1 },
            { "GEAR_HeavyBandage", 1 },
            { "GEAR_BottlePainKillers", 6 }

        };
        // lower priority item, higher priority item
        public static Dictionary<String, String> itemsPlayerKeepsPriority = new()
        {
            { "GEAR_PackMatches", "GEAR_WoodMatches" },
            { "GEAR_KnifeImprovised", "GEAR_Knife" }
        };

        public static float carryCapacityBase = 2f;

        public static float backpackAddCarryCapacity = 35f;
    }
}
