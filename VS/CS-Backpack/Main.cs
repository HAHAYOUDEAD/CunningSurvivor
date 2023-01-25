namespace CunningSurvivor
{
    internal class BPMain : MelonMod
    {
        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static AssetBundle backpackBundle;

        public static GameObject BackpackInst;
        public static Transform Backpack;
        public static bool BackpackPlaced { get; set; } = false;
        private static bool BackpackPlacing { get; set; } = false;

        public static Container BackpackContainer;
        public static ContainerInteraction BackpackContainerInteraction;


        public static readonly string modFolderName = "cunningSurvivor/backpack/";
        public static readonly string bundleName = "bundlebackpack";
        public static readonly string storedDataFolderName = "Assets/LooseFiles/";

        public static Dictionary<String, String> AttachableGearItems = new Dictionary<String, String>() {
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

        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.LoadSceneData), new Type[] { typeof(string), typeof(string) })]
        private static class SaveGameSystem_LoadSceneData
        {
            private static void Postfix(string name, string sceneSaveName)
            {
                MelonLogger.Msg("SaveGameSystem_LoadSceneData (" + name + "|" + sceneSaveName + ")");

            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveSceneData), new Type[] { typeof(SlotData), typeof(string) })]
        private static class SaveGameSystem_SaveSceneData
        {
            private static void Postfix(SlotData slot, string sceneSaveName)
            {
                PickupBackpack(true);
                MelonLogger.Msg("SaveGameSystem_SaveSceneData (" + slot.m_DisplayName + "|" + sceneSaveName + ")");

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.CancelPlaceMesh))]
        private static class PlayerManager_CancelPlaceMesh
        {
            private static void Postfix()
            {
                if (BackpackPlacing == true)
                {
                    MelonLogger.Msg("PlayerManager_CancelPlaceMesh");
                    PickupBackpack(true);
                    BackpackPlacing = false;
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
                    //MelonLogger.Msg("PlayerManager_AttemptToPlaceMesh - !m_ObjectToPlace");
                    return;
                }
                if (!__instance.CanPlaceCurrentPlaceable())
                {
                    //MelonLogger.Msg("PlayerManager_AttemptToPlaceMesh - !CanPlaceCurrentPlaceable");
                    return;
                }

                if (BackpackPlacing == true)
                {
                    PlaceBackpackComplete();
                }

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

                DebugMsg("backpack state " + BackpackPlaced);
                

                if (BackpackPlaced == false)
                {
                    PlaceBackpackStart();
                    return;
                }

                if (BackpackPlaced == true)
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

                DebugMsg("backpack state " + BackpackPlaced);

                if (BackpackPlaced == false)
                {
                    DebugMsg("No Backpack Placed");
                    return;
                }
                if (BackpackPlaced == true)
                {
                    BPUtils.IsBackpackInRange();
                    return;
                }
            }
        }

        public static void PlaceBackpackStart()
        {
            if (BackpackPlacing == true)
            {
                return;
            }
            BackpackPlacing = true;

            Transform player = GameManager.GetPlayerTransform();
            BackpackInst = GameObject.Instantiate(backpackBundle.LoadAsset<GameObject>("backpackWithAttachPoints"));
            Backpack = BackpackInst.transform.GetChild(0);
            BackpackInst.name = "[CunningSurvivor]Backpack_Parent";
            Backpack.name = "[CunningSurvivor]Backpack";

            BackpackContainer = BackpackInst.gameObject.AddComponent<Container>();
            BackpackContainer.m_Items.Clear();
            BackpackContainer.m_CapacityKG = 30;
            BackpackContainer.m_LocalizedDisplayName = null;
            BackpackContainer.name = "Backpack";
            BackpackContainer.m_Inspected = true;
            BackpackContainerInteraction = BackpackInst.gameObject.AddComponent<ContainerInteraction>();
            BackpackContainerInteraction.Start();
            BackpackContainerInteraction.HoldText = "Open Backpack";
            BackpackContainerInteraction._HoldText_k__BackingField = "Open Backpack";

            BackpackInst.gameObject.AddComponent<ObjectGuid>();
            BackpackInst.gameObject.GetComponent<ObjectGuid>().m_Guid = Guid.NewGuid().ToString();

            Backpack.gameObject.GetComponent<MeshRenderer>().materials[0].shader = vanillaSkinnedShader;
            Backpack.gameObject.GetComponent<MeshRenderer>().materials[1].shader = vanillaSkinnedShader;
            Backpack.gameObject.GetComponent<MeshRenderer>().materials[2].shader = vanillaSkinnedShader;

            // Will variant
            if (Settings.options.backPackVariant == 1)
            {
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainW.png");
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backW.png");
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailW.png");
            }
            // Astrid variant
            if (Settings.options.backPackVariant == 2)
            {
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainA.png");
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backA.png");
                Backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailA.png");
            }

            // attach the gear to the backpack before placing mesh
            // for visual appearance
            // populate on PlaceBackpackComplete()
            BPAttachPoints.AttachBackpackGear();


            GameManager.GetPlayerManagerComponent().StartPlaceMesh(BackpackInst.gameObject, PlaceMeshFlags.None);
            GameManager.GetPlayerManagerComponent().m_RotationAngle = BackpackInst.gameObject.transform.localEulerAngles.y;

            DebugMsg("Placing backpack");


        }

        public static void PlaceBackpackComplete()
        {
            DebugMsg("Backpack Placed 1 | GUID | " + BackpackInst.gameObject.GetComponent<ObjectGuid>().m_Guid);
            BackpackPlaced = true;
            BackpackPlacing = false;
            HUDMessage.AddMessage("Backpack Placed", 1, true, true);

            // TODO
            BPInventory.PopulateBackpack();


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
            if (BackpackPlaced == true || BackpackPlacing == true)
            {
                // TODO
                BPInventory.UnpopulateBackpack();

                if (force == false)
                {
                    HUDMessage.AddMessage("Backpack Picked Up", 1f, true, true);
                }
                GameManager.Destroy(Backpack.gameObject);
                GameManager.Destroy(BackpackInst.gameObject);
                BPAttachPoints.Clear();
                DebugMsg("Backpack Picked Up | forced " + force);
                BackpackPlaced = false;
            }
        }








    }
}
