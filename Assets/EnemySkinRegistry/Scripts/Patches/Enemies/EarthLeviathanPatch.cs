using AntlerShed.SkinRegistry.Events;
using HarmonyLib;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class EarthLeviathanPatch
    {
        private static Dictionary<SandWormAI, EarthLeviathanViewState> viewState = new Dictionary<SandWormAI, EarthLeviathanViewState>();


        [HarmonyPostfix]
        [HarmonyPatch(typeof(SandWormAI), nameof(SandWormAI.Start))]
        static void PostfixStart(SandWormAI __instance)
        {
            viewState.Add(__instance, new EarthLeviathanViewState());
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(SandWormAI), nameof(SandWormAI.Update))]
        static void PostfixUpdate(SandWormAI __instance)
        {
            if(__instance != null && viewState.ContainsKey(__instance))
            {
                if (viewState[__instance].lastEmergedState != __instance.emerged)
                {
                    if(__instance.emerged)
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EarthLeviathanEventHandler)?.OnEmergeFromGround(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Earth Leviathan emerged from underground");
                    }
                    else
                    {
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EarthLeviathanEventHandler)?.OnSubmergeIntoGround(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Earth Leviathan went back underground");
                    }
                }
                EarthLeviathanViewState state = viewState[__instance];
                state.lastEmergedState = __instance.emerged;
                viewState[__instance] = state;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is SandWormAI && __instance != null && viewState.ContainsKey(__instance as SandWormAI))
            {
                viewState.Remove(__instance as SandWormAI);
            }
        }
    }

    struct EarthLeviathanViewState
    {
        public bool lastEmergedState;
    }

}