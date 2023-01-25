﻿namespace CunningSurvivor
{
    internal static class BPAttachPoints
    {

        private static List<String> UsedAttachPoints = new();


        public static void Clear()
        {
            UsedAttachPoints.Clear();
        }

        public static void AttachBackpackGear()
        {
            foreach (KeyValuePair<String, String> item in BPMain.AttachableGearItems)
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
                        GearItem? GearItemObject = BPInventory.PlayerHasGearItem(GearItemName, i);
                        if (GearItemObject != null || !Settings.options.backPackCheckInv)
                        {
                            BPAttachPoints.AttachGearItem(GearItemName, attachPoint + i, flag);
                            // hide/remove item from inventory here
                        }
                    }
                }
                else
                {
                    GearItem? GearItemObject = BPInventory.PlayerHasGearItem(GearItemName, GearItemCount);
                    if (GearItemObject != null || !Settings.options.backPackCheckInv)
                    {
                        BPAttachPoints.AttachGearItem(GearItemName, attachPoint, flag);
                        // hide/remove item from inventory here
                    }

                }

            }
        }
        public static bool AttachGearItem(string gear, string attachPointName, string flag = "")
        {
            if (UsedAttachPoints.Contains(attachPointName))
            {
                if (Settings.options.backPackDebug)
                {
                    MelonLogger.Msg("Attach point already used " + attachPointName);
                }
                return false;
            }

            Transform output = GearItem.InstantiateGearItem(gear).transform;
            Transform attachPoint = BPMain.Backpack.FindChild(attachPointName);
            UnityEngine.Object.Destroy(output.GetComponent<GearItem>());
            output.SetParent(attachPoint);
            output.Zero();
            if (flag == "lantern")
            {
                Transform originalMesh = output.FindChild("KeroseneLampB");
                Transform replacementMesh = BPMain.Backpack.GetParent().FindChild("Lantern_HandleUp");
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
            UsedAttachPoints.Add(attachPointName);
            if (Settings.options.backPackDebug)
            {
                MelonLogger.Msg("Set up " + gear + " " + attachPointName + " " + flag);
            }
            return true;
        }

    }

}