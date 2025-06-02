using HarmonyLib;
using System.Collections.Generic;

namespace AntlerShed.SkinRegistry
{
    class GiantKiwiPatch
    {

        private static Dictionary<GiantKiwiAI, GiantKiwiViewState> viewState = new Dictionary<GiantKiwiAI, GiantKiwiViewState>();

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GiantKiwiAI), nameof(GiantKiwiAI.Start))]
        static void PostfixStart(GiantKiwiAI __instance)
        {
            viewState[__instance] = new GiantKiwiViewState();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            if (__instance is GiantKiwiAI && __instance != null && viewState.ContainsKey(__instance as GiantKiwiAI))
            {
                RemoveViewState(__instance as GiantKiwiAI);
            }
        }

        internal static void RemoveViewState(GiantKiwiAI __instance)
        {
            viewState.Remove(__instance);
        }

        private class GiantKiwiViewState
        {
            public int aggressionState;
            public bool sleeping;
        }
    }
}