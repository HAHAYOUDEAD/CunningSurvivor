namespace CunningSurvivor
{
    public class Utility
    {
        public static bool IsScenePlayable()
        {
            return !(string.IsNullOrEmpty(GameManager.m_ActiveScene) || GameManager.m_ActiveScene.Contains("MainMenu") || GameManager.m_ActiveScene == "Boot" || GameManager.m_ActiveScene == "Empty");
        }

        public static bool IsScenePlayable(string scene)
        {
            return !(string.IsNullOrEmpty(scene) || scene.Contains("MainMenu") || scene == "Boot" || scene == "Empty");
        }


    }

    public static class Extensions
    {
        public static void Zero(this Transform transform, int X = 90, int Y = 0, int Z = 0)
        {
            //transform.position = Vector3.zero;
            transform.localPosition = Vector3.zero;
            //transform.rotation = Quaternion.identity;
            transform.SetLocalEulerAngles(new Vector3(X, Y, Z), RotationOrder.OrderXYZ);
        }
    }

}