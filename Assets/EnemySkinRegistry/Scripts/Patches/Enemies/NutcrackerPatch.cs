using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class NutcrackerPatch
    {
        internal const int PATROL = 0;
        internal const int INSPECT = 1;
        internal const int ATTACK = 2;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "ReloadGun")]
        static void PrefixReload(NutcrackerEnemyAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnReloadShotgun(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker Reload Shotgun");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "StopReloading")]
        static void PrefixCancelReload(NutcrackerEnemyAI __instance, bool ___reloadingGun)
        {
            if(___reloadingGun)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnReloadStopped(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker Stop Reload Animation");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "FireGun")]
        static void PrefixFireGun(NutcrackerEnemyAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnFireShotgun(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker shot his gun (but he don't know what it means *drum fill*)");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(NutcrackerEnemyAI), "LegKickPlayer")]
        static void PrefixKickPlayer(NutcrackerEnemyAI __instance, int playerId)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnKickPlayer(__instance, StartOfRound.Instance.allPlayerScripts[playerId]));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker kicked player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is NutcrackerEnemyAI)
            {
                switch (stateIndex)
                {
                    case PATROL:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterPatrolState(__instance as NutcrackerEnemyAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker entered patrol state");
                        break;
                    case INSPECT:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterInspectState(__instance as NutcrackerEnemyAI, __instance.currentBehaviourStateIndex != NutcrackerPatch.ATTACK));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"Nutcracker entered inspect state{(__instance.currentBehaviourStateIndex != NutcrackerPatch.ATTACK ? " and popped its head out." : ".")}");
                        break;
                    case ATTACK:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterAttackState(__instance as NutcrackerEnemyAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker entered attack state");
                        break;
                }
            }
        }

        /*
        /// <summary>
        /// Called when the nutcracker fires its shotgun
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnFireShotgun(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker does its kick attack on a player
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnKickPlayer(NutcrackerEnemyAI nutcracker, PlayerControllerB player) { }
        */
    }
}