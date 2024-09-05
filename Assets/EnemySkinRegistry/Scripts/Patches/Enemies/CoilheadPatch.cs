using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class CoilheadPatch
    {
        internal const int ROAMING = 0;
        internal const int CHASING = 1;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is SpringManAI)
            {
                switch (stateIndex)
                {
                    case ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CoilheadEventHandler)?.OnEnterRoamingState(__instance as SpringManAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Coilhead entered roaming state");
                        break;
                    case CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CoilheadEventHandler)?.OnEnterChasingState(__instance as SpringManAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Coilhead entered chasing state");
                        break;
                }
            }
        }
    }
}