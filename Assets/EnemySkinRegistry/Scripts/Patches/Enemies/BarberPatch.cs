using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{

    class BarberPatch
    {
        private static Dictionary<ClaySurgeonAI, BarberViewState> viewState = new Dictionary<ClaySurgeonAI, BarberViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClaySurgeonAI), nameof(ClaySurgeonAI.Update))]
        static void PostFixUpdate(ClaySurgeonAI __instance)
        {
            if (viewState.ContainsKey(__instance))
            {
                if (__instance.isJumping != viewState[__instance].wasJumping)
                {
                    if (__instance.isJumping)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BarberEventHandler)?.OnStartJump(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Barber started jump");
                    }
                    else
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BarberEventHandler)?.OnStopJump(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Barber stopped jump");
                    }
                }
                viewState[__instance].wasJumping = __instance.isJumping;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(ClaySurgeonAI), nameof(ClaySurgeonAI.KillPlayerClientRpc))]
        static void PostfixCollideWithPlayer(ClaySurgeonAI __instance)
        {

        }

        private class BarberViewState
        {
            public bool wasJumping;
        }
    }
}