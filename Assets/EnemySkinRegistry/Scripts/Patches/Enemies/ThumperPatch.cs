using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class ThumperPatch
    {
        internal const int SEARCHING = 0;
        internal const int CHASING = 1;

        private static Dictionary<CrawlerAI, ThumperViewState> viewState = new Dictionary<CrawlerAI, ThumperViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CrawlerAI), nameof(CrawlerAI.Start))]
        static void PostfixStart(CrawlerAI __instance)
        {
            viewState.Add(__instance, new ThumperViewState());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(CrawlerAI), "MakeScreech")]
        static void PostfixScreech(CrawlerAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnScreech(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper Hit a wall");
        }

        //Hit player - bite - plays bite through voice, plays bite animation, do check of 0 seconds since hitting player in oncollide
        [HarmonyPostfix]
        [HarmonyPatch(typeof(CrawlerAI), nameof(CrawlerAI.OnCollideWithPlayer))]
        static void PostfixCollideWithPlayer(CrawlerAI __instance, float ___timeSinceHittingPlayer, Collider other)
        {
            if(___timeSinceHittingPlayer == 0f)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnBitePlayer(__instance, __instance.MeetsStandardPlayerCollisionConditions(other)));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper bit player");
            }
        }

        //Hit wall
        [HarmonyPrefix]
        [HarmonyPatch(typeof(CrawlerAI), "CalculateAgentSpeed")]
        static void PrefixCalculateSpeed(CrawlerAI __instance, Vector3 ___previousPosition, float ___averageVelocity, float ___wallCollisionSFXDebounce)
        {
            //copied code. Check this when the game updates
            float speedMeasure = (__instance.transform.position - ___previousPosition).magnitude / (Time.deltaTime / 1.4f);
            if (__instance.IsOwner && ___averageVelocity - speedMeasure > Mathf.Clamp(speedMeasure * 0.17f, 2f, 100f) && speedMeasure > 3f && __instance.currentBehaviourStateIndex == 1)
            {
                if (___wallCollisionSFXDebounce > 0.5f)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnHitWall(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper Hit a wall");
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CrawlerAI), nameof(CrawlerAI.Update))]
        static void PrefixUpdate(CrawlerAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            if(viewState.ContainsKey(__instance))
            {
                if(viewState[__instance].lastFrameHeldBody != ___currentlyHeldBody && ___currentlyHeldBody != null)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnEatPlayer(__instance, ___currentlyHeldBody));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper ate player");
                }
                viewState[__instance].lastFrameHeldBody = ___currentlyHeldBody;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CrawlerAI), "DropPlayerBody")]
        static void PrefixDropBody(CrawlerAI __instance, DeadBodyInfo ___currentlyHeldBody)
        {
            if(___currentlyHeldBody!=null)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnDropBody(__instance, ___currentlyHeldBody));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper dropped body");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is CrawlerAI)
            {
                switch (stateIndex)
                {
                    case SEARCHING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnEnterSearchState(__instance as CrawlerAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper entered search state");
                        break;
                    case CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnEnterChaseState(__instance as CrawlerAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper entered chasing state");
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is CrawlerAI && __instance != null && viewState.ContainsKey(__instance as CrawlerAI))
            {
                viewState.Remove(__instance as CrawlerAI);
            }
        }

        private class ThumperViewState
        {
            public DeadBodyInfo lastFrameHeldBody = null;
        }

    }
}