using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{
    class KidnapperFoxPatch
    {
        private static Dictionary<BushWolfEnemy, FoxViewState> viewState = new Dictionary<BushWolfEnemy, FoxViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BushWolfEnemy), "CancelReelingPlayerIn")]
        static void PostfixCancelReel(BushWolfEnemy __instance, int ___previousState, bool ___dragging)
        {
            if (___previousState == 2 || ___previousState == 1 || __instance.currentBehaviourStateIndex == 2)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as KidnapperFoxEventHandler)?.OnCancelReelingPlayer(__instance, ___dragging));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Kidnapper Fox stopped pulling in player");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BushWolfEnemy), "HitTongueLocalClient")]
        static void PostfixHitTongue(BushWolfEnemy __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as KidnapperFoxEventHandler)?.OnTongueHit(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Kidnapper Fox had its tongue hit");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(BushWolfEnemy), nameof(BushWolfEnemy.Update))]
        static void PostfixUpdate(BushWolfEnemy __instance, bool ___startedShootingTongue, bool ___dragging)
        {
            if (viewState.ContainsKey(__instance))
            {
                
                if (___startedShootingTongue != viewState[__instance].prevShootTongueFlag)
                {
                    if (___startedShootingTongue)
                    {
                        if (___dragging)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as KidnapperFoxEventHandler)?.OnLandedTongueShot(__instance, __instance.draggingPlayer));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Kidnapper Fox landed tongue shot");
                        }
                    }
                    else
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as KidnapperFoxEventHandler)?.OnTongueShot(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Kidnapper Fox shot tongue");
                    }
                }
                viewState[__instance].prevShootTongueFlag = ___startedShootingTongue;
            }
        }

        private class FoxViewState
        {
            public bool prevShootTongueFlag;
        }
    }
}
