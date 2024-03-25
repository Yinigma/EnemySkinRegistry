using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(DressGirlAI))]
    class GhostGirlPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("ChoosePlayerToHaunt")]
        static void PostfixChoosePlayer(DressGirlAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnChoosePlayer(__instance, __instance.hauntingPlayer));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Haunting Player");
        }

        [HarmonyPostfix]
        [HarmonyPatch("BeginChasing")]
        static void PostfixBeginChasing(DressGirlAI __instance)
        {
            if (__instance.currentBehaviourStateIndex != 1)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStartChasing(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Started Chasing Player");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("StopChasing")]
        static void PostfixStopChasing(DressGirlAI __instance)
        {

            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStopChasing(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Stopped Chasing Player");
        }

        [HarmonyPrefix]
        [HarmonyPatch("disappearOnDelay")]
        static void PrefixDisappear(DressGirlAI __instance)
        {

            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStartToDisappear(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Will Disappear in 0.1 seconds");
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(DressGirlAI.OnCollideWithPlayer))]
        static void PrefixOnCollideWithPlayer(DressGirlAI __instance, Collider other)
        {
            if (!__instance.hauntingLocalPlayer)
            {
                return;
            }

            PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, inKillAnimation: false, overrideIsInsideFactoryCheck: true);
            if (!(playerControllerB != null))
            {
                return;
            }

            if (playerControllerB == __instance.hauntingPlayer)
            {
                if (__instance.currentBehaviourStateIndex == 1)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnKillPlayer(__instance, __instance.hauntingPlayer));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl killed player");
                }
            }
        }
    }
}