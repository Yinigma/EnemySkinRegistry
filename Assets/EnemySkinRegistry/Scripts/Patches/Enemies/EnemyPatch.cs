using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(EnemyAI))]
    class EnemyPatch
    {
        const int SNEAKING = 0;
        const int EVADING = 1;
        const int ENRAGED = 2;

        static Dictionary<EnemyAI, EnemyViewState> viewState = new Dictionary<EnemyAI, EnemyViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.Start))]
        static void PostfixStart(EnemyAI __instance)
        {
            //This block should only run for vanilla enemies.
            //I would've done this in each individual enemy's start block, but harmony projectile vomits
            //at you if you try and patch a base class method that isn't overridden in the target class you're patching, so that's cool.
            //For modded enemies, it's up to the author of the modded enemy to add this logic in their enemy's start method, or wherever it's most appropriate.
            //It's your mod, don't let me tell you what to do.

            string skinnableEnemyId = EnemySkinRegistry.VanillaIdFromInstance(__instance);
            if (skinnableEnemyId != null)
            {
                viewState[__instance] = new EnemyViewState();
                viewState[__instance].ventAnimationFinished = __instance.ventAnimationFinished;
                Skin randomSkin = EnemySkinRegistry.PickSkin(skinnableEnemyId);
                EnemySkinRegistry.ApplySkin(randomSkin, skinnableEnemyId, __instance.gameObject);
                List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
                handler.ForEach((handler)=>handler.OnSpawn(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy start");
            }
        }


        [HarmonyPrefix]
        [HarmonyPatch(nameof(EnemyAI.SwitchToBehaviourState))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            List<EnemyEventHandler> handlers = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            if (__instance is FlowermanAI)
            {
                switch (stateIndex)
                {
                    case SNEAKING:
                        handlers.ForEach((handler) => (handler as BrackenEventHandler)?.OnSneakStateEntered(__instance as FlowermanAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Sneak");
                        break;
                    case EVADING:
                        handlers.ForEach((handler) => (handler as BrackenEventHandler)?.OnEvadeStateEntered(__instance as FlowermanAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Evade");
                        break;
                    case ENRAGED:
                        handlers.ForEach((handler)=> (handler as BrackenEventHandler)?.OnEnragedStateEntered(__instance as FlowermanAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Enrage");
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.SetEnemyStunned))]
        static void PostfixStunned(EnemyAI __instance, bool setToStunned)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            if (!__instance.isEnemyDead && __instance.enemyType.canBeStunned && setToStunned)
            {
                handler.ForEach((hnd) => hnd?.OnStun(__instance, __instance.stunnedByPlayer));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy stunned");
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.KillEnemy))]
        static void PostfixKilled(EnemyAI __instance)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnKilled(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy killed");
        }

        [HarmonyPostfix]
        [HarmonyPatch("OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnDestroy(__instance));
            viewState.Remove(__instance);
            if(__instance is BaboonBirdAI)
            {
                BaboonHawkPatch.RemoveViewState(__instance as BaboonBirdAI);
            }
            EnemySkinRegistry.RemoveSkinner(__instance.gameObject);
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("On Destroy called on enemy.");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.HitEnemy))]
        static void PostfixHit(EnemyAI __instance, PlayerControllerB playerWhoHit)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnHit(__instance, playerWhoHit));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Something Hit Enemy");
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.Update))]
        static void PostfixUpdate(EnemyAI __instance)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd)=>hnd?.Update(__instance));
            if (viewState.ContainsKey(__instance) && !viewState[__instance].ventAnimationFinished && __instance.ventAnimationFinished)
            {
                viewState[__instance].ventAnimationFinished = true;
                handler.ForEach((hnd) => hnd?.OnSpawnFinished(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy spawn finished");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(nameof(EnemyAI.EnableEnemyMesh))]
        static void PostFixEnableMeshes(EnemyAI __instance, bool enable)
        {
            List<EnemyEventHandler> handlers = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            if(__instance is DressGirlAI)
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

        private class EnemyViewState
        {
            public bool ventAnimationFinished;
        }
    }
}