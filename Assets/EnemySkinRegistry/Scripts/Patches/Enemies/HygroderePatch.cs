using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(BlobAI))]
    class HygroderePatch
    {
        //hit player
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BlobAI.OnCollideWithPlayer))]
        static void PrefixCollideWithPlayer(BlobAI __instance, float ___timeSinceHittingLocalPlayer, Collider other)
        {
            if (___timeSinceHittingLocalPlayer == 0f)
            {
                PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HygrodereEventHandler)?.OnHitPlayer(__instance, playerControllerB));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Slime hurt player");
            }
        }

        //kill player
        [HarmonyPrefix]
        [HarmonyPatch("eatPlayerBody")]
        static void PrefixKillPlayer(BlobAI __instance, int playerKilled)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HygrodereEventHandler)?.OnKillPlayer(__instance, StartOfRound.Instance.allPlayerScripts[playerKilled]));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Slime killed player");
        }
    }
}