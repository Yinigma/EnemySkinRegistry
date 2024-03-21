using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System.Linq;
using System.Reflection;
using UnityEngine;
using LethalConfig;
using LethalConfig.ConfigItems;
using AntlerShed.SkinRegistry.View;
using System.Collections.Generic;
using AntlerShed.SkinRegistry.Events;

namespace AntlerShed.SkinRegistry
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class EnemySkinRegistry : BaseUnityPlugin
    {
        public const string modGUID = "antlershed.lethalcompany.enemyskinregistry";
        public const string modName = "EnemySkinRegistry";
        public const string modVersion = "0.0.2";

        public const string GHOST_GIRL_ID = "LethalCompany.GhostGirl";
        public const string THUMPER_ID = "LethalCompany.Thumper";
        public const string HOARDER_BUG_ID = "LethalCompany.HoarderBug";
        public const string NUTCRACKER_ID = "LethalCompany.Nutcracker";
        public const string JESTER_ID = "LethalCompany.Jester";
        public const string SPIDER_ID = "LethalCompany.BunkerSpider";
        public const string HYGRODERE_ID = "LethalCompany.Hygrodere";
        public const string COILHEAD_ID = "LethalCompany.Coilhead";
        public const string SNARE_FLEA_ID = "LethalCompany.SnareFlea";
        public const string SPORE_LIZARD_ID = "LethalCompany.SporeLizard";
        public const string BRACKEN_ID = "LethalCompany.Bracken";
        public const string EYELESS_DOG_ID = "LethalCompany.EyelessDog";
        public const string BABOON_HAWK_ID = "LethalCompany.BaboonHawk";
        public const string FOREST_KEEPER_ID = "LethalCompany.ForestKeeper";
        public const string EARTH_LEVIATHAN_ID = "LethalCompany.EarthLeviathan";

        public const string EXPERIMENTATION_ID = "41 Experimentation";
        public const string ASSURANCE_ID = "220 Assurance";
        public const string VOW_ID = "56 Vow";
        public const string MARCH_ID = "61 March";
        public const string REND_ID = "85 Rend";
        public const string DINE_ID = "7 Dine";
        public const string OFFENSE_ID = "21 Offense";
        public const string TITAN_ID = "8 Titan";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static ManualLogSource SkinLogger { get; private set; } = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        internal static LogLevel LogLevelSetting => skinConfig.LogLevelSetting.Value;

        private static SkinRepository skins = new SkinRepository();
        private static EnemyRepository enemies = new EnemyRepository();
        private static MoonRepository moons = new MoonRepository();
        private static SessionState sessionState = new SessionState();
        private static SkinConfig skinConfig;
        private static EnemyEventHandlerContainer EventHandlers = new EnemyEventHandlerContainer();

        void Awake()
        {
            skinConfig = new SkinConfig(Config);
            harmony.PatchAll(typeof(BaboonHawkPatch));
            harmony.PatchAll(typeof(BrackenPatch));
            harmony.PatchAll(typeof(BunkerSpiderPatch));
            harmony.PatchAll(typeof(CoilheadPatch));
            harmony.PatchAll(typeof(EarthLeviathanPatch));
            harmony.PatchAll(typeof(EyelessDogPatch));
            harmony.PatchAll(typeof(ForestKeeperPatch));
            harmony.PatchAll(typeof(GhostGirlPatch));
            harmony.PatchAll(typeof(HoarderBugPatch));
            harmony.PatchAll(typeof(HygroderePatch));
            harmony.PatchAll(typeof(JesterPatch));
            harmony.PatchAll(typeof(NutcrackerPatch));
            harmony.PatchAll(typeof(SnareFleaPatch));
            harmony.PatchAll(typeof(SporeLizardPatch));
            harmony.PatchAll(typeof(ThumperPatch));
            harmony.PatchAll(typeof(EnemyPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));

            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Finished Patching Skin Events");

            //Register Vanilla Enemies
            RegisterEnemy(GHOST_GIRL_ID, "Ghost Girl");
            RegisterEnemy(THUMPER_ID, "Thumper");
            RegisterEnemy(HOARDER_BUG_ID, "Hoarder Bug");
            RegisterEnemy(NUTCRACKER_ID, "Nutcracker");
            RegisterEnemy(JESTER_ID, "Jester");
            RegisterEnemy(SPIDER_ID, "Bunker Spider");
            RegisterEnemy(HYGRODERE_ID, "Hygrodere");
            RegisterEnemy(COILHEAD_ID, "Coilhead");
            RegisterEnemy(SNARE_FLEA_ID, "Snare Flea");
            RegisterEnemy(SPORE_LIZARD_ID, "Spore Lizard");
            RegisterEnemy(BRACKEN_ID, "Bracken");
            RegisterEnemy(EYELESS_DOG_ID, "Eyeless Dog");
            RegisterEnemy(BABOON_HAWK_ID, "Baboon Hawk");
            RegisterEnemy(FOREST_KEEPER_ID, "Forest Keeper");
            RegisterEnemy(EARTH_LEVIATHAN_ID, "Earth Leviathan");
            if(LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Registered Vanilla Enemies");

            //Register Vanilla Moons
            RegisterMoon(EXPERIMENTATION_ID, "Experimentation");
            RegisterMoon(ASSURANCE_ID, "Assurance");
            RegisterMoon(VOW_ID, "Vow");
            RegisterMoon(OFFENSE_ID, "Offense");
            RegisterMoon(MARCH_ID, "March");
            RegisterMoon(REND_ID, "Rend");
            RegisterMoon(DINE_ID, "Dine");
            RegisterMoon(TITAN_ID, "Titan");
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Registered Vanilla Moons");

            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(path, "AssetBundles/assets"));

            LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<LogLevel>(skinConfig.LogLevelSetting, false));
            LethalConfigManager.AddConfigItem
            (
                new GenericButtonConfigItem
                (
                    "Menu Access", 
                    "Enemy Skin Configuration Menu", 
                    "Nested configuration menu for editing skins", 
                    "Open", 
                    () =>
                    {
                        InitConfig();
                        Dictionary<string, Skin[]> enemySkins = new Dictionary<string, Skin[]>();
                        List<EnemyConfiguration> configs = new List<EnemyConfiguration>();
                        foreach(EnemyInfo enemy in enemies.Enemies.Values)
                        {
                            enemySkins.Add(enemy.Id, skins.Skins.Values.Where((skin => skin.EnemyId.Equals(enemy.Id))).ToArray());
                            EnemyConfiguration? config = skinConfig.GetConfiguration(enemy.Id);
                            if(config!=null)
                            {
                                configs.Add((EnemyConfiguration) config);
                            }
                        }
                        ConfigurationViewModel vm = new ConfigurationViewModel(moons.Moons.Values.ToArray(), enemies.Enemies, enemySkins, configs.ToArray());
                        GameObject skinConfigMenuPrefab = bundle.LoadAsset<GameObject>("assets/enemyskinregistry/ui/skinconfigurationmenu.prefab");
                        //still waiting on that feature request
                        Transform parent = GameObject.Find("QuickMenu")?.transform ?? FindFirstObjectByType<MenuManager>().transform.parent;
                        GameObject configView = Instantiate(skinConfigMenuPrefab, parent);
                        configView.GetComponent<EnemySkinConfigurationMenu>().Init(vm);
                    }
                )
            );
        }

        internal static void InitConfig()
        {
            skinConfig.InitConfigForAllSkins(enemies.Enemies.Values.ToArray(), skins.Skins.Values.ToArray(), moons.Moons.Values.Select((moon)=>moon.Id).ToArray());
        }

        /// <summary>
        /// Registers a skin with this mod. This should be done in your mod's awake method.
        /// </summary>
        /// <param name="skin">the skin to register</param>
        public static void RegisterSkin(Skin skin)
        {
            skins.RegisterSkin(skin);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered skin \"{skin.Label}\"");
        }

        /// <summary>
        /// Register a moon to be configurable in the Enemy Skin Configuration menu
        /// </summary>
        /// <param name="planetName">the planet name field of the moon's SelectableLevel. This has to match, or the moon won't be configurable.</param>
        /// <param name="configLabel">the moon name as you want it to appear in the configuration menu. Really it's just here to get rid of the numbers in front if you want.</param>
        public static void RegisterMoon(string planetName, string configLabel)
        {
            moons.RegisterMoon(planetName, configLabel);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered moon \"{configLabel}\"");
        }

        /// <summary>
        /// Randomly selects a skin from the registry for the given enemy type based on the user's config and the seed of the current play session
        /// </summary>
        /// <param name="enemyType">The type of enemy to select a skin for</param>
        /// <returns>A randomly selected skin that can be applied to the given enemy type</returns>
        //The stuff you can say with a straight face while programming will never cease to amaze me.
        public static Skin PickSkin(string enemyType)
        {
            InitConfig(); //if for whatever reason this hasn't been called yet
            SkinDistribution skinDistribution = skinConfig.GetConfiguredDistribution(moons.GetMoon(sessionState.LevelId)?.Id ?? null, enemyType);
            string selection = skinDistribution.GetSkinAtValue(sessionState.GetRandom());
            return skins.GetSkin(selection);
        }

        /// <summary>
        /// Creates a skinner using the given skin and applies it to the enemy, along with some bookkeeping necessary for mid-round reconfiguration
        /// </summary>
        /// <param name="skin">the skin to use, null if using the vanilla appearance</param>
        /// <param name="enemyTypeId">the type of the enemy, matching whater string was used as the id when the enemy type was registered</param>
        /// <param name="enemy">the enemy instance's game object</param>
        public static void ApplySkin(Skin skin, string enemyTypeId, GameObject enemy)
        {
            
            Skinner skinner = skin?.CreateSkinner() ?? new DummySkinner();
            sessionState.AddSkinner(enemy, skin?.Id, enemyTypeId, skinner);
            skinner.Apply(enemy);
            if (LogLevelSetting >= LogLevel.INFO && skin!=null) SkinLogger.LogInfo($"Applying skin with id \"{skin.Id}\" to enemy of type \"{enemyTypeId}\"");
        }

        /// <summary>
        /// Removes the skinner from the session state. This is called automatically for any enemy that has an EnemyAI component,
        /// but if for whatever reason you're not using that component, call this sometime before your game object is destroyed.
        /// </summary>
        /// <param name="enemy">the enemy to remove the skinner of</param>
        public static void RemoveSkinner(GameObject enemy)
        {
            sessionState.RemoveInstance(enemy);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Removed skinner from enemy instance");
        }

        internal static void UpdateRoundInfo(int seed, string level)
        {
            sessionState.SetCurrentLevel(level);
            sessionState.SetRandomNumberGenerator(seed);
        }

        /// <summary>
        /// Register an enemy with this mod. 
        /// </summary>
        /// <param name="enemyId">unique string identifying this enemy. It's arbitrary, but to help ensure uniqueness it's recommended to include the mod guid in this identifier</param>
        /// <param name="label">The label to appear in the menu</param>
        public static void RegisterEnemy(string enemyId, string label)
        {
            enemies.RegisterEnemy(enemyId, label);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered enemy with name \"{label}\"");
        }

        internal static void UpdateConfiguration(EnemyConfiguration[] updatedEntries)
        {
            skinConfig.UpdateConfiguration(updatedEntries);
            //Remove any skins that have been marked as inactive.
            //The skins can't be immediately re-rolled because the armature reflector requires.
            //"But shouldn't this component not care what your default implementation of a skinner is doing???"
            //Yeah.
            //But don't tell nobody. Maybe I'll figure out how to swap a skinned mesh like a sane person one of these days, in spite of unity, blender, and zeekerss.
            foreach (GameObject enemyGameObject in sessionState.GetSkinnedEnemies())
            {
                string enemyType = sessionState.GetEnemyType(enemyGameObject);
                string skinId = sessionState.GetSkinId(enemyGameObject);
                Skinner skinner = sessionState.GetSkinner(enemyGameObject);
                if (enemyType != null && skinId != null && skinner != null)
                {
                    string[] active = skinConfig.GetConfiguration(enemyType)?.ActiveSkins ?? new string[0];
                    if(!active.Contains(skinId))
                    {
                        skinner.Remove(enemyGameObject);
                        sessionState.ClearSkinner(enemyGameObject);
                    }
                }
            }
        }

        /// <summary>
        /// Retreives the event handlers associated with the given EnemyAI component. If none exist, this returns an empty list.
        /// If the given key is invalid, this will also just return an empty list.
        /// </summary>
        /// <param name="enemy">the enemy's EnemyAI component</param>
        /// <returns>the list of event handlers associated with enemy</returns>
        public static List<EnemyEventHandler> GetEnemyEventHandlers(EnemyAI enemy)
        {
            return EventHandlers.GetEventHandlers(enemy);
        }

        /// <summary>
        /// Maps an event handler implementation to the enemy with the given EnemyAI component. 
        /// This must be done for each enemy instance and it's recommended that it be done in the Apply method of your Skinner.
        /// An Enemy AI can have multiple handlers associated with it, so adding one will not remove another.
        /// </summary>
        /// <param name="enemy">The EnemyAI component of the enemy you're attaching your handler to</param>
        /// <param name="handler">the handler to register</param>
        public static void RegisterEnemyEventHandler(EnemyAI enemy, EnemyEventHandler handler)
        {
            EventHandlers.RegisterEnemyEventHandler(enemy, handler);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered event handler for enemy.");
        }

        /// <summary>
        /// Unmaps an event handler from an enemy. This should normally be called from the Remove method in a Skinner implmentation, provided a
        /// call to register
        /// </summary>
        /// <param name="enemy">the enmy to remove a handler from</param>
        /// <param name="enemy">the handler to remove</param>
        public static void RemoveEnemyEventHandler(EnemyAI enemy, EnemyEventHandler handler)
        {
            EventHandlers.RemoveEnemyEventHandler(enemy, handler);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Removed event handler for enemy.");
        }

        internal static string VanillaIdFromInstance(EnemyAI enemy)
        {
            switch(enemy)
            {
                case FlowermanAI bracken:
                    return BRACKEN_ID;
                case HoarderBugAI bug:
                    return HOARDER_BUG_ID;
                case SandSpiderAI spider:
                    return SPIDER_ID;
                case BaboonBirdAI bird:
                    return BABOON_HAWK_ID;
                case SpringManAI coilhead:
                    return COILHEAD_ID;
                case SandWormAI worm:
                    return EARTH_LEVIATHAN_ID;
                case MouthDogAI dog:
                    return EYELESS_DOG_ID;
                case ForestGiantAI giant:
                    return FOREST_KEEPER_ID;
                case DressGirlAI girl:
                    return GHOST_GIRL_ID;
                case BlobAI slime:
                    return HYGRODERE_ID;
                case JesterAI jester:
                    return JESTER_ID;
                case NutcrackerEnemyAI cracker:
                    return NUTCRACKER_ID;
                case CentipedeAI centipede:
                    return SNARE_FLEA_ID;
                case PufferAI puffer:
                    return SPORE_LIZARD_ID;
                case CrawlerAI crawler:
                    return THUMPER_ID;
                default:
                    return null;
            }
        }
    }

    internal enum LogLevel
    {
        NONE,
        ERROR,
        WARN,
        INFO
    }

    internal struct EnemyInfo
    {
        public string Name { get; }
        public string Id { get; }

        public EnemyInfo(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }

    internal struct MoonInfo
    {
        public string Name { get; }
        public string Id { get; }

        internal MoonInfo(string name, string id)
        {
            Name = name;
            Id = id;
        }
    }

    internal class SkinDistribution
    {
        private SkinThreshold[] skins;

        internal SkinDistribution(SkinThreshold[] skins)
        {
            this.skins = skins;
        }

        internal SkinDistribution()
        {
            skins = new SkinThreshold[0];
        }

        internal string GetSkinAtValue(float value)
        {
            int i = 0;

            while (i < skins.Length && value > skins[i].Threshold)
            {
                i++;
            }
            if(i>=skins.Length)
            {
                return null;
            }
            return skins[i].SkinId;
        }
    }

    internal struct SkinThreshold
    {
        public float Threshold{ get; }
        public string SkinId { get; }

        public SkinThreshold(string skinId, float threshold)
        {
            Threshold = threshold;
            SkinId = skinId;
        }
    }
}