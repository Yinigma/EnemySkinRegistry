using AntlerShed.SkinRegistry.Events;
using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    class ManticoilPatch
    {
        public const int FLYING = 1;
        [HarmonyPrefix]
        [HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.Update))]
        static void PrefixUpdate(DoublewingAI __instance, int ___behaviourStateLastFrame)
        {
            if(__instance.currentBehaviourStateIndex == FLYING)
            {
                if (___behaviourStateLastFrame != __instance.currentBehaviourStateIndex)
                {
                    EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ManticoilEventHandler)?.OnTakeOff(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Manticoil take off");
                }
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DoublewingAI), nameof(DoublewingAI.AnimationEventB))]
        static void PostfixLand(DoublewingAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ManticoilEventHandler)?.OnLand(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Manticoil land");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DoublewingAI), "BirdScreech")]
        static void PostfixScreech(DoublewingAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ManticoilEventHandler)?.OnScreech(__instance));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Manticoil screech");
        }
    }
}