using GameNetcodeStuff;

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
        public void OnHit(EnemyAI enemy, PlayerControllerB attackingPlayer);

        /// <summary>
        /// Fires when an enemy has been stunned with a grenade or zap gun
        /// </summary>
        public void OnStun(EnemyAI enemy, PlayerControllerB attackingPlayer);

        /// <summary>
        /// Fires when a killable enemy has been killed
        /// </summary>
        public void OnKilled(EnemyAI enemy);

        /// <summary>
        /// Called when the enemy first spawns in
        /// </summary>
        public void OnSpawn(EnemyAI enemy);

        /// <summary>
        /// Called when the enemy has finished its spawn animation and becomes active
        /// </summary>
        public void OnSpawnFinished(EnemyAI enemy);

        /// <summary>
        /// Called when an enemy picks a target player
        /// </summary>
        public void OnTargetPlayer(EnemyAI enemy, PlayerControllerB target);

        /// <summary>
        /// Called on the enemy's update loop. If an even you need is missing this can probably be used to check for change in state you want attach some logic to.
        /// If you think something important is missing from a particular enemy's event handler, feel free to let me know.
        /// </summary>
        public void Update(EnemyAI enemy);

        public void OnDestroy(EnemyAI enemy);
    }

    /// <summary>
    /// Bracken-specific events
    /// </summary>
    public interface BrackenEventHandler : EnemyEventHandler
    {
        

        /// <summary>
        /// Called when the bracken has entered its anger state. This occurs after a player has stared at it for too long.
        /// </summary>
        public void OnEnragedStateEntered(FlowermanAI bracken);

        /// <summary>
        /// 
        /// </summary>
        public void OnSneakStateEntered(FlowermanAI bracken);

        /// <summary>
        /// Called when a bracken enters the evade state. This occurs when a bracken has been caught while trying to sneak up on a player
        /// </summary>
        public void OnEvadeStateEntered(FlowermanAI bracken);

        /// <summary>
        /// He got you.
        /// </summary>
        public void OnSnapPlayerNeck(FlowermanAI bracken, PlayerControllerB player);

        /// <summary>
        /// Called when a bracken picks up a corpse to drag away.
        /// </summary>
        public void OnPickUpCorpse(FlowermanAI bracken, DeadBodyInfo info);

        public void OnDropCorpse(FlowermanAI bracken);

        public void OnReachedFavoriteSpot(FlowermanAI bracken);
    }

    public interface BaboonHawkEventHandler: EnemyEventHandler
    {
        public void OnIntimidate(BaboonBirdAI baboonHawk);

        public void OnAttackPlayer(BaboonBirdAI baboonHawk, PlayerControllerB player);

        public void OnAttackEnemy(BaboonBirdAI baboonHawk, EnemyAI enemy);

        public void OnKillPlayer(BaboonBirdAI baboonHawk, PlayerControllerB player);

        public void OnFinishKillPlayerAnimation(BaboonBirdAI baboonHawk);

        public void OnSleep(BaboonBirdAI baboonHawk);

        public void OnSit(BaboonBirdAI baboonHawk);

        public void OnGetUp(BaboonBirdAI baboonHawk);

        public void OnCalmDown(BaboonBirdAI baboonHawk);

        public void OnEnterAttackMode(BaboonBirdAI baboonHawk);

        public void OnPickUpScrap(BaboonBirdAI baboonHawk, GrabbableObject heldItem);

        public void OnDropScrap(BaboonBirdAI baboonHawk);
    }

    public interface BunkerSpiderEventHandler : EnemyEventHandler
    {

    }

    public interface CoilheadEventHandler : EnemyEventHandler
    {

    }

    public interface EarthLeviathanEventHandler : EnemyEventHandler
    {

    }

    public interface EyelessDogEventHandler : EnemyEventHandler
    {

    }

    public interface ForestKeeperEventHandler : EnemyEventHandler
    {

    }

    public interface GhostGirlEventHandler : EnemyEventHandler    
    {
        public void OnChoosePlayer(DressGirlAI ghostGirl, PlayerControllerB target);

        public void OnStartChasing(DressGirlAI ghostGirl);

        public void OnStopChasing(DressGirlAI ghostGirl);

        public void OnKillPlayer(DressGirlAI ghostGirl, PlayerControllerB player);

        public void OnHide(DressGirlAI ghostGirl);

        public void OnShow(DressGirlAI ghostGirl);

        public void OnStartToDisappear(DressGirlAI ghostGirl);
    }

    public interface HoarderBugEventHandler : EnemyEventHandler    
    {

    }

    public interface HygrodereEventHandler : EnemyEventHandler    
    {

    }

    public interface JesterEventHandler : EnemyEventHandler    
    {

    }

    public interface NutcrackerEventHandler : EnemyEventHandler    
    {

    }

    public interface SnareFleaEventHandler : EnemyEventHandler    
    {

    }

    public interface SporeLizardEventHandler : EnemyEventHandler    
    {

    }

    public interface ThumperEventHandler : EnemyEventHandler    
    {

    }
}