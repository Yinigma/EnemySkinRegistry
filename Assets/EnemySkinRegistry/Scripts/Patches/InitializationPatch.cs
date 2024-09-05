using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

namespace AntlerShed.SkinRegistry
{
    class InitializationPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), "Awake")]
        static void PostfixOnAwake()
        {
            EnemySkinRegistry.InitConfig();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(GameNetworkManager), "StartDisconnect")]
        static void PrefixDisconnect()
        {
            if(NetworkManager.Singleton!=null)
            {
                if (NetworkManager.Singleton.IsHost)
                {
                    EnemySkinRegistry.StopConfigServer();
                }
                else
                {
                    EnemySkinRegistry.StopConfigClient();
                }
            }
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(MenuManager), nameof(MenuManager.StartHosting))]
        static void PostfixStartHosting()
        {
            EnemySkinRegistry.StartConfigServer();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnClientConnect))]
        static void PostfixClientConnect()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                EnemySkinRegistry.StartConfigClient();
            }
        }
        
         */

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer(PlayerControllerB __instance)
        {
            if (__instance.IsHost)
            {
                EnemySkinRegistry.StartConfigServer();
            }
            else
            {
                EnemySkinRegistry.StartConfigClient();
            }
        }

        
    }
}


