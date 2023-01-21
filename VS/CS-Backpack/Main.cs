namespace CunningSurvivor
{
    internal class BPMain : MelonMod
    {
        public static Shader vanillaSkinnedShader;
        public static Shader vanillaDefaultShader;

        public static AssetBundle backpackBundle;

        public static Transform backpack;


        public static readonly string modFolderName = "cunningSurvivor/backpack/";
        public static readonly string bundleName = "bundlebackpack";
        public static readonly string storedDataFolderName = "Assets/LooseFiles/";


        public override void OnApplicationStart()
        {
            // Load assets
            backpackBundle = AssetBundle.LoadFromFile("Mods/" + modFolderName + bundleName);

            // Get shaders
            vanillaSkinnedShader = Shader.Find("Shader Forge/TLD_StandardSkinned");
            vanillaDefaultShader = Shader.Find("Shader Forge/TLD_StandardDiffuse");

            // Enable settings
            Settings.OnLoad();
        }

        private static Transform PrepareGear(string gear, string attachPointName, string flag = "")
        {
            Transform output = GearItem.InstantiateGearItem(gear).transform;
            Transform attachPoint = backpack.FindChild(attachPointName);
            UnityEngine.Object.Destroy(output.GetComponent<GearItem>());
            output.SetParent(attachPoint);
            output.Zero();
            if (flag == "lamp")
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
            //MelonLogger.Msg("Set up " + gear + " " + attachPointName);
            return output; 
        }
        public override void OnUpdate()
        {

            if (InputManager.GetKeyDown(InputManager.m_CurrentContext, KeyCode.B))
            {
                HUDMessage.AddMessage("Spawning backpack", true, true);

                Transform player = GameManager.GetPlayerTransform();
                backpack = GameObject.Instantiate(backpackBundle.LoadAsset<GameObject>("backpackWithAttachPoints")).transform;
                backpack = backpack.GetChild(0);
                backpack.name = "[CunningSurvivor]Backpack";
                backpack.position = player.position;
                //backpack.gameObject.AddComponent<GearItem>();
                backpack.gameObject.GetComponent<MeshRenderer>().materials[0].shader = vanillaSkinnedShader;
                backpack.gameObject.GetComponent<MeshRenderer>().materials[1].shader = vanillaSkinnedShader;
                backpack.gameObject.GetComponent<MeshRenderer>().materials[2].shader = vanillaSkinnedShader;

                PrepareGear("GEAR_BearSkinBedRoll", "attachPoint_bedroll");
                PrepareGear("GEAR_BedRoll", "attachPoint_bedroll");
                PrepareGear("GEAR_Bow", "attachPoint_bow");
                PrepareGear("GEAR_RecycledCan", "attachPoint_can");
                PrepareGear("GEAR_BlueFlare", "attachPoint_flareBlue");
                PrepareGear("GEAR_FlareA", "attachPoint_flareRed");
                PrepareGear("GEAR_Flashlight", "attachPoint_flashlight");
                PrepareGear("GEAR_Hatchet", "attachPoint_hatchet");
                PrepareGear("GEAR_HatchetImprovised", "attachPoint_hatchet");
                PrepareGear("GEAR_KeroseneLampB", "attachPoint_lantern", "lamp");
                PrepareGear("GEAR_CookingPot", "attachPoint_pot");
                PrepareGear("GEAR_Prybar", "attachPoint_prybar");
                PrepareGear("GEAR_Rope", "attachPoint_rope");
                PrepareGear("GEAR_SprayPaintCan", "attachPoint_sprayCan");
                PrepareGear("GEAR_Torch", "attachPoint_torch");
                PrepareGear("GEAR_Arrow", "attachPoint_arrow1");
                PrepareGear("GEAR_Arrow", "attachPoint_arrow2");
                PrepareGear("GEAR_Arrow", "attachPoint_arrow3");

            }
        }

        public static void ChangeTex(string variant)
        {
            if (variant == "astrid")
            {
                backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainA.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backA.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailA.png");
            }

            if (variant == "will")
            {
                backpack.gameObject.GetComponent<MeshRenderer>().materials[0].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/mainW.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[1].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/backW.png");
                backpack.gameObject.GetComponent<MeshRenderer>().materials[2].mainTexture = backpackBundle.LoadAsset<Texture>(storedDataFolderName + "Tex/detailW.png");
            }
        }
    }
}
