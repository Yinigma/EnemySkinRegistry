using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{

    class TulipSnakePatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), "SetFlappingLocalClient")]
        static void PostfixSetFlap(FlowerSnakeEnemy __instance, bool setFlapping)
        {
            if(setFlapping)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnStartedFlapping(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake started flapping");
            }
            else
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnStoppedFlapping(__instance));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake stopped flapping");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), "StartLeapOnLocalClient")]
        static void PostfixStartLeap(FlowerSnakeEnemy __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnStartLeap(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake started leap");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), "StopLeapOnLocalClient")]
        static void PostfixStopLeap(FlowerSnakeEnemy __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnStopLeap(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake ended leap");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), "SetClingToPlayer")]
        static void PostfixStartCling(FlowerSnakeEnemy __instance, PlayerControllerB playerToCling, int setClingPosition)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnClingToPlayer(__instance, playerToCling, setClingPosition));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake started cling");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), "StopClingingOnLocalClient")]
        static void PostfixStopCling(FlowerSnakeEnemy __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnStopCling(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake ended cling");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(FlowerSnakeEnemy), nameof(FlowerSnakeEnemy.MakeChuckleClientRpc))]
        static void PostfixChuckle(FlowerSnakeEnemy __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as TulipSnakeEventHandler)?.OnChuckle(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Tulip Snake chuckled");
        }
    }
}