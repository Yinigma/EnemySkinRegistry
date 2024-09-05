using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace AntlerShed.SkinRegistry
{
    class ForestKeeperPatch
    {
        internal const int ROAMING = 0;
        internal const int CHASING = 1;
        internal const int BURNING = 2;


        [HarmonyPrefix]
        [HarmonyPatch(typeof(ForestGiantAI), "EatPlayerAnimation")]
        static void PrefixEatPlayer(ForestGiantAI __instance, PlayerControllerB playerBeingEaten, Vector3 enemyPosition, int enemyYRot)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnGrabbedPlayer(__instance, playerBeingEaten, enemyPosition, enemyYRot));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant grabbed player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is ForestGiantAI)
            {
                switch (stateIndex)
                {
                    case ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredRomaingState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant Entered Roaming State");
                        break;
                    case CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredChasingState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant entered chasing state");
                        break;
                    case BURNING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredBurningState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant entered burning state");
                        break;
                }
            }
        }
    }
}