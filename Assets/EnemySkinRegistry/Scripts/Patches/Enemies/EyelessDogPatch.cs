using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class EyelessDogPatch
    {
        internal const int CALM = 0;
        internal const int SUSPICIOUS = 1;
        internal const int CHASING = 2;
        internal const int LUNGING = 3;

        private static Dictionary<MouthDogAI, EyelessDogViewState> viewState = new Dictionary<MouthDogAI, EyelessDogViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MouthDogAI), nameof(MouthDogAI.Start))]
        static void PostfixStart(MouthDogAI __instance)
        {
            viewState.Add(__instance, new EyelessDogViewState());
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MouthDogAI), nameof(MouthDogAI.Update))]
        static void PrefixUpdate(MouthDogAI __instance, bool ___coweringOnFloor, bool ___coweringOnFloorDebounce, bool ___hasEnteredChaseModeFully)
        {
            if (EnemyPatch.FinishedSpawning(__instance))
            {
                if (!___coweringOnFloor)
                {
                    if (!___coweringOnFloorDebounce)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnExitCower(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog stopped cowering");
                    }
                }
                else
                {
                    if (___coweringOnFloorDebounce)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterCower(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog started cowering");
                    }
                }
            }
            if(__instance.currentBehaviourStateIndex == CHASING && viewState.ContainsKey(__instance) && viewState[__instance].prevEnteredChaseModeFully != ___hasEnteredChaseModeFully && ___hasEnteredChaseModeFully)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterChasingState(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog finished screaming and decided to try and kill something");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(MouthDogAI), nameof(MouthDogAI.Update))]
        static void PostfixUpdate(MouthDogAI __instance, bool ___hasEnteredChaseModeFully)
        {
            if(__instance.currentBehaviourStateIndex == CHASING && viewState.ContainsKey(__instance))
            {
                viewState[__instance].prevEnteredChaseModeFully = ___hasEnteredChaseModeFully;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MouthDogAI), "KillPlayer")]
        static void PrefixKillPlayer(MouthDogAI __instance, int playerId)
        {
            //vanilla blind dog will stop their kill animation if they're hit
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnKillPlayer(__instance, StartOfRound.Instance.allPlayerScripts[playerId]));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog killed player");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MouthDogAI), "TakeBodyInMouth")]
        static void PrefixTakeBodyInMouth(MouthDogAI __instance, DeadBodyInfo ___carryingBody)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnPickUpBody(__instance, ___carryingBody));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to chasing state");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(MouthDogAI), "DropCarriedBody")]
        static void PrefixDropCarriedBody(MouthDogAI __instance, DeadBodyInfo ___carryingBody)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnDropBody(__instance, ___carryingBody));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to chasing state");
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
        static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
        {
            if (stateIndex != __instance.currentBehaviourStateIndex && __instance is MouthDogAI)
            {
                switch (stateIndex)
                {
                    case CALM:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterCalmState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to calm state");
                        break;
                    case SUSPICIOUS:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterSuspiciousState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to suspicious state");
                        //EyelessDogPatch.UnsetSuspiciousToChasingFlag(__instance as MouthDogAI);
                        break;
                    case CHASING:
                        if (__instance.currentBehaviourStateIndex == SUSPICIOUS)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnChaseHowl(__instance as MouthDogAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to chasing state from suspicious state");
                        }
                        else
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterChasingState(__instance as MouthDogAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to chasing state");
                        }
                        //EyelessDogPatch.SetSuspiciousToChasingFlag(__instance as MouthDogAI);
                        break;
                    case LUNGING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterLungeState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to lunging state");
                        break;
                }
            }
        }

        /*[HarmonyPrefix]
        [HarmonyPatch("enterChaseMode")]
        static void PrefixEnterChaseMode(MouthDogAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnChaseHowl(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog heard something and is now mad");
        }

        [HarmonyPostfix]
        [HarmonyPatch("enterChaseMode")]
        static void PostfixEnterChaseMode(MouthDogAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterChasingState(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to chasing state");
        }*/

        /*internal static void SetSuspiciousToChasingFlag(MouthDogAI instance)
        {
            if (instance != null && viewState.ContainsKey(instance))
            {
                viewState[instance].suspiciousToChasingFlag = true;
            }
        }

        internal static void UnsetSuspiciousToChasingFlag(MouthDogAI instance)
        {
            if(instance != null && viewState.ContainsKey(instance))
            {
                viewState[instance].suspiciousToChasingFlag = false;
            }
        }

        internal static bool GetSuspiciousToChasingFlag(MouthDogAI instance)
        {
            if (instance != null && viewState.ContainsKey(instance))
            {
                return viewState[instance].suspiciousToChasingFlag;
            }
            return false;
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if(__instance is MouthDogAI && __instance != null && viewState.ContainsKey(__instance as MouthDogAI))
            {
                viewState.Remove(__instance as MouthDogAI);
            }
        }

        private class EyelessDogViewState
        {
            public bool prevEnteredChaseModeFully = false; //the value of the coroutine finish flage during the previous frame
            public bool suspiciousToChasingFlag = false; //indicates whether or not this is the first time the dog has entered the chasing state since going back to the suspicious state
        }

    }
}