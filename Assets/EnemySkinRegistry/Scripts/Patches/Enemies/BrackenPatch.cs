using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{
    class BrackenPatch
    {
        internal const int SNEAKING = 0;
        internal const int EVADING = 1;
        internal const int ENRAGED = 2;

        private static Dictionary<FlowermanAI, BrackenViewState> viewState = new Dictionary<FlowermanAI, BrackenViewState>();

        /*[HarmonyPrefix]
        [HarmonyPatch(nameof(FlowermanAI.FinishKillAnimation))]
        static void PrefixFinishKillAnimation(FlowermanAI __instance, bool carryingBody)
        { 
            if(carryingBody)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler)=>(handler as BrackenEventHandler)?.OnPickUpCorpse(__instance, __instance.bodyBeingCarried));
                if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Pickup Corpse");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DropPlayerBody")]
        static void PostfixDropPlayerBody(FlowermanAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnDropCorpse(__instance));
            if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Drop Corpse");
        }*/

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex)
            {
                if (__instance is FlowermanAI)
                {
                    switch (stateIndex)
                    {
                        case SNEAKING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnSneakStateEntered(__instance as FlowermanAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Sneak");
                            break;
                        case EVADING:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnEvadeStateEntered(__instance as FlowermanAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Evade");
                            break;
                        case ENRAGED:
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnEnragedStateEntered(__instance as FlowermanAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Enrage");
                            break;
                    }
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowermanAI), "killAnimation")]
        static void PostFixKillPlayer(FlowermanAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnSnapPlayerNeck(__instance, __instance.inSpecialAnimationWithPlayer));
            if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Kill");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowermanAI), nameof(FlowermanAI.Update))]
        static void PostFixUpdate(FlowermanAI __instance)
        {
            if(viewState.ContainsKey(__instance))
            {
                if (__instance.carryingPlayerBody != viewState[__instance].carryingBody)
                {
                    if(__instance.carryingPlayerBody)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnPickUpCorpse(__instance, __instance.bodyBeingCarried));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Pickup Corpse");
                    }
                    else
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnDropCorpse(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Drop Corpse");
                    }
                }
                viewState[__instance].carryingBody = __instance.carryingPlayerBody;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is FlowermanAI && __instance != null && viewState.ContainsKey(__instance as FlowermanAI))
            {
                viewState.Remove(__instance as FlowermanAI);
            }
        }
        
        private class BrackenViewState
        {
            public bool carryingBody;
        }
    }
}
