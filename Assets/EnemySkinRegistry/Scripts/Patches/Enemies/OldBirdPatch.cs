using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class OldBirdPatch
    {
        internal const int ROAMING = 0;

        private static Dictionary<RadMechAI, OldBirdViewState> viewState = new Dictionary<RadMechAI, OldBirdViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.Start))]
        static void PostfixStart(RadMechAI __instance)
        {
            viewState.Add(__instance, new OldBirdViewState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), "flickerSpotlightAnim")]
        static void PrefixFlickerSpotlight(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnFlickerSpotlight(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird flickered on spotlight");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.EnableSpotlight))]
        static void PrefixEnableSpotlight(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnActivateSpotlight(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird turned on spotlight");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.DisableSpotlight))]
        static void PrefixSpotlight(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnDeactivateSpotlight(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird turned off spotlight");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.SetMechAlertedToThreat))]
        static void PrefixMechAlerted(RadMechAI __instance)
        {
            if(!__instance.isAlerted)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnAlerted(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird alerted");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.SetChargingForwardOnLocalClient))]
        static void PrefixSetCharging(RadMechAI __instance, bool charging)
        {
            if (charging != __instance.chargingForward)
            {
                if(charging)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnCharge(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird started charging");
                }
                else
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnStopCharge(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird stopped charging");
                }
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.StartFlying))]
        static void PrefixEnterFlight(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnFly(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird started flying");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.EndFlight))]
        static void PrefixEndFlight(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnLand(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird landed");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), "Stomp")]
        static void PrefixStomp(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnStomp(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird stomped");
        }

        //Server brainwash
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RadMechAI), "LateUpdate")]
        static void PostfixLateUpdate(RadMechAI __instance, float ___LRADAudio2BroadcastTimer)
        {
            if (__instance.IsServer && !__instance.LocalLRADAudio2.isPlaying && __instance.isAlerted && ___LRADAudio2BroadcastTimer > 0.05f && viewState.ContainsKey(__instance))
            {
                viewState[__instance].serverBlastBrainwash = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), "LateUpdate")]
        static void PostfixLateUpdate(RadMechAI __instance)
        {
            if(viewState.ContainsKey(__instance) && viewState[__instance].serverBlastBrainwash)
            {
                int clipIndex = Array.IndexOf(__instance.enemyType.audioClips, __instance.LocalLRADAudio2.clip);
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnBlastBrainwashing(__instance, clipIndex - 4));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird used LRAD weapon");
                viewState[__instance].serverBlastBrainwash = false;
            }
            
        }

        //client brainwash
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.ChangeBroadcastClipClientRpc))]
        static void PrefixBrainwash(RadMechAI __instance, int clipIndex)
        {
            if (!__instance.IsServer)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnBlastBrainwashing(__instance, clipIndex - 4));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird used LRAD weapon");
            }
        }

        //start aim
        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.SetAimingGun))]
        static void PrefixAim(RadMechAI __instance, bool setAiming)
        {
            if(__instance.aimingGun!=setAiming)
            {
                if(setAiming)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnStartAiming(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird started aiming");
                }
                else
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnStopAiming(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird stopped aiming");
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.ShootGun))]
        static void PrefixShoot(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnShootGun(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird fired missile");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), "BeginTorchPlayer")]
        static void PrefixBeginTorchPlayer(RadMechAI __instance, PlayerControllerB playerBeingTorched)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnGrabPlayer(__instance, playerBeingTorched));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird began torching player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.EnableBlowtorch))]
        static void PrefixEnableBlowtorch(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnTorchPlayer(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird turned on blowtorch");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(RadMechAI), nameof(RadMechAI.CancelTorchPlayerAnimation))]
        static void PrefixCancelTorchPlayer(RadMechAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnEndTorchPlayer(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird old bird turned off blowtorch");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is RadMechAI)
            {
                switch (stateIndex)
                {
                    case ROAMING:
                        if (!__instance.inSpecialAnimation)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnAlertEnded(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird lowered alert");
                        }
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is RadMechAI && __instance != null && viewState.ContainsKey(__instance as RadMechAI))
            {
                viewState.Remove(__instance as RadMechAI);
            }
        }

        private class OldBirdViewState
        {
            public bool serverBlastBrainwash = false;
        }


    }
}