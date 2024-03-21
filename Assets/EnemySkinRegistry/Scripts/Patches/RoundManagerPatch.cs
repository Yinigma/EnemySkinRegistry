using HarmonyLib;

namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch(typeof(RoundManager))]
    class RoundManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("Awake")]
        static void PostfixOnAwake(RoundManager __instance)
        {
            EnemySkinRegistry.InitConfig();
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(RoundManager.LoadNewLevel))]
        static void PrefixOnLoad(RoundManager __instance, int randomSeed, SelectableLevel newLevel)
        {
            EnemySkinRegistry.UpdateRoundInfo(randomSeed, newLevel.PlanetName);
        }
    }
}


