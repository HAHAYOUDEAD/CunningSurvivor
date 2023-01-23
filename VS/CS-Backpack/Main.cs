using Il2Cpp;
using Il2CppTLD.Gear;
using static Il2Cpp.Utils;

namespace CunningSurvivor
{
    internal class BPMain : MelonMod
    {
        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static AssetBundle backpackBundle;

        private static GameObject backpackInst;
        private static Transform backpack;
        private static bool backpackPlaced { get; set; } = false;
        private static bool backpackPlacing { get; set; } = false;

        private static List<String> usedAttachPoints = new List<String>();

        private static Container backpackContainer;
        private static ContainerInteraction backpackContainerInteraction;


        public static readonly string modFolderName = "cunningSurvivor/backpack/";
        public static readonly string bundleName = "bundlebackpack";
        public static readonly string storedDataFolderName = "Assets/LooseFiles/";

        private static Dictionary<String, String> attachGearItems = new Dictionary<String, String>() {
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
                if (backpackPlacing == true)
                {
                    MelonLogger.Msg("PlayerManager_CancelPlaceMesh");
                    PickupBackpack(true);
                    backpackPlacing = false;
                }

            }
        }
        [HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.AttemptToPlaceMesh))]
        private static class PlayerManager_AttemptToPlaceMesh
        {
            private static void Postfix()
            {
                if (backpackPlacing == true)
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

        private static bool AttachGearItem(string gear, string attachPointName, string flag = "")
        {
            if (usedAttachPoints.Contains(attachPointName))
            {
                if (Settings.options.backPackDebug)
                {
                    MelonLogger.Msg("Attach point already used " + attachPointName);
                }
                return false;
            }

            Transform output = GearItem.InstantiateGearItem(gear).transform;
            Transform attachPoint = backpack.FindChild(attachPointName);
            UnityEngine.Object.Destroy(output.GetComponent<GearItem>());
            output.SetParent(attachPoint);
            output.Zero();
            if (flag == "lantern")
            {
                Transform originalMesh = output.FindChild("KeroseneLampB");
                Transform replacementMesh = backpack.GetParent().FindChild("Lantern_HandleUp");
                if (originalMesh)
                {
                    replacementMesh.SetParent(originalMesh.GetParent());
                    replacementMesh.GetComponent<MeshRenderer>().materials = originalMesh.GetComponent<MeshRenderer>().materials;
                    replacementMesh.localEulerAngles = Vector3.right * 270f;
                    replacementMesh.localPosition = Vector3.zero;
                    replacementMesh.localScale = Vector3.one;
                    originalMesh.gameObject.SetActive(false);
                    replacementMesh.gameObject.SetActive(true);
                }
            }
            usedAttachPoints.Add(attachPointName);
            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Set up " + gear + " " + attachPointName + " " + flag);
            }
            return true;
        }
        public override void OnUpdate()
        {
            // temp proximity check keybind
            // replace with inventory interaction/button
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.B))
            {

                if (InterfaceManager.GetInstance().AnyOverlayPanelEnabled())
                {
                    if (Settings.options.backPackDebug)
                    {
                        MelonLogger.Msg("! Overlay/Panel open");
                    }
                    return;
                }

                if (Settings.options.backPackDebug)
                {
                    MelonLogger.Msg("backpack state " + backpackPlaced);
                }

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
                    if (Settings.options.backPackDebug)
                    {
                        MelonLogger.Msg("! Overlay/Panel open");
                    }
                    return;
                }

                MelonLogger.Msg("m_Containers " + ContainerManager.m_Containers.Count);

                if (Settings.options.backPackDebug)
                {
                    MelonLogger.Msg("backpack state " + backpackPlaced);
                }

                if (backpackPlaced == false)
                {
                    HUDMessage.AddMessage("No backpack placed", true, true);
                    return;
                }
                if (backpackPlaced == true)
                {
                    IsBackpackInRange();
                    return;
                }
            }
        }

        public static void PlaceBackpackStart()
        {
            backpackPlacing = true;

            Transform player = GameManager.GetPlayerTransform();
            backpackInst = GameObject.Instantiate(backpackBundle.LoadAsset<GameObject>("backpackWithAttachPoints"));
            //backpackInst.AddComponent<MeshFilter>();
            //backpackInst.AddComponent<BoxCollider>();
            //backpackInst.AddComponent<MeshCollider>();
            backpack = GameManager.Instantiate<Transform>(backpackInst.transform.GetChild(0));
            //backpack.gameObject.transform.rotation.Set(270f, 90f, backpack.gameObject.transform.rotation.z, backpack.gameObject.transform.rotation.w);
            //backpack.SetPositionAndRotation(player.position, player.rotation);
            GameManager.Destroy(backpackInst);
            backpack.name = "[CunningSurvivor]Backpack";
            //backpack.position = player.position;
            //backpackInst.gameObject.AddComponent<BoxCollider>();
            //backpack.RotateAround(Vector3.leftVector, 0);

            backpackContainer = backpack.gameObject.AddComponent<Container>();
            backpackContainer.m_Items.Clear();
            backpackContainer.m_CapacityKG = 30;
            backpackContainer.m_LocalizedDisplayName = null;
            backpackContainer.name = "[CunningSurvivor]Backpack";
            backpackContainer.m_Inspected = true;
            backpackContainerInteraction = backpack.gameObject.AddComponent<ContainerInteraction>();
            backpackContainerInteraction.Start();
            backpackContainerInteraction.HoldText = "Open Backpack";
            backpackContainerInteraction._HoldText_k__BackingField = "Open Backpack";

            backpack.gameObject.AddComponent<ObjectGuid>();
            backpack.gameObject.GetComponent<ObjectGuid>().m_Guid = Guid.NewGuid().ToString();

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
            attachBackpackGear();


            GameManager.GetPlayerManagerComponent().StartPlaceMesh(backpack.gameObject, PlaceMeshFlags.None);
            GameManager.GetPlayerManagerComponent().m_RotationAngle = backpack.gameObject.transform.localEulerAngles.y;

            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Placing backpack");
            }


        }

        public static void PlaceBackpackComplete()
        {
            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Backpack Placed 1 | GUID | " + backpack.gameObject.GetComponent<ObjectGuid>().m_Guid);
            }
            backpackPlaced = true;
            backpackPlacing = false;
            HUDMessage.AddMessage("Backpack Placed", true, true);

            // TODO
            PopulateBackpack();


        }

        public static void PickupBackpack(bool force = false)
        {
            if (force == false)
            {
                if (!IsBackpackInRange())
                {
                    HUDMessage.AddMessage("Backpack too far away", true, true);
                    return;
                }
            }
            if (backpackPlaced == true || backpackPlacing == true)
            {
                // TODO
                UnpopulateBackpack();

                if (force == false)
                {
                    HUDMessage.AddMessage("Backpack Picked Up", true, true);
                }
                GameManager.Destroy(backpack.gameObject);
                usedAttachPoints.Clear();
                if (Settings.options.backPackDebug)
                {
                    MelonLogger.Msg("Backpack Picked Up | forced " + force);
                }
                backpackPlaced = false;
            }
        }


        public static void attachBackpackGear()
        {
            foreach (KeyValuePair<String, String> item in attachGearItems)
            {
                int GearItemCount = 1;
                String GearItemName = item.Key;

                if (item.Key.Split("|").Length == 2)
                {
                    GearItemCount = Int32.Parse(item.Key.Split("|")[1]);
                    GearItemName = item.Key.Split("|")[0];
                }

                String attachPoint = item.Value;
                String flag = item.Value.Split('_')[1];

                if (GearItemCount > 1)
                {
                    for (int i = 1; i <= GearItemCount; i++)
                    {
                        GearItem? GearItemObject = PlayerHasGearItem(GearItemName, i);
                        if (GearItemObject != null || !Settings.options.backPackCheckInv)
                        {
                            AttachGearItem(GearItemName, attachPoint + i, flag);
                            // hide/remove item from inventory here
                        }
                    }
                }
                else
                {
                    GearItem? GearItemObject = PlayerHasGearItem(GearItemName, GearItemCount);
                    if (GearItemObject != null || !Settings.options.backPackCheckInv)
                    {
                        AttachGearItem(GearItemName, attachPoint, flag);
                        // hide/remove item from inventory here
                    }

                }

            }
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

        public static float? GetBackpackDistance()
        {
            if (backpackPlaced == false)
            {
                return null;
            }
            float distance = Utils.DistanceToMainCamera(backpack.position);
            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Backpack distance " + distance + " required " + Settings.options.backPackMinDistance);
            }
            return distance;
        }

        public static bool IsBackpackInRange()
        {
            if (backpackPlaced == true)
            {
                float? distance = GetBackpackDistance();
                if (distance != null && distance <= Settings.options.backPackMinDistance)
                {
                    if (Settings.options.backPackDebug)
                    {
                        MelonLogger.Msg("Backpack IN range");
                    }
                    return true;
                }
            }
            MelonLogger.Msg("Backpack NOT in range");
            return false;
        }
    }
}
