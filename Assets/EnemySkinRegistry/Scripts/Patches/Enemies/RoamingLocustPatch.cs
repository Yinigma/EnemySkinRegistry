using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class RoamingLocustPatch
    {
        public const int DISPERSED = 1;
        public const int GATHERED = 0;

        [HarmonyPrefix]
        [HarmonyPatch(typeof(DocileLocustBeesAI), nameof(DocileLocustBeesAI.Update))]
        static void PrefixUpdate(DocileLocustBeesAI __instance, float ___timeSinceReturning)
        {
            
            if (__instance.previousBehaviourStateIndex != __instance.currentBehaviourStateIndex)
            {
                switch (__instance.currentBehaviourStateIndex)
                {
                    case GATHERED:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as RoamingLocustEventHandler)?.OnGather(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Roaming Locusts gathered");
                        break;
                    case DISPERSED:
                        if (___timeSinceReturning > 2f)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as RoamingLocustEventHandler)?.OnDisperse(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Roaming Locusts dispersed");
                        }
                        break;
                }

            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DocileLocustBeesAI), nameof(DocileLocustBeesAI.DaytimeEnemyLeave))]
        static void PostfixLeave(DocileLocustBeesAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as RoamingLocustEventHandler)?.OnDisperse(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Roaming Locusts dispersed");
        }
    }
}