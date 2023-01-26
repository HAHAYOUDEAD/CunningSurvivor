namespace CunningSurvivor
{
    internal static class BPAttachPoints
    {

        public static List<String> usedAttachPoints = new();


        public static void Clear()
        {
            usedAttachPoints.Clear();
        }

        public static void AttachBackpackGear()
        {
            foreach (KeyValuePair<String, String> item in BPParams.attachableGearItems)
            {
                int gearItemCount = 1;
                String gearItemName = item.Key;

                if (item.Key.Split("|").Length == 2)
                {
                    gearItemCount = Int32.Parse(item.Key.Split("|")[1]);
                    gearItemName = item.Key.Split("|")[0];
                }

                String attachPoint = item.Value;
                String flag = item.Value.Split('_')[1];

                if (gearItemCount > 1)
                {
                    for (int i = 1; i <= gearItemCount; i++)
                    {
                        GearItem? gearItemObj = BPInventory.PlayerHasGearItem(gearItemName, i);
                        if (gearItemObj != null || !Settings.options.backPackCheckInv)
                        {
                            BPAttachPoints.AttachGearItem(gearItemName, attachPoint + i, flag);
                            // hide/remove item from inventory here
                        }
                    }
                }
                else
                {
                    GearItem? gearItemObj = BPInventory.PlayerHasGearItem(gearItemName, gearItemCount);
                    if (gearItemObj != null || !Settings.options.backPackCheckInv)
                    {
                        BPAttachPoints.AttachGearItem(gearItemName, attachPoint, flag);
                        // hide/remove item from inventory here
                    }

                }

            }
        }
        public static bool AttachGearItem(string gear, string attachPointName, string flag = "")
        {
            if (usedAttachPoints.Contains(attachPointName))
            {
                BPUtils.DebugMsg("Attach point already used " + attachPointName);
                return false;
            }

            Transform output = GearItem.InstantiateGearItem(gear).transform;
            Transform attachPoint = BPMain.backpack.FindChild(attachPointName);
            UnityEngine.Object.Destroy(output.GetComponent<GearItem>());
            output.SetParent(attachPoint);
            output.Zero();
            if (flag == "lantern")
            {
                Transform originalMesh = output.FindChild("KeroseneLampB");
                Transform replacementMesh = BPMain.backpack.GetParent().FindChild("Lantern_HandleUp");
                if (originalMesh)
                {
                    replacementMesh.SetParent(originalMesh.GetParent());
                    replacementMesh.GetComponent<MeshRenderer>().materials = originalMesh.GetComponent<MeshRenderer>().materials;
                    replacementMesh.localEulerAngles = Vector3.zero;
                    replacementMesh.localPosition = Vector3.zero;
                    replacementMesh.localScale = Vector3.one;
                    originalMesh.gameObject.SetActive(false);
                    replacementMesh.gameObject.SetActive(true);
                }
            }
            usedAttachPoints.Add(attachPointName);
            BPUtils.DebugMsg("Set up " + gear + " " + attachPointName + " " + flag);
            
            return true;
        }

    }

}