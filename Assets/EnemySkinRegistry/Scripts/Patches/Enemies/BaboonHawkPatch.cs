using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{

    [HarmonyPatch(typeof(BaboonBirdAI))]
    class BaboonHawkPatch
    {
        //your guess is as good as mine
        private const int NONE = 0;
        private const int INTIMIDATE = 1;
        private const int ATTACK = 2;

        private static Dictionary<BaboonBirdAI, BaboonHawkViewState> viewState = new Dictionary<BaboonBirdAI, BaboonHawkViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.Start))]
        static void PostfixStart(BaboonBirdAI __instance, bool ___doingKillAnimation)
        {
            viewState[__instance] = new BaboonHawkViewState();
            viewState[__instance].inKillAnimation = ___doingKillAnimation;
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.OnCollideWithPlayer))]
        static void PostfixOnCollideWithPlayer(BaboonBirdAI __instance, bool ___doingKillAnimation, float ___timeSinceHitting, Collider other)
        {
            if (___timeSinceHitting >= 0.5f)
            {
                Vector3 vector = Vector3.Normalize(__instance.transform.position + Vector3.up * 0.7f - (other.transform.position + Vector3.up * 0.4f)) * 0.5f;
                if (Physics.Linecast(__instance.transform.position + Vector3.up * 0.7f + vector, other.transform.position + Vector3.up * 0.4f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
                {
                    PlayerControllerB controller = __instance.MeetsStandardPlayerCollisionConditions(other, __instance.inSpecialAnimation || ___doingKillAnimation);
                    if (controller != null)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnAttackPlayer(__instance, controller));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Attack Player");
                    }
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(BaboonBirdAI.OnCollideWithEnemy))]
        static void PrefixOnCollideWithEnemy(BaboonBirdAI __instance, float ___timeSinceHitting, EnemyAI enemyScript)
        {
            if (!(enemyScript.enemyType == __instance.enemyType) && !(___timeSinceHitting < 0.75f) && __instance.IsOwner && enemyScript.enemyType.canDie)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler)=>(handler as BaboonHawkEventHandler)?.OnAttackEnemy(__instance, enemyScript));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Attack Enemy");
            }
        }
        

        [HarmonyPrefix]
        [HarmonyPatch("killPlayerAnimation")]
        static void PrefixKillAnimation(BaboonBirdAI __instance, int playerObject)
        {
            PlayerControllerB killedPlayer = StartOfRound.Instance.allPlayerScripts[playerObject];
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnKillPlayer(__instance, killedPlayer));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Start Kill Player");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.StopKillAnimation))]
        static void PostfixStopKillAnimation(BaboonBirdAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnFinishKillPlayerAnimation(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Cancel Kill Animation");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.EnemyEnterRestModeClientRpc))]
        static void PostfixEnterRestMode(BaboonBirdAI __instance, bool sleep)
        {
            if (viewState.ContainsKey(__instance))
            {
                if (sleep && !viewState[__instance].sleeping)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnSleep(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Sleep");
                }
                else if (!sleep && !viewState[__instance].sitting)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnSit(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO)  EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Sit");
                }
                viewState[__instance].sleeping = sleep;
                viewState[__instance].sitting = !sleep;
            }
        }

        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.EnemyGetUpClientRpc))]
        static void PostfixEnemyGetUp(BaboonBirdAI __instance)
        {
            if(viewState.ContainsKey(__instance) && ( viewState[__instance].sitting || viewState[__instance].sleeping ) )
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnGetUp(__instance));
                //EnemySkins.SkinLogger.LogInfo("BB Hawk Get Up");
                viewState[__instance].sleeping = false;
                viewState[__instance].sitting = false;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.SetAggressiveModeClientRpc))]
        static void PostfixSetAggressionMode(BaboonBirdAI __instance, int mode)
        {
            if(viewState.ContainsKey(__instance) && viewState[__instance].aggressionState != mode)
            {
                viewState[__instance].aggressionState = mode;
                switch (mode)
                {
                    case NONE:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnCalmDown(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Calm Down");
                        break;
                    case INTIMIDATE:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnIntimidate(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Intimidate");
                        break;
                    case ATTACK:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BaboonHawkEventHandler)?.OnEnterAttackMode(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Attack State");
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrabScrap")]
        static void PostfixGrabScrap(BaboonBirdAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandlers(__instance) as BaboonHawkEventHandler)?.OnPickUpScrap(__instance, __instance.heldScrap);
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk pick up scrap");
        }

        [HarmonyPostfix]
        [HarmonyPatch("DropScrap")]
        static void PostfixDropScrap(BaboonBirdAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandlers(__instance) as BaboonHawkEventHandler)?.OnDropScrap(__instance);
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Drop Scrap");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.Update))]
        static void PostfixUpdate(BaboonBirdAI __instance, bool ___doingKillAnimation)
        {
            if(viewState.ContainsKey(__instance))
            {
                if(viewState[__instance].inKillAnimation != ___doingKillAnimation)
                {
                    if (!___doingKillAnimation)
                    {
                        (EnemySkinRegistry.GetEnemyEventHandlers(__instance) as BaboonHawkEventHandler)?.OnFinishKillPlayerAnimation(__instance);
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk End Kill");
                    }
                    viewState[__instance].inKillAnimation = ___doingKillAnimation;
                }
            }
        }

        internal static void RemoveViewState(BaboonBirdAI __instance)
        {
            viewState.Remove(__instance);
        }

        //This is a big hack, but otherwise I'm gonna have to do runtime netcode patches for what's supposed to be a client-side mod. Nah.
        private class BaboonHawkViewState
        {
            public bool inKillAnimation;
            public int aggressionState;
            public bool sleeping;
            public bool sitting;
        }
    }
}