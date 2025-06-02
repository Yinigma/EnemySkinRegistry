using GameNetcodeStuff;
using HarmonyLib;
using LethalLevelLoader;
using SoftMasking;
using Unity.Netcode;
using UnityEngine;

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
            if (NetworkManager.Singleton != null)
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(PlayerControllerB), "ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer(PlayerControllerB __instance)
        {
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Initializing Enemy Skin Registry for local player");

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


