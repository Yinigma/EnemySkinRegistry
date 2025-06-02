using GameNetcodeStuff;
using UnityEngine;

namespace AntlerShed.SkinRegistry.Events
{
    //All of these events are not supposed to be networked, but are supposed to trigger regardless of the game being a host or a client.
    //If this isn't the case for any of these, then that's a bug, and I'd sppreciate it being reported. I don't really know all the nuances of lethal company's netcode.
    //I just remember enough about working with unet to know it sucks.

    public interface EnemyEventHandler
    {
        /// <summary>
        /// Fires when an enemy is hit
        /// </summary>
        public void OnHit(EnemyAI enemy, PlayerControllerB attackingPlayer, bool playHitSoundEffect) { }

        /// <summary>
        /// Fires when an enemy has been stunned with a grenade or zap gun
        /// </summary>
        public void OnStun(EnemyAI enemy, PlayerControllerB attackingPlayer) { }

        /// <summary>
        /// Fires when a killable enemy has been killed
        /// </summary>
        public void OnKilled(EnemyAI enemy) { }

        /// <summary>
        /// Fires when a killable enemy has been killed, includes destroy flag
        /// </summary>
        public void OnKilled(EnemyAI enemy, bool destroy) { }

        /// <summary>
        /// Called when the enemy first spawns in
        /// </summary>
        public void OnSpawn(EnemyAI enemy) { }

        /// <summary>
        /// Called when the enemy has finished its spawn animation and becomes active
        /// </summary>
        public void OnSpawnFinished(EnemyAI enemy) { }

        /// <summary>
        /// Called when an enemy picks a target player
        /// </summary>
        public void OnTargetPlayer(EnemyAI enemy, PlayerControllerB target) { }

        /// <summary>
        /// Called on the enemy's update loop. If an even you need is missing this can probably be used to check for change in state you want attach some logic to.
        /// If you think something important is missing from a particular enemy's event handler, feel free to let me know.
        /// </summary>
        public void OnEnemyUpdate(EnemyAI enemy) { }

        public void OnEnemyDestroyed(EnemyAI enemy) { }
    }

    /// <summary>
    /// Bracken-specific events
    /// </summary>
    public interface BrackenEventHandler : EnemyEventHandler
    {


        /// <summary>
        /// Called when the bracken has entered its anger state. This occurs after a player has stared at it for too long.
        /// </summary>
        public void OnEnragedStateEntered(FlowermanAI bracken) { }

        /// <summary>
        /// 
        /// </summary>
        public void OnSneakStateEntered(FlowermanAI bracken) { }

        /// <summary>
        /// Called when a bracken enters the evade state. This occurs when a bracken has been caught while trying to sneak up on a player
        /// </summary>
        public void OnEvadeStateEntered(FlowermanAI bracken) { }

        /// <summary>
        /// He got you.
        /// </summary>
        public void OnSnapPlayerNeck(FlowermanAI bracken, PlayerControllerB player) { }

        /// <summary>
        /// Called when a bracken picks up a corpse to drag away.
        /// </summary>
        public void OnPickUpCorpse(FlowermanAI bracken, DeadBodyInfo info) { }

        public void OnDropCorpse(FlowermanAI bracken) { }

        public void OnReachedFavoriteSpot(FlowermanAI bracken) { }
    }

