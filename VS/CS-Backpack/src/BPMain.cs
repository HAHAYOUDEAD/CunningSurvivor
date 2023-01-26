namespace CunningSurvivor
{
    internal class BPMain : MelonMod
    {
        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static AssetBundle backpackBundle;

        public static GameObject backpackInst;
        public static Transform backpack;
        public static bool backpackPlaced { get; set; } = false;
        private static bool backpackPlacing { get; set; } = false;

        public static Container backpackContainer;
        public static ContainerInteraction backpackInteraction;

        public static float carryCapacityBase = 5f;
        public static float BackpackAddCarryCapacity = 35f;

        public static readonly string modFolderName = "cunningSurvivor/backpack/";
        public static readonly string bundleName = "bundlebackpack";
        public static readonly string storedDataFolderName = "Assets/LooseFiles/";

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

        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData), new Type[] { typeof(string), typeof(string) })]
        private static class SaveGameSystem_LoadSceneData
        {
            private static void Postfix(string name, string sceneSaveName)
            {
                DebugMsg("SaveGameSystem_LoadSceneData (" + name + "|" + sceneSaveName + ")");
                if (backpackPlaced == true)
                {
                    GameManager.GetEncumberComponent().m_MaxCarryCapacityKG = carryCapacityBase;
                }
                else
                {
                    GameManager.GetEncumberComponent().m_MaxCarryCapacityKG = carryCapacityBase + BackpackAddCarryCapacity;
                }

            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGame), new Type[] { typeof(string), typeof(string) })]
        private static class SaveGameSystem_SaveGame
        {
            private static void Prefix(string name, string sceneSaveName)
            {
                PickupBackpack(true);
                DebugMsg("SaveGameSystem_SaveGame (" + name + "|" + sceneSaveName + ")");

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.CancelPlaceMesh))]
        private static class PlayerManager_CancelPlaceMesh
        {
            private static void Postfix()
            {
                if (backpackPlacing == true)
                {
                    DebugMsg("PlayerManager_CancelPlaceMesh");
                    PickupBackpack(true);
                    backpackPlacing = false;
                }

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.AttemptToPlaceMesh))]
        private static class PlayerManager_AttemptToPlaceMesh
        {
            private static void Postfix(PlayerManager __instance)
            {
                if (!__instance.m_ObjectToPlace)
                {
                    return;
                }
                if (!__instance.CanPlaceCurrentPlaceable())
                {
                    return;
                }

                if (backpackPlacing == true)
                {
                    PlaceBackpackComplete();
                }

            }
        }

        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.InteractiveObjectsProcessAltFire))]
        internal class PlayerManager_InteractiveObjectsProcessAltFire
        {
            private static bool Prefix()
            {
                GameObject ItemUnderCrosshair = Utility.GetObjectUnderCrosshair();
                MelonLogger.Msg(ItemUnderCrosshair);
                MelonLogger.Msg(ItemUnderCrosshair.name);
                if (ItemUnderCrosshair && ItemUnderCrosshair == backpack)
                {
                    DebugMsg("Item Under Crosshair = " + ItemUnderCrosshair.name);

                    GameManager.GetPlayerManagerComponent().StartPlaceMesh(ItemUnderCrosshair.transform.GetParent().gameObject, PlaceMeshFlags.None);
                    return false;
                }
                return true;
            }

        }


        public override void OnInitializeMelon()
        {
            // Load assets
            backpackBundle = AssetBundle.LoadFromFile("Mods/" + modFolderName + bundleName);

            // Get shaders
            vanillaSkinnedShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
            vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

            // Enable settings
            Settings.OnLoad();


        }

        public static void DebugMsg(string msg = "")
        {
            if (Settings.options.backPackDebug == true && msg != "")
            {
                MelonLogger.Msg(msg);
            }
        }


        public override void OnUpdate()
        {
            // temp proximity check keybind
            // replace with inventory interaction/button
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.B))
            {

                if (InterfaceManager.GetInstance().AnyOverlayPanelEnabled())
                {
                    DebugMsg("! Overlay/Panel open");
                    return;
                }

                DebugMsg("backpack state " + backpackPlaced);


                if (backpackPlaced == false)
                {
                    PlaceBackpackStart();
                    return;
                }

                if (backpackPlaced == true)
                {
                    PickupBackpack();
                    return;
                }
            }
            // temp proximity check keybind
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.V))
            {
                if (InterfaceManager.GetInstance().AnyOverlayPanelEnabled())
                {
                    DebugMsg("! Overlay/Panel Open");
                    return;
                }

                DebugMsg("backpack state " + backpackPlaced);

                if (backpackPlaced == false)
                {
                    DebugMsg("No Backpack Placed");
                    return;
                }
                if (backpackPlaced == true)
                {
                    BPUtils.IsBackpackInRange();
                    return;
                }
            }
        }

        public static void PlaceBackpackStart()
        {
            if (backpackPlacing == true)
            {
                return;
            }
            backpackPlacing = true;

            //Transform player = GameManager.GetPlayerTransform();
            backpackInst = GameObject.Instantiate(backpackBundle.LoadAsset<GameObject>("backpackWithAttachPoints"));
            backpackInst.layer = vp_Layer.Container;
            backpack = backpackInst.transform.GetChild(0);
            backpack.gameObject.layer = vp_Layer.Container;
            backpackInst.name = "[CunningSurvivor]Backpack_Parent";
            backpack.name = "[CunningSurvivor]Backpack";

            //MeshCollider BPCollider = backpackInst.gameObject.AddComponent<MeshCollider>();
            //Backpack.gameObject.AddComponent<BoxCollider>();

            
            backpackInst.gameObject.AddComponent<Container>();
            backpackContainer = backpackInst.GetComponent<Container>();
            backpackContainer.Start();
            backpackContainer.m_Inspected = true;
            backpackContainer.m_NotPopulated = false;
            backpackContainer.m_GearToInstantiate.Clear();
            backpackContainer.m_Items.Clear();
            backpackContainer.m_CapacityKG = BackpackAddCarryCapacity;
            backpackContainer.m_LocalizedDisplayName = null;

            backpackInst.gameObject.AddComponent<ContainerInteraction>();
            backpackInteraction = backpackInst.GetComponent<ContainerInteraction>();
            backpackInteraction.enabled = true;
            backpackInteraction.gameObject.SetActive(true);
            backpackInteraction.CanInteract = true;
            backpackInteraction.HoverText = "RMB - Move Backpack";
            backpackInteraction.m_DefaultHoverText = null;
            

            //BackpackInteraction.Start();
            //BackpackInteraction.HoldText = "Open Backpack";
            //BackpackInteraction._HoldText_k__BackingField = "Open Backpack";

            backpackInst.gameObject.AddComponent<ObjectGuid>();
            backpackInst.gameObject.GetComponent<ObjectGuid>().m_Guid = Guid.NewGuid().ToString();

            ContainerManager.m_Containers.Add(backpackContainer);

            backpack.gameObject.GetComponent<MeshRenderer>().materials[0].shader = vanillaSkinnedShader;
            backpack.gameObject.GetComponent<MeshRenderer>().materials[1].shader = vanillaSkinnedShader;
            backpack.gameObject.GetComponent<MeshRenderer>().materials[2].shader = vanillaSkinnedShader;

            // Will variant
            if (Settings.options.backPackVariant == 1)
            {
                backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainW.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backW.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailW.png");
            }
            // Astrid variant
            if (Settings.options.backPackVariant == 2)
            {
                backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainA.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backA.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailA.png");
            }

            // attach the gear to the backpack before placing mesh
            // for visual appearance
            BPAttachPoints.AttachBackpackGear();
            // populate the backpack
            BPInventory.PopulateBackpack();
            GameManager.GetEncumberComponent().m_MaxCarryCapacityKG = carryCapacityBase;


            GameManager.GetPlayerManagerComponent().StartPlaceMesh(backpackInst.gameObject, PlaceMeshFlags.None);
            GameManager.GetPlayerManagerComponent().m_RotationAngle = backpackInst.gameObject.transform.localEulerAngles.y;

            DebugMsg("Placing backpack");


        }

        public static void PlaceBackpackComplete()
        {
            DebugMsg("Backpack Placed 1 | GUID | " + backpackInst.gameObject.GetComponent<ObjectGuid>().m_Guid);
            backpackPlaced = true;
            backpackPlacing = false;
            HUDMessage.AddMessage("Backpack Placed", 1, true, true);


        }

        public static void PickupBackpack(bool force = false)
        {
            if (force == false)
            {
                if (!BPUtils.IsBackpackInRange())
                {
                    HUDMessage.AddMessage("Backpack too far away", 1f, true, true);
                    return;
                }
            }
            if (backpackPlaced == true || backpackPlacing == true)
            {
                // TODO
                BPInventory.UnpopulateBackpack();

                if (force == false)
                {
                    HUDMessage.AddMessage("Backpack Picked Up", 1f, true, true);
                }
                GameManager.Destroy(backpack.gameObject);
                GameManager.Destroy(backpackInst.gameObject);
                BPAttachPoints.Clear();
                DebugMsg("Backpack Picked Up | forced " + force);
                backpackPlaced = false;
                GameManager.GetEncumberComponent().m_MaxCarryCapacityKG = carryCapacityBase + BackpackAddCarryCapacity;
            }
        }








    }
}
