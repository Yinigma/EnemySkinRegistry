using HarmonyLib;
using AntlerShed.EnemySkinKit;
using AntlerShed.EnemySkinKit.Events;


namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(FlowermanAI))]
    class BrackenPatch
    {
        const int SNEAKING = 0;
        const int EVADING = 1;
        const int ENRAGED = 2;

        [HarmonyPrefix]
        [HarmonyPatch(nameof(FlowermanAI.FinishKillAnimation))]
        static void PrefixFinishKillAnimation(FlowermanAI __instance, bool carryingBody)
        { 
            if (carryingBody)
            {
                (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BrackenEventHandler)?.OnPickUpCorpse(__instance, __instance.bodyBeingCarried);
                EnemySkinRegistry.SkinLogger.LogInfo("Bracken Pickup Corpse");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch("DropPlayerBody")]
        static void PostfixDropPlayerBody(FlowermanAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BrackenEventHandler)?.OnDropCorpse(__instance);
            if(EnemySkinRegistry.Debug) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Drop Corpse");
        }

        [HarmonyPostfix]
        [HarmonyPatch("killAnimation")]
        static void PostFixKillPlayer(FlowermanAI __instance)
        {
            (EnemySkinRegistry.GetEnemyEventHandler(__instance) as BrackenEventHandler)?.OnSnapPlayerNeck(__instance, __instance.inSpecialAnimationWithPlayer);
            if (EnemySkinRegistry.Debug) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Kill");
        }
    }
}
