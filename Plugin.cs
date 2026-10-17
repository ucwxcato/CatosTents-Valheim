using BepInEx;
using BepInEx.Logging;
using Jotunn.Managers;

namespace CatosTents
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("com.jotunn.jotunn")]
    public sealed class Plugin : BaseUnityPlugin
    {
        public const string GUID = "com.catosvalheim.catostents";
        public const string NAME = "CatosTents";
        public const string VERSION = "0.1.0";

        internal static ManualLogSource Log;

        private void Awake()
        {
            Log = Logger;
            PrefabManager.OnVanillaPrefabsAvailable += TentPiece.Register;
            Log.LogInfo($"{NAME} {VERSION} loaded; waiting for vanilla prefabs");
        }

        private void OnDestroy()
        {
            PrefabManager.OnVanillaPrefabsAvailable -= TentPiece.Register;
        }
    }
}
