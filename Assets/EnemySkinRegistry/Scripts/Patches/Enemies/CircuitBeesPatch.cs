using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class CircuitBeesPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.BeesZap))]
        static void PostfixBeesZap(RedLocustBees __instance, int ___beesZappingMode)
        {
            if (___beesZappingMode != 3)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CircuitBeeEventHandler)?.OnZapAudioCue(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Circuit Bees zap");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RedLocustBees), "BeesZapOnTimer")]
        static void PrefixZapTimer(RedLocustBees __instance, int ___beesZappingMode, float ___beesZapTimer, float ___beesZapCurrentTimer, float ___attackZapModeTimer, float ___mostOptimalDistance)
        {
            if (___beesZapCurrentTimer > ___beesZapTimer)
            {
                ___beesZapCurrentTimer = 0f;
                if (___beesZappingMode == 3)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CircuitBeeEventHandler)?.OnZapAudioStart(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Circuit Bees start zap audio");

                    
                    if (___attackZapModeTimer > 3f)
                    {
                        ___attackZapModeTimer = 0f;
                        if (___mostOptimalDistance > 3f)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CircuitBeeEventHandler)?.OnZapAudioStop(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Circuit Bees stop zap");
                        }
                    }
                }
            }
        }


        [HarmonyPostfix]
        [HarmonyPatch(typeof(RedLocustBees), "ResetBeeZapTimer")]
        static void PostfixStopZapTimer(RedLocustBees __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CircuitBeeEventHandler)?.OnZapAudioStop(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Circuit Bees reset zap timer");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.DaytimeEnemyLeave))]
        static void PostfixLeave(RedLocustBees __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CircuitBeeEventHandler)?.OnLeaveLevel(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Circuit Bees Leaving Level");
        }
    }
}