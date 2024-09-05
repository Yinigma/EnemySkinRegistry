using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class ButlerPatch
    {
        internal const int SWEEPING = 0;
        internal const int PREMEDITATING = 1;
        internal const int MURDERING = 2;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ButlerBeesEnemyAI), nameof(ButlerBeesEnemyAI.Start))]
        static void PostfixHornetsSpawned(ButlerBeesEnemyAI __instance)
        {
            //find closest butler
            if(__instance!=null)
            {
                ButlerEnemyAI butler = GameObject.FindObjectsOfType<ButlerEnemyAI>().Aggregate
                (
                   null,
                   (ButlerEnemyAI current, ButlerEnemyAI next) =>
                   {
                       float cdist = current == null ? float.PositiveInfinity : Vector3.Distance(current.transform.position, __instance.transform.position);
                       float ndist = next == null ? float.PositiveInfinity : Vector3.Distance(next.transform.position, __instance.transform.position);
                       return ndist < cdist ? next : current;
                   }
                );
                if (butler != null)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(butler).ForEach((handler) => (handler as ButlerEventHandler)?.OnSpawnHornets(butler, __instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Masked Hornets Spawned");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.OnCollideWithPlayer))]
        static void PostfixOnCollideWithPlayer(ButlerEnemyAI __instance, float ___timeSinceHittingPlayer, Collider other)
        {
            if (___timeSinceHittingPlayer == 0f)
            {
                PlayerControllerB playerControllerB = __instance.MeetsStandardPlayerCollisionConditions(other);
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnStabPlayer(__instance, playerControllerB));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler stabbed player");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex)
            {
                if (__instance is ButlerEnemyAI)
                {
                    switch (stateIndex)
                    {
                        case SWEEPING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterSweepingState(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered sweeping state");
                            break;
                        case PREMEDITATING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterPremeditatingState(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered premeditating state");
                            break;
                        case MURDERING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterMurderingState(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered murdering state");
                            break;
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(ButlerEnemyAI), "ButlerBlowUpAndPop")]
        static void PrefixBlowUp(ButlerEnemyAI __instance)
        {
            __instance.StartCoroutine(ButlerPopTracker(__instance));
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnInflate(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler started inflating (making it big and round)");
        }

        private static IEnumerator ButlerPopTracker(ButlerEnemyAI __instance)
        {
            yield return new WaitForSeconds(1.1f);
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnPop(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler popped (it was only wafer thin)");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.AnimationEventA))]
        static void PostfixStep(ButlerEnemyAI __instance, float ___timeAtLastFootstep)
        {
            if(Mathf.Abs(Time.realtimeSinceStartup - ___timeAtLastFootstep) < 0.01f)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnStep(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler step audio cue");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.AnimationEventB))]
        static void PrefixSweep(ButlerEnemyAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnSweep(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler sweep audio cue");
        }
    }
}