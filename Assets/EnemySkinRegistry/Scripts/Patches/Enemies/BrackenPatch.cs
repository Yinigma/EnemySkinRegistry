using AntlerShed.SkinRegistry.Events;
using HarmonyLib;


namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(FlowermanAI))]
    class BrackenPatch
    {
        internal const int SNEAKING = 0;
        internal const int EVADING = 1;
        internal const int ENRAGED = 2;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FlowermanAI.FinishKillAnimation))]
        static void PrefixFinishKillAnimation(FlowermanAI __instance, bool carryingBody)
        { 
            if (carryingBody)
            {
                EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler)=>(handler as BrackenEventHandler)?.OnPickUpCorpse(__instance, __instance.bodyBeingCarried));
                if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Pickup Corpse");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DropPlayerBody")]
        static void PostfixDropPlayerBody(FlowermanAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnDropCorpse(__instance));
            if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Drop Corpse");
        }

        [HarmonyPostfix]
        [HarmonyPatch("killAnimation")]
        static void PostFixKillPlayer(FlowermanAI __instance)
        {
            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BrackenEventHandler)?.OnSnapPlayerNeck(__instance, __instance.inSpecialAnimationWithPlayer));
            if(EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Kill");
        }
    }
}
