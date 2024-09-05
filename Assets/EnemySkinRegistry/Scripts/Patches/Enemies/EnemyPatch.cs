using AntlerShed.SkinRegistry.Events;
using GameNetcodeStuff;
using HarmonyLib;
using System.Collections.Generic;


namespace AntlerShed.SkinRegistry
{
    [HarmonyPatch()]
    class EnemyPatch
    {
        const int SNEAKING = 0;
        const int EVADING = 1;
        const int ENRAGED = 2;

        static Dictionary<EnemyAI, EnemyViewState> viewState = new Dictionary<EnemyAI, EnemyViewState>();

        private static uint hash(uint a)
        {
            a = (a + 0x7ed55d16) + (a << 12);
            a = (a ^ 0xc761c23c) ^ (a >> 19);
            a = (a + 0x165667b1) + (a << 5);
            a = (a + 0xd3a2646c) ^ (a << 9);
            a = (a + 0xfd7046c5) + (a << 3);
            a = (a ^ 0xb55a4f09) ^ (a >> 16);
            return a;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Start))]
        [HarmonyAfter("AudioKnight.StarlancerAIFix")]
        static void PostfixStart(EnemyAI __instance)
        {
            //This block should only run for vanilla enemies.
            //For modded enemies, it's up to the author of the modded enemy to add this logic in their enemy's start method, or wherever it's most appropriate.
            //It's your mod, don't let me tell you what to do.

            string skinnableEnemyId = EnemySkinRegistry.VanillaIdFromInstance(__instance);
            if (skinnableEnemyId != null)
            {
                viewState[__instance] = new EnemyViewState();
                viewState[__instance].ventAnimationFinished = __instance.ventAnimationFinished;
                uint seed = hash((uint)__instance.NetworkObjectId);
                Skin randomSkin = EnemySkinRegistry.SelectSpawnSkin(__instance.gameObject, skinnableEnemyId, __instance.isOutside ? SpawnLocation.OUTDOOR : SpawnLocation.INDOOR, (seed%4096)/4096.0f);
                EnemySkinRegistry.ApplySkin(randomSkin, skinnableEnemyId, __instance.gameObject);
                List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
                handler.ForEach((handler)=>handler.OnSpawn(__instance));
                //if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"Assigned \"{randomSkin.Label}\" skin to {__instance.enemyType.enemyName} instance spawned {(__instance.isOutside ? "outdoors" : "indoors")}");
            }
        }

        static int lastLevelSeed = -1;

        [HarmonyPostfix]
        [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncNestSpawnPositionsClientRpc))]
        static void PostfixSyncNestObjects(RoundManager __instance)
        {
            if(StartOfRound.Instance.randomMapSeed != lastLevelSeed)
            {
                lastLevelSeed = StartOfRound.Instance.randomMapSeed;
                uint i = 1;
                foreach(EnemyAINestSpawnObject nest in __instance.enemyNestSpawnObjects)
                {
                    nest.gameObject.AddComponent<NestTracker>();
                    string skinnableEnemyId = EnemySkinRegistry.VanillaIdFromInstance(nest.enemyType.enemyPrefab.GetComponent<EnemyAI>());
                    if (skinnableEnemyId != null)
                    {
                        uint seed = hash(i * (uint)StartOfRound.Instance.randomMapSeed);
                        Skin randomSkin = EnemySkinRegistry.PickSkinAtValue(skinnableEnemyId, SpawnLocation.OUTDOOR, (seed%4096)/4096.0f);
                        EnemySkinRegistry.ApplyNestSkin(randomSkin?.Id, skinnableEnemyId, nest);
                    }
                    i++;
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.UseNestSpawnObject))]
        static void PrefixUseNest(EnemyAI __instance, EnemyAINestSpawnObject nestSpawnObject)
        {
            EnemySkinRegistry.StageSkin(__instance.gameObject, nestSpawnObject);
        }


            /*[HarmonyPrefix]
            [HarmonyPatch(nameof(EnemyAI.SwitchToBehaviourStateOnLocalClient))]
            static void PrefixSwitchBehavior(EnemyAI __instance, int stateIndex)
            {
                if (stateIndex != __instance.currentBehaviourStateIndex)
                {
                    List<EnemyEventHandler> handlers = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
                    /*if (__instance is FlowermanAI)
                    {
                        switch (stateIndex)
                        {
                            case SNEAKING:
                                handlers.ForEach((handler) => (handler as BrackenEventHandler)?.OnSneakStateEntered(__instance as FlowermanAI));
                                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Sneak");
                                break;
                            case EVADING:
                                handlers.ForEach((handler) => (handler as BrackenEventHandler)?.OnEvadeStateEntered(__instance as FlowermanAI));
                                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Evade");
                                break;
                            case ENRAGED:
                                handlers.ForEach((handler) => (handler as BrackenEventHandler)?.OnEnragedStateEntered(__instance as FlowermanAI));
                                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Bracken Switch To Enrage");
                                break;
                        }
                    }*/
            /*else if (__instance is SandSpiderAI)
            {
                switch (stateIndex)
                {
                    case BunkerSpiderPatch.WEB_PLACING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterWebbingState(__instance as SandSpiderAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider Enter Webbing State");
                        break;
                    case BunkerSpiderPatch.WAITING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterWaitingState(__instance as SandSpiderAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"Spider entered waiting state");
                        break;
                    case BunkerSpiderPatch.CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as BunkerSpiderEventHandler)?.OnEnterChasingState(__instance as SandSpiderAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spider entered chasing state");
                        break;
                }
            }*/
            /*else if (__instance is SpringManAI)
            {
                switch (stateIndex)
                {
                    case CoilheadPatch.ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CoilheadEventHandler)?.OnEnterRoamingState(__instance as SpringManAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Coilhead entered roaming state");
                        break;
                    case CoilheadPatch.CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as CoilheadEventHandler)?.OnEnterChasingState(__instance as SpringManAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Coilhead entered chasing state");
                        break;
                }
            }*/
            /*else if (__instance is MouthDogAI)
            {
                switch (stateIndex)
                {
                    case EyelessDogPatch.CALM:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterCalmState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to calm state");
                        break;
                    case EyelessDogPatch.SUSPICIOUS:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterSuspiciousState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to suspicious state");
                        //EyelessDogPatch.UnsetSuspiciousToChasingFlag(__instance as MouthDogAI);
                        break;
                    case EyelessDogPatch.CHASING:
                        if (__instance.currentBehaviourStateIndex == EyelessDogPatch.SUSPICIOUS)
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
                    case EyelessDogPatch.LUNGING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as EyelessDogEventHandler)?.OnEnterLungeState(__instance as MouthDogAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Dog switched to lunging state");
                        break;
                }
            }*/
            /*else if (__instance is ForestGiantAI)
            {
                switch (stateIndex)
                {
                    case ForestKeeperPatch.ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredRomaingState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant Entered Roaming State");
                        break;
                    case ForestKeeperPatch.CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredChasingState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant entered chasing state");
                        break;
                    case ForestKeeperPatch.BURNING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ForestKeeperEventHandler)?.OnEnteredBurningState(__instance as ForestGiantAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Giant entered burning state");
                        break;
                }
            }*/
            /*else if (__instance is JesterAI)
            {
                switch (stateIndex)
                {
                    case JesterPatch.ROAMING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterRoamingState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered roaming state");
                        break;
                    case JesterPatch.CRANKING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterCrankingState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered cranking state");
                        break;
                    case JesterPatch.POPPED:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as JesterEventHandler)?.OnEnterPoppedState(__instance as JesterAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Jester entered popped state");
                        break;
                }
            }*/
            /*else if (__instance is NutcrackerEnemyAI)
            {
                switch (stateIndex)
                {
                    case NutcrackerPatch.PATROL:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterPatrolState(__instance as NutcrackerEnemyAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker entered patrol state");
                        break;
                    case NutcrackerPatch.INSPECT:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterInspectState(__instance as NutcrackerEnemyAI, __instance.currentBehaviourStateIndex != NutcrackerPatch.ATTACK));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"Nutcracker entered inspect state{(__instance.currentBehaviourStateIndex != NutcrackerPatch.ATTACK ? " and popped its head out." : ".")}");
                        break;
                    case NutcrackerPatch.ATTACK:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as NutcrackerEventHandler)?.OnEnterAttackState(__instance as NutcrackerEnemyAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Nutcracker entered attack state");
                        break;
                }
            }*/
            /*else if (__instance is CentipedeAI)
            {
                switch (stateIndex)
                {
                    case SnareFleaPatch.MOVING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnEnterMovingState(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea entered moving state");
                        break;
                    case SnareFleaPatch.HIDING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnClingToCeiling(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea clinging to ceiling");
                        break;
                    case SnareFleaPatch.ATTACKING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnFallFromCeiling(__instance as CentipedeAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea fell from ceiling");
                        break;
                    case SnareFleaPatch.CLINGING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SnareFleaEventHandler)?.OnClingToPlayer(__instance as CentipedeAI, (__instance as CentipedeAI).clingingToPlayer));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Snare Flea clinging to player");
                        break;
                }
            }*/
            /*else if (__instance is PufferAI)
            {
                switch (stateIndex)
                {
                    case SporeLizardPatch.ROAM:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnEnterRoamingState(__instance as PufferAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard entered roaming state");
                        break;
                    case SporeLizardPatch.AVOID:
                        if (__instance.currentBehaviourStateIndex == SporeLizardPatch.ROAM)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnAlarmed(__instance as PufferAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard alarmed");
                        }
                        else
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnEnterAvoidState(__instance as PufferAI));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore Lizard entered avoid state");
                        }

                        break;
                    case SporeLizardPatch.ATTACK:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as SporeLizardEventHandler)?.OnEnterAttackState(__instance as PufferAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Spore lizard entered attack state");
                        break;
                }
            }*/
            /*else if (__instance is CrawlerAI)
            {
                switch (stateIndex)
                {
                    case ThumperPatch.SEARCHING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnEnterSearchState(__instance as CrawlerAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper entered search state");
                        break;
                    case ThumperPatch.CHASING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ThumperEventHandler)?.OnEnterChaseState(__instance as CrawlerAI));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Thumper entered chasing state");
                        break;
                }
            }*/
            /*else if (__instance is RadMechAI)
            {
                switch (stateIndex)
                {
                    case OldBirdPatch.ROAMING:
                        if(!__instance.inSpecialAnimation)
                        {
                            EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as OldBirdEventHandler)?.OnAlertEnded(__instance));
                            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Old bird lowered alert");
                        }
                        break;
                }
            }*/
            /*else if (__instance is ButlerEnemyAI)
            {
                switch (stateIndex)
                {
                    case ButlerPatch.SWEEPING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterSweepingState(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered sweeping state");
                        break;
                    case ButlerPatch.PREMEDITATING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterPremeditatingState(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered premeditating state");
                        break;
                    case ButlerPatch.MURDERING:
                        EnemySkinRegistry.GetEnemyEventHandlers(__instance).ForEach((handler) => (handler as ButlerEventHandler)?.OnEnterMurderingState(__instance));
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Butler entered murdering state");
                        break;
                }
            }
        }

    }*/

            [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.SetEnemyStunned))]
        static void PostfixStunned(EnemyAI __instance, bool setToStunned)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            if (!__instance.isEnemyDead && __instance.enemyType.canBeStunned && setToStunned)
            {
                handler.ForEach((hnd) => hnd?.OnStun(__instance, __instance.stunnedByPlayer));
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy stunned");
            }

        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.KillEnemy))]
        static void PostfixKilled(EnemyAI __instance, bool destroy)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnKilled(__instance));
            handler.ForEach((hnd) => hnd?.OnKilled(__instance, destroy));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy killed");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), "OnDestroy")]
        static void PostfixDestroyed(EnemyAI __instance)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnEnemyDestroyed(__instance));
            viewState.Remove(__instance);
            EnemySkinRegistry.RemoveSkinner(__instance.gameObject);
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("On Destroy called on enemy.");
        }

        /*[HarmonyPostfix]
        [HarmonyPatch(typeof(UnityEngine.Object), nameof(UnityEngine.Object.Destroy))]
        static void PostfixNestDestroyed(UnityEngine.Object obj, float t)
        {
            EnemySkinRegistry.SkinLogger?.LogError("Destroying object");
            if(obj is EnemyAINestSpawnObject)
            {
                EnemySkinRegistry.SkinLogger?.LogError("Object was nest");
                EnemySkinRegistry.RemoveSkinner((obj as EnemyAINestSpawnObject).gameObject);
            }
            if (obj is GameObject)
            {
                EnemyAINestSpawnObject nest = (obj as GameObject).GetComponent<EnemyAINestSpawnObject>();
                if(nest != null)
                {
                    EnemySkinRegistry.SkinLogger?.LogError("object was nest gameobject");
                    EnemySkinRegistry.RemoveSkinner(obj as GameObject);
                }
            }
        }*/

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.HitEnemy))]
        static void PostfixHit(EnemyAI __instance, PlayerControllerB playerWhoHit, bool playHitSFX)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd) => hnd?.OnHit(__instance, playerWhoHit, playHitSFX));
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Something Hit Enemy");
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Update))]
        static void PostfixUpdate(EnemyAI __instance)
        {
            List<EnemyEventHandler> handler = EnemySkinRegistry.GetEnemyEventHandlers(__instance);
            handler.ForEach((hnd)=>hnd?.OnEnemyUpdate(__instance));
            if (viewState.ContainsKey(__instance) )
            {
                if (!viewState[__instance].ventAnimationFinished && __instance.ventAnimationFinished)
                {
                    viewState[__instance].ventAnimationFinished = true;
                    handler.ForEach((hnd) => hnd?.OnSpawnFinished(__instance));
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Enemy spawn finished");
                }
            }
        }

        internal static bool FinishedSpawning(EnemyAI instance)
        {
            if (instance != null && viewState.ContainsKey(instance))
            {
                return viewState[instance].ventAnimationFinished;
            }
            return false;
        }

        private class EnemyViewState
        {
            public bool ventAnimationFinished = false;
        }
    }
}