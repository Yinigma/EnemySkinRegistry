using AntlerShed.EnemySkinKit.Events;
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
                        (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnAttackPlayer(__instance, controller);
                        //EnemySkins.SkinLogger.LogInfo("BB Hawk Attack Player");
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
                (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnAttackEnemy(__instance, enemyScript);
                //EnemySkins.SkinLogger.LogInfo("BB Hawk Attack Enemy");
            }
        }
        

        [HarmonyPrefix]
        [HarmonyPatch("killPlayerAnimation")]
        static void PrefixKillAnimation(BaboonBirdAI __instance, int playerObject)
        {
            PlayerControllerB killedPlayer = StartOfRound.Instance.allPlayerScripts[playerObject];
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnKillPlayer(__instance, killedPlayer);
            //EnemySkins.SkinLogger.LogInfo("BB Hawk Start Kill Player");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.StopKillAnimation))]
        static void PostfixStopKillAnimation(BaboonBirdAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnFinishKillPlayerAnimation(__instance);
            //EnemySkins.SkinLogger.LogInfo("BB Hawk Cancel Kill Animation");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(BaboonBirdAI.EnemyEnterRestModeClientRpc))]
        static void PostfixEnterRestMode(BaboonBirdAI __instance, bool sleep)
        {
            if (viewState.ContainsKey(__instance))
            {
                if (sleep && !viewState[__instance].sleeping)
                {
                    (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnSleep(__instance);
                    //EnemySkins.SkinLogger.LogInfo("BB Hawk Sleep");
                }
                else if (!sleep && !viewState[__instance].sitting)
                {
                    (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnSit(__instance);
                    //EnemySkins.SkinLogger.LogInfo("BB Hawk Sit");
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
                (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnGetUp(__instance);
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
                        (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnCalmDown(__instance);
                        EnemySkinRegistry.SkinLogger.LogInfo("BB Calm Down");
                        break;
                    case INTIMIDATE:
                        (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnIntimidate(__instance);
                        EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Intimidate");
                        break;
                    case ATTACK:
                        (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnEnterAttackMode(__instance);
                        EnemySkinRegistry.SkinLogger.LogInfo("BB Attack State");
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("GrabScrap")]
        static void PostfixGrabScrap(BaboonBirdAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnPickUpScrap(__instance, __instance.heldScrap);
            EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk pick up scrap");
        }

        [HarmonyPostfix]
        [HarmonyPatch("DropScrap")]
        static void PostfixDropScrap(BaboonBirdAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnDropScrap(__instance);
            EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk Drop Scrap");
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
                        (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BaboonHawkEventHandler)?.OnFinishKillPlayerAnimation(__instance);
                        EnemySkinRegistry.SkinLogger.LogInfo("BB Hawk End Kill");
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