    public interface BaboonHawkEventHandler : EnemyEventHandler
    {
        /// <summary>
        /// Called when the baboonhawk's agression state changes to "intimidate," where it keeps a distance and gives warning calls
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that dispatched the event</param>
        public void OnIntimidate(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk attacks a player
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        /// <param name="player">the player that was attacked</param>
        public void OnAttackPlayer(BaboonBirdAI baboonHawk, PlayerControllerB player) { }
        /// <summary>
        /// Called when the baboon hawk attacks an entity. Baboon hawks cannot hit other baboon hawks.
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        /// <param name="enemy">the enemy that the baboon hawk attacked</param>
        public void OnAttackEnemy(BaboonBirdAI baboonHawk, EnemyAI enemy) { }

        /// <summary>
        /// Called when a Baboon hawk kills a player
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        /// <param name="player">the killed player</param>
        public void OnKillPlayer(BaboonBirdAI baboonHawk, PlayerControllerB player) { }

        /// <summary>
        /// Called when the baboon hawk is finished with its kill animation and returns to its state-driven behavior
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnFinishKillPlayerAnimation(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk starts to sleep
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnSleep(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk sits down at its camp
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnSit(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk gets up after sitting
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnGetUp(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk enters its lowest agression state
        /// </summary>
        /// <param name="baboonHawk"></param>
        public void OnCalmDown(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk enters the highest aggression state and starts attacking a target
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnEnterAttackMode(BaboonBirdAI baboonHawk) { }

        /// <summary>
        /// Called when the baboon hawk picks up scrap
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnPickUpScrap(BaboonBirdAI baboonHawk, GrabbableObject heldItem) { }

        /// <summary>
        /// Called when the baboon hawk drops a piece of scrap
        /// </summary>
        /// <param name="baboonHawk">the baboon hawk that generated the event</param>
        public void OnDropScrap(BaboonBirdAI baboonHawk) { }
    }

    public interface BunkerSpiderEventHandler : EnemyEventHandler
    {
        /// <summary>
        /// Called when a spider wraps a dead player's body in webbing
        /// </summary>
        /// <param name="spider">the bunker spider that generated the event</param>
        /// <param name="wrappedBody">the body being wrapped</param>
        public void OnWrapBody(SandSpiderAI spider, DeadBodyInfo wrappedBody) { }

        /// <summary>
        /// Called when a spider stops wrapping a body for whatever reason
        /// </summary>
        /// <param name="spider">the bunker spider that generated the event</param>
        /// <param name="droppedBody">the body that was being wrapped</param>
        public void OnCancelWrappingBody(SandSpiderAI spider, DeadBodyInfo droppedBody) { }

        /// <summary>
        /// Called when a spider hangs a wrapped body from the ceiling
        /// </summary>
        /// <param name="spider">the bunker spider that generated the event</param>
        /// <param name="wrappedBody">the body being hung up</param>
        public void OnHangBody(SandSpiderAI spider, DeadBodyInfo wrappedBody) { }

        /// <summary>
        /// Called when a spider attacks a player
        /// </summary>
        /// <param name="spider">the bunker spider that generated the event</param>
        /// <param name="player">the player being attacked</param>
        public void OnAttackPlayer(SandSpiderAI spider, PlayerControllerB player) { }

        public void OnEnterWebbingState(SandSpiderAI spider) { }

        public void OnEnterWaitingState(SandSpiderAI spider) { }

        public void OnEnterChasingState(SandSpiderAI spider) { }

        public void OnPlaceWeb(SandSpiderAI spider) { }
    }

    public interface CoilheadEventHandler : EnemyEventHandler
    {
        public void OnEnterRoamingState(SpringManAI coilhead) { }

        public void OnEnterChasingState(SpringManAI coilhead) { }
    }

    public interface EarthLeviathanEventHandler : EnemyEventHandler
    {
        public void OnEmergeFromGround(SandWormAI worm) { }

        public void OnSubmergeIntoGround(SandWormAI worm) { }
    }

    public interface EyelessDogEventHandler : EnemyEventHandler
    {
        public void OnEnterCalmState(MouthDogAI dog) { }

        public void OnEnterSuspiciousState(MouthDogAI dog) { }

        public void OnChaseHowl(MouthDogAI dog) { }

        public void OnEnterChasingState(MouthDogAI dog) { }

        public void OnEnterLungeState(MouthDogAI dog) { }

        public void OnKillPlayer(MouthDogAI instance, PlayerControllerB killedPlayer) { }

        public void OnPickUpBody(MouthDogAI instance, DeadBodyInfo info) { }

        public void OnDropBody(MouthDogAI instance, DeadBodyInfo info) { }

        public void OnEnterCower(MouthDogAI instance) { }

        public void OnExitCower(MouthDogAI instance) { }
    }

    public interface ForestKeeperEventHandler : EnemyEventHandler
    {
        public void OnEnteredRomaingState(ForestGiantAI giant) { }

        public void OnEnteredChasingState(ForestGiantAI giant) { }

        public void OnEnteredBurningState(ForestGiantAI giant) { }

        public void OnGrabbedPlayer(ForestGiantAI giant, PlayerControllerB playerBeingEaten, Vector3 enemyPosition, int enemyYRot) { }
    }

    public interface GhostGirlEventHandler : EnemyEventHandler
    {
        public void OnChoosePlayer(DressGirlAI ghostGirl, PlayerControllerB target) { }

        public void OnStartChasing(DressGirlAI ghostGirl) { }

        public void OnStopChasing(DressGirlAI ghostGirl) { }

        public void OnKillPlayer(DressGirlAI ghostGirl, PlayerControllerB player) { }

        public void OnHide(DressGirlAI ghostGirl) { }

        public void OnShow(DressGirlAI ghostGirl) { }

        public void OnStartToDisappear(DressGirlAI ghostGirl) { }
    }

    public interface HoarderBugEventHandler : EnemyEventHandler
    {
        public void OnEnterChasingState(HoarderBugAI instance) { }

        public void OnExitChasingState(HoarderBugAI instance) { }

        public void OnHitPlayer(HoarderBugAI instance, PlayerControllerB playerControllerB) { }

        public void OnPickUpItem(HoarderBugAI instance, GrabbableObject grabbableObject) { }

        public void OnDropItem(HoarderBugAI instance, GrabbableObject grabbableObject) { }

        public void OnSwitchLookAtPlayer(HoarderBugAI instance, PlayerControllerB watchingPlayer) { }
    }

    public interface HygrodereEventHandler : EnemyEventHandler
    {
        public void OnHitPlayer(BlobAI instance, PlayerControllerB playerControllerB) { }
        public void OnKillPlayer(BlobAI instance, PlayerControllerB playerControllerB) { }
    }

    public interface JesterEventHandler : EnemyEventHandler
    {
        public void OnEnterCrankingState(JesterAI instance) { }
        public void OnEnterPoppedState(JesterAI instance) { }
        public void OnEnterRoamingState(JesterAI instance) { }
        public void OnKillPlayer(JesterAI instance, PlayerControllerB playerControllerB) { }
    }

    public interface NutcrackerEventHandler : EnemyEventHandler
    {
        /// <summary>
        /// Called when the nutcracker does its reload animation
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnReloadShotgun(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker's reload animation is interrupted
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnReloadStopped(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker enters the "inspect" state
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnEnterInspectState(NutcrackerEnemyAI nutcracker, bool headPopUp) { }

        /// <summary>
        /// Called when the nutcracker enters the patrol state, where it moves about with its eye hidden away
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnEnterPatrolState(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker enters the attack state, where it follows and tries to shoot its target
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnEnterAttackState(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker fires its shotgun
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnFireShotgun(NutcrackerEnemyAI nutcracker) { }

        /// <summary>
        /// Called when the nutcracker does its kick attack on a player
        /// </summary>
        /// <param name="nutcracker">the nutcracker instance for this handler</param>
        public void OnKickPlayer(NutcrackerEnemyAI nutcracker, PlayerControllerB player) { }
    }

    public interface SnareFleaEventHandler : EnemyEventHandler
    {
        public void OnBeginAttackMovement(CentipedeAI instance) { }
        public void OnClingToCeiling(CentipedeAI instance) { }
        public void OnClingToPlayer(CentipedeAI instance, PlayerControllerB clingingToPlayer) { }
        public void OnEnterMovingState(CentipedeAI instance) { }
        public void OnFallFromCeiling(CentipedeAI instance) { }
        public void OnHitGroundFromCeiling(CentipedeAI instance) { }
    }

    public interface SporeLizardEventHandler : EnemyEventHandler
    {
        public void OnAlarmed(PufferAI pufferAI) { }
        public void OnEnterAttackState(PufferAI pufferAI) { }
        public void OnEnterAvoidState(PufferAI pufferAI) { }
        public void OnEnterRoamingState(PufferAI pufferAI) { }
        public void OnShakeTail(PufferAI instance) { }
        public void OnStomp(PufferAI instance) { }
        public void OnPuff(PufferAI instance) { }
    }

    public interface ThumperEventHandler : EnemyEventHandler
    {
        void OnBitePlayer(CrawlerAI instance, PlayerControllerB bittenPlayer) { }
        void OnDropBody(CrawlerAI instance, DeadBodyInfo currentlyHeldBody) { }
        void OnEatPlayer(CrawlerAI instance, DeadBodyInfo currentlyHeldBody) { }
        void OnEnterChaseState(CrawlerAI crawlerAI) { }
        void OnEnterSearchState(CrawlerAI crawlerAI) { }
        void OnHitWall(CrawlerAI instance) { }
        void OnScreech(CrawlerAI instance) { }
    }

    public interface ButlerEventHandler : EnemyEventHandler
    {
        public void OnEnterMurderingState(EnemyAI instance) { }
        public void OnEnterPremeditatingState(EnemyAI instance) { }
        public void OnEnterSweepingState(EnemyAI instance) { }
        public void OnInflate(ButlerEnemyAI instance) { }
        public void OnPop(ButlerEnemyAI instance) { }
        public void OnStabPlayer(ButlerEnemyAI instance, PlayerControllerB playerControllerB) { }
        public void OnStep(ButlerEnemyAI instance) { }
        public void OnSweep(ButlerEnemyAI instance) { }
        public void OnSpawnHornets(ButlerEnemyAI instance, ButlerBeesEnemyAI hornets) { }
    }

    public interface OldBirdEventHandler : EnemyEventHandler
    {
        public void OnActivateSpotlight(RadMechAI instance) { }
        public void OnAlerted(RadMechAI instance) { }
        public void OnAlertEnded(EnemyAI instance) { }
        public void OnBlastBrainwashing(RadMechAI instance, int clipIndex) { }
        public void OnCharge(RadMechAI instance) { }
        public void OnDeactivateSpotlight(RadMechAI instance) { }
        public void OnEndTorchPlayer(RadMechAI instance) { }
        public void OnFlickerSpotlight(RadMechAI instance) { }
        public void OnFly(RadMechAI instance) { }
        public void OnGrabPlayer(RadMechAI instance, PlayerControllerB grabbedPlayer) { }
        public void OnLand(RadMechAI instance) { }
        public void OnShootGun(RadMechAI instance) { }
        public void OnStartAiming(RadMechAI instance) { }
        public void OnStomp(RadMechAI instance) { }
        public void OnStopAiming(RadMechAI instance) { }
        public void OnStopCharge(RadMechAI instance) { }
        public void OnTorchPlayer(RadMechAI instance) { }
    }

    public interface BarberEventHandler : EnemyEventHandler
    {
        public void OnStartJump(ClaySurgeonAI barber) { }

        public void OnStopJump(ClaySurgeonAI barber) { }

        public void OnPlayerKilled(ClaySurgeonAI barber, PlayerControllerB killedPlayer) { }
    }

    public interface KidnapperFoxEventHandler : EnemyEventHandler
    {
        /// <summary>
        /// Called any time the kidnapper fox is forced out of dragging a player
        /// </summary>
        /// <param name="fox">the kidnapper fox that dispatched the event</param>
        /// <param name="wasDragging">true if the fox was actually dragging a player</param>
        public void OnCancelReelingPlayer(BushWolfEnemy fox, bool wasDragging) { }

        /// <summary>
        /// Called when the kidnapper fox's tongue is hit, forcing it to release the dragged player
        /// </summary>
        /// <param name="fox">the kidnapper fox that dispatched the event</param>
        public void OnTongueHit(BushWolfEnemy fox) { }

        /// <summary>
        /// Called when the Kidnapper Fox successfully hits a player with its tongue and starts dragging them
        /// </summary>
        /// <param name="fox">the kidnapper fox performing the tongue shot</param>
        /// <param name="draggingPlayer">the player that is now being dragged</param>
        public void OnLandedTongueShot(BushWolfEnemy fox, PlayerControllerB draggingPlayer) { }

        /// <summary>
        /// Called when the Kidnapper Fox starts a tongue shot
        /// </summary>
        /// <param name="fox">the kidnapper fox performing the tongue shot</param>
        public void OnTongueShot(BushWolfEnemy fox) { }
    }

    public interface TulipSnakeEventHandler : EnemyEventHandler
    {
        public void OnStoppedFlapping(FlowerSnakeEnemy snake) { }

        public void OnStartedFlapping(FlowerSnakeEnemy snake) { }

        public void OnClingToPlayer(FlowerSnakeEnemy snake, PlayerControllerB player, int position) { }

        public void OnStopCling(FlowerSnakeEnemy snake) { }

        public void OnStartLeap(FlowerSnakeEnemy snake) { }

        public void OnStopLeap(FlowerSnakeEnemy snake) { }

        public void OnChuckle(FlowerSnakeEnemy snake) { }
    }

    public interface CircuitBeeEventHandler : EnemyEventHandler
    {
        //This is here so any skinners will be posted on the BeeZapAudio.Stop() call when the audio source is replaced
        public void OnZapAudioStop(RedLocustBees bees) { }

        public void OnZapAudioCue(RedLocustBees bees) { }

        public void OnZapAudioStart(RedLocustBees bees) { }

        public void OnLeaveLevel(RedLocustBees bees) { }
    }

    public interface RoamingLocustEventHandler : EnemyEventHandler
    {
        public void OnDisperse(DocileLocustBeesAI locusts) { }

        public void OnGather(DocileLocustBeesAI locusts) { }
    }

    public interface ManticoilEventHandler : EnemyEventHandler
    {
        public void OnTakeOff(DoublewingAI manticoil) { }

        public void OnLand(DoublewingAI manticoil) { }

        public void OnScreech(DoublewingAI manticoil) { }
    }

    public interface ManeaterEventHandler : EnemyEventHandler
    {
    }

    public interface GiantKiwiEventHandler : EnemyEventHandler
    {
        public void OnStartChasing(GiantKiwiAI instance) { }

        public void OnStopChasing(GiantKiwiAI instance) { }

        public void OnKillPlayer(GiantKiwiAI instance, PlayerControllerB player) { }

        public void OnSleep(GiantKiwiAI instance) { }

        public void OnKillEnemy(GiantKiwiAI instance) { }

        public void OnGrabEgg(GiantKiwiAI instance) { }

        public void OnDropEgg(GiantKiwiAI instance) { }
        
        public void OnForceOpenShipDoor(GiantKiwiAI instance) { }
    }
}