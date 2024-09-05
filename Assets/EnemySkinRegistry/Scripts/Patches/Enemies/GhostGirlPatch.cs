using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class GhostGirlPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(DressGirlAI), "ChoosePlayerToHaunt")]
        static void PostfixChoosePlayer(DressGirlAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnChoosePlayer(__instance, __instance.hauntingPlayer));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Haunting Player");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DressGirlAI), "BeginChasing")]
        static void PostfixBeginChasing(DressGirlAI __instance)
        {
            if (__instance.currentBehaviourStateIndex != 1)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStartChasing(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Started Chasing Player");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DressGirlAI), "StopChasing")]
        static void PostfixStopChasing(DressGirlAI __instance)
        {

            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStopChasing(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Stopped Chasing Player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DressGirlAI), "disappearOnDelay")]
        static void PrefixDisappear(DressGirlAI __instance)
        {

            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as GhostGirlEventHandler)?.OnStartToDisappear(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Will Disappear in 0.1 seconds");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DressGirlAI), nameof(DressGirlAI.OnCollideWithPlayer))]
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

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.EnableEnemyMesh))]
        static void PostFixEnableMeshes(EnemyAI __instance, bool enable)
        {
            List<EnemyEventHandler> handlers = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            if (__instance is DressGirlAI)
            {
                if (enable)
                {
                    handlers.ForEach((handler) => (handler as GhostGirlEventHandler)?.OnShow(__instance as DressGirlAI));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Set to Show");
                }
                else
                {
                    handlers.ForEach((handler) => (handler as GhostGirlEventHandler)?.OnHide(__instance as DressGirlAI));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Ghost Girl Set to Hide");

                }
            }
        }
    }
}