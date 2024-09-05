using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections;
using UnityEngine;
namespace AntlerShed.SkinRegistry
{
    class SporeLizardPatch
    {
        internal const int ROAM = 0;
        internal const int AVOID = 1;
        internal const int ATTACK = 2;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PufferAI), "stompAnimation")]
        static void OnStomp(PufferAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnStomp(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard did stomp animation");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(PufferAI), "shakeTailAnimation")]
        static void OnShakeTail(PufferAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnShakeTail(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard rattled tail");
            __instance.StartCoroutine(shakeTailEventDispatcher(__instance));
        }

        private static IEnumerator shakeTailEventDispatcher(PufferAI instance)
        {
            yield return new WaitForSeconds(0.5f);
            EnemySkinRegistry.GetEnemyEventHandlers(instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnPuff(instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard did smoke puff");
            //yield return new WaitForSeconds(0.2f);
        }
    }
}