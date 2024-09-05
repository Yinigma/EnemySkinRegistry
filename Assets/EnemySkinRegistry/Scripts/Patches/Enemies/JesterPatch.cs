using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class JesterPatch
    {
        internal const int ROAMING = 0;
        internal const int CRANKING = 1;
        internal const int POPPED = 2;

        //kill player
        [HarmonyPrefix]
        [HarmonyPatch(typeof(JesterAI), "killPlayerAnimation")]
        static void OnKillPlayer(JesterAI __instance, int playerId)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnKillPlayer(__instance, StartOfRound.Instance.allPlayerScripts[playerId]));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester killed player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is JesterAI)
            {
                switch (stateIndex)
                {
                    case ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterRoamingState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered roaming state");
                        break;
                    case CRANKING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterCrankingState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered cranking state");
                        break;
                    case POPPED:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterPoppedState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered popped state");
                        break;
                }
            }
        }
    }
}