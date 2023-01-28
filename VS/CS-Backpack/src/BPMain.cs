namespace CunningSurvivor
{
    internal class BPMain : MelonMod
    {
        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static AssetBundle backpackBundle;

        public static GameObject backpackParent;
        public static Transform backpack;
        public static bool backpackPlaced { get; set; } = false;
        public static bool backpackPlacing { get; set; } = false;

        public static Container backpackContainer;
        public static ContainerInteraction backpackInteraction;

        public static readonly string modFolderName = "cunningSurvivor/backpack/";
        public static readonly string bundleName = "bundlebackpack";
        public static readonly string storedDataFolderName = "Assets/LooseFiles/";


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

        public override void OnUpdate()
        {
            // temp placement keybind
            // replace with inventory interaction/button
            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.B))
            {

                if (InterfaceManager.GetInstance().AnyOverlayPanelEnabled())
                {
                    BPUtils.DebugMsg("! Overlay/Panel open");
                    return;
                }

                BPUtils.DebugMsg("backpack state " + backpackPlaced);


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
                    BPUtils.DebugMsg("! Overlay/Panel Open");
                    return;
                }

                BPUtils.DebugMsg("backpack state " + backpackPlaced);

                if (backpackPlaced == false)
                {
                    BPUtils.DebugMsg("No Backpack Placed");
                    return;
                }
                if (backpackPlaced == true && backpackPlacing == false)
                {
                    try
                    {
                        backpackInteraction.PerformInteraction();
                    }
                    catch (Exception e)
                    {
                        BPUtils.DebugMsg(e.ToString());
                    }
                    
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
            backpackParent = GameObject.Instantiate(backpackBundle.LoadAsset<GameObject>("backpackWithAttachPoints"));
            backpackParent.layer = vp_Layer.Container;
            backpack = backpackParent.transform.GetChild(0);
            backpack.gameObject.layer = vp_Layer.Container;
            backpackParent.name = "[CunningSurvivor]Backpack_Parent";
            backpack.name = "[CunningSurvivor]Backpack";

            //MeshCollider BPCollider = backpackParent.gameObject.AddComponent<MeshCollider>();
            //Backpack.gameObject.AddComponent<BoxCollider>();


            BPInventory.InitBackpackContainer();

            backpack.gameObject.AddComponent<ContainerInteraction>();
            backpackInteraction = backpack.GetComponent<ContainerInteraction>();
            backpackInteraction.enabled = true;
            backpackInteraction.gameObject.SetActive(true);
            backpackInteraction.CanInteract = true;
            backpackInteraction.HoverText = "Open Backpack\nRMB - Move Backpack";
            backpackInteraction.m_DefaultHoverText = null;


            //BackpackInteraction.Start();
            //BackpackInteraction.HoldText = "Open Backpack";
            //BackpackInteraction._HoldText_k__BackingField = "Open Backpack";

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

            GameManager.GetPlayerManagerComponent().StartPlaceMesh(backpackParent.gameObject, PlaceMeshFlags.None);
            GameManager.GetPlayerManagerComponent().m_RotationAngle = backpackParent.gameObject.transform.localEulerAngles.y;

            BPCarryCapacity.Update();

            BPUtils.DebugMsg("Placing backpack");


        }

        public static void PlaceBackpackComplete()
        {
            backpackPlaced = true;
            backpackPlacing = false;
            BPCarryCapacity.Update();
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


                BPAttachPoints.Clear();
                GameObject.Destroy(backpackParent.gameObject);
                GameObject.Destroy(backpack.gameObject);
                backpackContainer.DestroyAllGear();
                BPUtils.DebugMsg("Backpack Picked Up | forced " + force);
                backpackPlaced = false;
            }
            BPCarryCapacity.Update();
        }








    }
}
