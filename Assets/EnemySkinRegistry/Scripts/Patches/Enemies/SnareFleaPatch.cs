using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class SnareFleaPatch
    {
        internal const int MOVING = 0;
        internal const int HIDING = 1;
        internal const int ATTACKING = 2;
        internal const int CLINGING = 3;

        private static Dictionary<CentipedeAI, SnareFleaViewState> viewState = new Dictionary<CentipedeAI, SnareFleaViewState>();

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CentipedeAI), nameof(CentipedeAI.Start))]
        static void PrefixStart(CentipedeAI __instance)
        {
            viewState.Add(__instance, new SnareFleaViewState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CentipedeAI), nameof(CentipedeAI.Update))]
        static void PrefixUpdate(CentipedeAI __instance, bool ___clingingToCeiling, bool ___startedCeilingAnimationCoroutine)
        {
            if (__instance.currentBehaviourStateIndex == ATTACKING && viewState.ContainsKey(__instance))
            {
                if (viewState[__instance].hasHitGroundFromCeiling == ___clingingToCeiling)
                {
                    if (!___clingingToCeiling)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnHitGroundFromCeiling(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea Hit Ground");
                    }
                }
                viewState[__instance].hasHitGroundFromCeiling = !___clingingToCeiling;
                if (viewState[__instance].hasStartedAttackMovement == ___startedCeilingAnimationCoroutine)
                {
                    if (!___startedCeilingAnimationCoroutine && !___clingingToCeiling)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnBeginAttackMovement(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea started attack");
                    }
                }
                viewState[__instance].hasStartedAttackMovement = !___startedCeilingAnimationCoroutine;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(CentipedeAI), "fallFromCeiling")]
        static void PrefixFallFromCeiling(CentipedeAI __instance)
        {
            if (viewState.ContainsKey(__instance))
            {
                viewState[__instance].hasHitGroundFromCeiling = false;
            }
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnFallFromCeiling(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea fell from the ceiling");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is CentipedeAI)
            {
                switch (stateIndex)
                {
                    case MOVING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnEnterMovingState(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea entered moving state");
                        break;
                    case HIDING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnClingToCeiling(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea clinging to ceiling");
                        break;
                    case ATTACKING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnFallFromCeiling(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea fell from ceiling");
                        break;
                    case CLINGING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnClingToPlayer(__instance as CentipedeAI, (__instance as CentipedeAI).clingingToPlayer));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea clinging to player");
                        break;
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is CentipedeAI && __instance != null && viewState.ContainsKey(__instance as CentipedeAI))
            {
                viewState.Remove(__instance as CentipedeAI);
            }
        }

        private class SnareFleaViewState
        {
            public bool hasHitGroundFromCeiling;
            public bool hasStartedAttackMovement;
        }
    }
}