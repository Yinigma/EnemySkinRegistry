using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using UnityEngine;
using GameNetcodeStuff;

namespace AntlerShed.SkinRegistry
{
    class BunkerSpiderPatch
    {
        internal const int WEB_PLACING = 0;
        internal const int WAITING = 1;
        internal const int CHASING = 2;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SandSpiderAI), "turnBodyIntoWeb")]
        static void PrefixSpoolBody(SandSpiderAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            if (___currentlyHeldBody != null)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnWrapBody(__instance, ___currentlyHeldBody));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider Spooled Body");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SandSpiderAI), "CancelSpoolingBody")]
        static void PrefixCancelSpoolBody(SandSpiderAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnCancelWrappingBody(__instance, ___currentlyHeldBody));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider Spooling interrupted");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(SandSpiderAI), "HangBodyFromCeiling")]
        static void PrefixHangBody(SandSpiderAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            if (___currentlyHeldBody != null)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnHangBody(__instance, ___currentlyHeldBody));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider Hang Body");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex)
            {
                if (__instance is SandSpiderAI)
                {
                    switch (stateIndex)
                    {
                        case WEB_PLACING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterWebbingState(__instance as SandSpiderAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider Enter Webbing State");
                            break;
                        case WAITING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterWaitingState(__instance as SandSpiderAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"Spider entered waiting state");
                            break;
                        case CHASING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterChasingState(__instance as SandSpiderAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider entered chasing state");
                            break;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SandSpiderAI), nameof(SandSpiderAI.OnCollideWithPlayer))]
        static void PostfixOnCollideWithPlayer(SandSpiderAI __instance, float ___timeSinceHittingPlayer, bool ___spoolingPlayerBody, Collider other)
        {
            if (___timeSinceHittingPlayer == 0f)
            {
                PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other, ___spoolingPlayerBody);
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnAttackPlayer(__instance, playerControllerB));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider hit player");
            }
        }
    }
}