using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class HoarderBugPatch
    {
        internal const int SCAVENGING = 0;
        internal const int RETURNING = 1;
        internal const int CHASING = 2;

        private static Dictionary<HoarderBugAI, BugViewState> viewState = new Dictionary<HoarderBugAI, BugViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HoarderBugAI), nameof(HoarderBugAI.Start))]
        static void PostfixStart(HoarderBugAI __instance)
        {
            viewState.Add(__instance, new BugViewState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(HoarderBugAI), nameof(HoarderBugAI.Update))]
        static void PrefixUpdate(HoarderBugAI __instance, bool ___inChase)
        {
            if (__instance.previousBehaviourStateIndex != CHASING)
            {
                switch (__instance.currentBehaviourStateIndex)
                {
                    case CHASING:
                        if (!___inChase)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnEnterChasingState(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug entered chasing state");
                        }
                        break;
                }
            }
        }

        //pick up scrap
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HoarderBugAI), "GrabItem")]
        static void PrefixGrabItem(HoarderBugAI __instance, bool ___sendingGrabOrDropRPC, NetworkObject item)
        {
            if (!___sendingGrabOrDropRPC)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnPickUpItem(__instance, item.gameObject.GetComponent<GrabbableObject>()));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug grabbed item");
            }
        }

        //drop off scrap
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HoarderBugAI), "DropItem")]
        static void PrefixDropItem(HoarderBugAI __instance, bool ___sendingGrabOrDropRPC, NetworkObject item)
        {
            if (!___sendingGrabOrDropRPC)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnDropItem(__instance, item.gameObject.GetComponent<GrabbableObject>()));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug dropped item");
            }
        }


        //land
        [HarmonyPrefix]
        [HarmonyPatch(typeof(HoarderBugAI), "ExitChaseMode")]
        static void PrefixExitChase(HoarderBugAI __instance, bool ___inChase)
        {
            if (___inChase)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnExitChasingState(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug exited chasing state");
            }
        }

        //hit player
        [HarmonyPostfix]
        [HarmonyPatch(typeof(HoarderBugAI), nameof(HoarderBugAI.OnCollideWithPlayer))]
        static void PrefixCollideWithPlayer(HoarderBugAI __instance, float ___timeSinceHittingPlayer, Collider other)
        {
            if (___timeSinceHittingPlayer == 0f)
            {
                PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnHitPlayer(__instance, playerControllerB));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug entered chasing state");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(HoarderBugAI), "DetectAndLookAtPlayers")]
        static void PostfixLookAtPLayers(HoarderBugAI __instance)
        {
            if(viewState.ContainsKey(__instance) && viewState[__instance].prevWatchedPlayer != __instance.watchingPlayer)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as HoarderBugEventHandler)?.OnSwitchLookAtPlayer(__instance, __instance.watchingPlayer));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bug switched what it was looking at");
            }
            viewState[__instance].prevWatchedPlayer = __instance.watchingPlayer;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is HoarderBugAI && __instance != null && viewState.ContainsKey(__instance as HoarderBugAI))
            {
                viewState.Remove(__instance as HoarderBugAI);
            }
        }

        private class BugViewState
        {
            public PlayerControllerB prevWatchedPlayer = null;
        }

    }
}