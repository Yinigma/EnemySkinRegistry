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
using System;
using System.Text.RegularExpressions;

namespace AntlerShed.SkinRegistry
{
    [BepInPlugin(modGUID, modName, modVersion)]
    [BepInDependency("ainavt.lc.lethalconfig")]
    public class EnemySkinRegistry : BaseUnityPlugin
    {
        public const string modGUID = "antlershed.lethalcompany.enemyskinregistry";
        public const string modName = "EnemySkinRegistry";
        public const string modVersion = "1.4.6";

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

        public const string MANTICOIL_ID = "LethalCompany.Manticoil";
        public const string CIRCUIT_BEES_ID = "LethalCompany.CircuitBees";
        public const string ROAMING_LOCUST_ID = "LethalCompany.RoamingLocust";
        //v50
        public const string OLD_BIRD_ID = "LethalCompany.OldBird";
        public const string BUTLER_ID = "LethalCompany.Butler";
        public const string TULIP_SNAKE_ID = "LethalCompany.TulipSnake";
        //v55
        public const string BARBER_ID = "LethalCompany.Barber";
        public const string KIDNAPPER_FOX_ID = "LethalCompany.KidnapperFox";
        //v60
        public const string MANEATER_ID = "LethalCompany.Maneater";

        public const string EXPERIMENTATION_ID = "41 Experimentation";
        public const string ASSURANCE_ID = "220 Assurance";
        public const string VOW_ID = "56 Vow";
        public const string MARCH_ID = "61 March";
        public const string REND_ID = "85 Rend";
        public const string DINE_ID = "7 Dine";
        public const string OFFENSE_ID = "21 Offense";
        public const string TITAN_ID = "8 Titan";
        //v50
        public const string ARTIFICE_ID = "68 Artifice";
        public const string EMBRION_ID = "5 Embrion";
        public const string ADAMANCE_ID = "20 Adamance";

        public const string WASTELAND_TAG = "wasteland";
        public const string CANYON_TAG = "canyon";
        public const string VALLEY_TAG = "valley";
        public const string TUNDRA_TAG = "tundra";
        public const string MARSH_TAG = "marsh";
        public const string MILITARY_TAG = "military";
        public const string ROCKY_TAG = "rocky";

        public const string FREE_TAG = "free";
        public const string PAID_TAG = "paid";

        private readonly Harmony harmony = new Harmony(modGUID);

        internal static ManualLogSource SkinLogger { get; private set; } = BepInEx.Logging.Logger.CreateLogSource(modGUID);
        internal static LogLevel LogLevelSetting => skinConfig?.LogLevelSetting?.Value ?? LogLevel.INFO;
        internal static bool AllowSyncSetting => skinConfig?.AllowSyncSetting?.Value ?? false;
        internal static bool AttemptSyncSetting => skinConfig?.AttemptSyncSetting?.Value ?? false;
        internal static bool AllowIndoorOutdoorConfig => skinConfig?.IndoorOutdoorSetting?.Value ?? true;

        internal static bool ClientSyncActive => (configClient?.Running ?? false) && AttemptSyncSetting;

        private static bool configInitialized = false;

        private static SkinRepository skins = new SkinRepository();
        private static EnemyRepository enemies = new EnemyRepository();
        private static MoonRepository moons = new MoonRepository();
        private static SessionState sessionState = new SessionState();
        private static SkinConfig skinConfig;
        private static ConfigServer configServer = new ConfigServer();
        private static ConfigClient configClient = new ConfigClient();
        private static EnemyEventHandlerContainer EventHandlers = new EnemyEventHandlerContainer();

        void Awake()
        {
            skinConfig = new SkinConfig(Config);//Path.Combine(Paths.ConfigPath, modGUID));
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

            harmony.PatchAll(typeof(CircuitBeesPatch));
            harmony.PatchAll(typeof(RoamingLocustPatch));
            harmony.PatchAll(typeof(TulipSnakePatch));

            harmony.PatchAll(typeof(EnemyPatch));
            harmony.PatchAll(typeof(InitializationPatch));
            //v50
            harmony.PatchAll(typeof(ButlerPatch));
            harmony.PatchAll(typeof(OldBirdPatch));
            //v55
            harmony.PatchAll(typeof(BarberPatch));
            harmony.PatchAll(typeof(KidnapperFoxPatch));
            //v60
            harmony.PatchAll(typeof(ManeaterPatch));

            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Finished Patching Skin Events");

            //Register Vanilla Enemies
            RegisterEnemy(GHOST_GIRL_ID, "Ghost Girl", SpawnLocation.INDOOR);
            RegisterEnemy(THUMPER_ID, "Thumper", SpawnLocation.INDOOR);
            RegisterEnemy(HOARDER_BUG_ID, "Hoarder Bug", SpawnLocation.INDOOR);
            RegisterEnemy(NUTCRACKER_ID, "Nutcracker", SpawnLocation.INDOOR);
            RegisterEnemy(JESTER_ID, "Jester", SpawnLocation.INDOOR);
            RegisterEnemy(SPIDER_ID, "Bunker Spider", SpawnLocation.INDOOR);
            RegisterEnemy(HYGRODERE_ID, "Hygrodere", SpawnLocation.INDOOR);
            RegisterEnemy(COILHEAD_ID, "Coilhead", SpawnLocation.INDOOR);
            RegisterEnemy(SNARE_FLEA_ID, "Snare Flea", SpawnLocation.INDOOR);
            RegisterEnemy(SPORE_LIZARD_ID, "Spore Lizard", SpawnLocation.INDOOR);
            RegisterEnemy(BRACKEN_ID, "Bracken", SpawnLocation.INDOOR);
            RegisterEnemy(EYELESS_DOG_ID, "Eyeless Dog", SpawnLocation.OUTDOOR);
            RegisterEnemy(BABOON_HAWK_ID, "Baboon Hawk", SpawnLocation.OUTDOOR);
            RegisterEnemy(FOREST_KEEPER_ID, "Forest Keeper", SpawnLocation.OUTDOOR);
            RegisterEnemy(EARTH_LEVIATHAN_ID, "Earth Leviathan", SpawnLocation.OUTDOOR);
            
            RegisterEnemy(CIRCUIT_BEES_ID, "Circuit Bees", SpawnLocation.OUTDOOR);
            RegisterEnemy(MANTICOIL_ID, "Manticoil", SpawnLocation.OUTDOOR);
            RegisterEnemy(ROAMING_LOCUST_ID, "Roaming Locusts", SpawnLocation.OUTDOOR);
            //v50
            RegisterEnemy(OLD_BIRD_ID, "Old Bird", SpawnLocation.OUTDOOR);
            RegisterEnemy(BUTLER_ID, "Butler", SpawnLocation.INDOOR);
            RegisterEnemy(TULIP_SNAKE_ID, "Tulip Snake", SpawnLocation.OUTDOOR);
            //v55
            RegisterEnemy(KIDNAPPER_FOX_ID, "Kidnapper Fox", SpawnLocation.OUTDOOR);
            RegisterEnemy(BARBER_ID, "Barber", SpawnLocation.INDOOR);
            //v60
            RegisterEnemy(MANEATER_ID, "Maneater", SpawnLocation.INDOOR);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Registered Vanilla Enemies");

            //Register Vanilla Moons
            RegisterMoon(EXPERIMENTATION_ID, "Experimentation", new string[] { WASTELAND_TAG, FREE_TAG }, null);
            RegisterMoon(ASSURANCE_ID, "Assurance", new string[] { CANYON_TAG, FREE_TAG }, null);
            RegisterMoon(VOW_ID, "Vow", new string[] { VALLEY_TAG, FREE_TAG }, null);

            RegisterMoon(OFFENSE_ID, "Offense", new string[] { CANYON_TAG, FREE_TAG }, null);
            RegisterMoon(MARCH_ID, "March", new string[] { VALLEY_TAG, FREE_TAG }, null);

            RegisterMoon(REND_ID, "Rend", new string[] { TUNDRA_TAG, PAID_TAG }, null);
            RegisterMoon(DINE_ID, "Dine", new string[] { TUNDRA_TAG, PAID_TAG }, null);
            RegisterMoon(TITAN_ID, "Titan", new string[] { TUNDRA_TAG, PAID_TAG }, null);
            //v50
            RegisterMoon(ADAMANCE_ID, "Adamance", new string[] { VALLEY_TAG, FREE_TAG }, null);
            RegisterMoon(ARTIFICE_ID, "Artifice", new string[] { MARSH_TAG, MILITARY_TAG, PAID_TAG }, null);
            RegisterMoon(EMBRION_ID, "Embrion", new string[] { ROCKY_TAG, PAID_TAG }, null);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Registered Vanilla Moons");

            string path = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AssetBundle bundle = AssetBundle.LoadFromFile(System.IO.Path.Combine(path, "AssetBundles/assets"));

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
                        Transform parent = GameObject.Find("QuickMenu")?.transform ?? FindFirstObjectByType<MenuManager>().transform.parent;
                        //InitConfig();
                        
                        Dictionary<string, Skin[]> enemySkins = new Dictionary<string, Skin[]>();
                        foreach (EnemyInfo enemy in enemies.Enemies.Values)
                        {
                            enemySkins.Add(enemy.Id, skins.Skins.Values.Where((skin => skin.EnemyId.Equals(enemy.Id))).ToArray());
                        }
                        ConfigurationViewModel vm = new ConfigurationViewModel
                        (
                            moons.Moons.Values.ToArray(),
                            moons.MoonTags.ToArray(),
                            enemies.Enemies,
                            enemySkins,
                            skinConfig.GetProfiles(),
                            skinConfig.GetWorkingProfile().Copy(),
                            skinConfig.DefaultSkinConfigs
                                .Where((pair) => skins.Skins.ContainsKey(pair.Key))
                                .Select((pair) => new KeyValuePair<string, string>(pair.Key, skins.Skins[pair.Key].Label))
                                .ToDictionary((pair) => pair.Key, (pair) => pair.Value),
                            skinConfig.DefaultMoonConfigs
                                .Where((pair) => moons.Moons.ContainsKey(pair.Key))
                                .Select((pair) => new KeyValuePair<string, string>(pair.Key, moons.Moons[pair.Key].Name))
                                .ToDictionary((pair) => pair.Key, (pair) => pair.Value)
                        );
                        GameObject skinConfigMenuPrefab = bundle.LoadAsset<GameObject>("assets/enemyskinregistry/ui/skinconfigurationmenu.prefab");
                        //waiting on that feature request
                            
                        GameObject configView = Instantiate(skinConfigMenuPrefab, parent);
                        configView.GetComponent<EnemySkinConfigurationMenu>().Init(vm);
                        
                    }
                )
            );
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(skinConfig.AttemptSyncSetting, false ));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(skinConfig.AllowSyncSetting, false));
            LethalConfigManager.AddConfigItem(new BoolCheckBoxConfigItem(skinConfig.IndoorOutdoorSetting, false));
            LethalConfigManager.AddConfigItem(new EnumDropDownConfigItem<LogLevel>(skinConfig.LogLevelSetting, false));

            /*try
            {
                if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "LethalLevelLoader"))
                {
                    if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Found lethal level loader plugin");
                    LethalLevelLoaderCompatibility.RegisterInitCallback();
                }
            }
            catch { }*/
        }

        internal static void InitConfig()
        {
            if (!configInitialized)
            {
                try
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "LethalLevelLoader"))
                    {
                        if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Found lethal level loader plugin");
                        LethalLevelLoaderCompatibility.RegisterLLLMaps();
                    }
                }
                catch { }
                if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogMessage("Initializing config");
                skinConfig.InitConfigForAllSkins(enemies.Enemies.Values.ToArray(), skins.Skins.Values.ToArray(), moons.Moons.Values.Select((moon) => moon.Id).ToArray(), moons.MoonTags.ToHashSet());
                configInitialized = true;
            }
        }

        /// <summary>
        /// Registers a skin with this mod. This should be done in your mod's awake method.
        /// </summary>
        /// <param name="skin">the skin to register</param>
        /// <param name="defaultConfig">optional default configuration for this skin. Applied once when the mod is first added and can be reapplied from the skin configuration menu.</param>
        public static void RegisterSkin(Skin skin, DefaultSkinConfigData? defaultConfig = null)
        {
            try
            {
                skins.RegisterSkin(skin);
                if (defaultConfig.HasValue)
                {
                    skinConfig.AddDefaultSkinConfig
                    (
                        new DefaultSkinConfiguration
                        (
                            skin.Id,
                            defaultConfig.Value.defaultEntries,
                            defaultConfig.Value.defaultOutdoorFrequency,
                            defaultConfig.Value.vanillaFallbackOutdoorFrequency,
                            defaultConfig.Value.defaultIndoorFrequency,
                            defaultConfig.Value.vanillaFallbackIndoorFrequency
                        )
                    );
                }
                if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered skin \"{skin.Label}\"");
            }
            catch (Exception e)
            {
                if(e is DuplicateSkinException || e is InvalidSkinIdException)
                {
                    if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError(e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Registers a skin with this mod. This should be done in your mod's awake method.
        /// </summary>
        /// <param name="skin">the skin to register</param>
        public static void RegisterSkin(Skin skin)
        {
            //included for backwards compatibility. Really screwed the pooch here.
            RegisterSkin(skin, null);
        }

        /// <summary>
        /// Registers a nest skin. Nest skins apply to the "nest" objects that enemies like the Old Bird spawn from. 
        /// They must use the same id as the skin the nest will spawn. 
        /// </summary>
        /// <param name="nestSkin">the skin to register</param>
        public static void RegisterNestSkin(NestSkin nestSkin)
        {
            try
            {
                skins.RegisterNestSkin(nestSkin);
            }
            catch (Exception e)
            {
                if (e is DuplicateSkinException || e is InvalidSkinIdException || e is MissingSkinException)
                {
                    if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError(e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        /// <summary>
        /// Register a moon to be configurable in the Enemy Skin Configuration menu
        /// </summary>
        /// <param name="planetName">the planet name field of the moon's SelectableLevel. This has to match, or the moon won't be configurable.</param>
        /// <param name="configLabel">the moon name as you want it to appear in the configuration menu. Really it's just here to get rid of the numbers in front if you want.</param>
        /// <param name="tags">A list of descriptive tags associated with the moon</param>
        /// <param name="defaultConfig">default configuration of skins on this moon applied when the moon is first added and can be reapplied from the skin configuration menu.</param>
        public static void RegisterMoon(string planetName, string configLabel, string[] tags, DefaultMapConfigEntry[] defaultConfig)
        {
            try
            {
                moons.RegisterMoon(planetName, configLabel, tags);
                if (defaultConfig != null && defaultConfig.Length != 0)
                {
                    skinConfig.AddDefaultMoonConfig(new DefaultMapConfiguration(planetName, defaultConfig));
                }
                if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered moon with id \"{planetName}\"");
            }
            catch (Exception e)
            {
                if (e is MoonKeyCollisionException || e is InvalidMoonIdException)
                {
                    if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError(e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        public static Skin PickSkinAtValue(string enemyType, SpawnLocation spawnLocation, float val)
        {
            MoonInfo? moon = moons.GetMoon(RoundManager.Instance?.currentLevel?.PlanetName);
            SkinDistribution skinDistribution = skinConfig.GetConfiguredDistribution(moon?.Id ?? null, moon?.Tags.ToArray() ?? new string[0], enemyType, spawnLocation);
            string selection = skinDistribution.GetSkinAtValue(val);
            return skins.GetSkin(selection);
        }

        /// <summary>
        /// Selects a skin based off the enemy's type and whether or not it has a nest it spawned from
        /// </summary>
        /// <param name="enemy">The game object of the enemy instance that will be skinned</param>
        /// <param name="enemyType">The type of enemy to select a skin for</param>
        /// <param name="location">Type of location the enemy is spawning in</param>
        /// <returns>A randomly selected skin that can be applied to the given enemy type</returns>
        public static Skin SelectSpawnSkin(GameObject enemy, string enemyType, SpawnLocation location, float value)
        {
            if(LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Attempting to skin an enemy of type {enemyType}:{location} with randomValue {value} on moon with level id: {RoundManager.Instance?.currentLevel?.PlanetName}");
            if(sessionState.SpawnedFromNest(enemy))
            {
                string stagedId = sessionState.RetrieveStagedSkin(enemy);
                return stagedId != null && skins.Skins.ContainsKey(stagedId) ? skins.Skins[stagedId] : null;
            }
            else
            {
                return PickSkinAtValue(enemyType, location, value);
            }
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

        public static void ApplyNestSkin(string skinId, string enemyTypeId, EnemyAINestSpawnObject nest)
        {
            NestSkin nestSkin = skinId != null && skins.NestSkins.ContainsKey(skinId) ? skins.NestSkins[skinId] : null;
            Skinner skinner = nestSkin?.CreateNestSkinner() ?? new DummySkinner();
            sessionState.AddSkinner(nest.gameObject, skinId, enemyTypeId, skinner);
            sessionState.AddSkinNest(nest, skinId);
            skinner.Apply(nest.gameObject);
            if (LogLevelSetting >= LogLevel.INFO && skinId != null && skins.NestSkins.ContainsKey(skinId)) SkinLogger.LogInfo($"Applying nest skin with id \"{skinId}\" to enemy nest of type \"{enemyTypeId}\"");
        }

        /// <summary>
        /// Removes the skinner from the session state. This is called automatically for any enemy that has an EnemyAI component,
        /// but if for whatever reason you're not using that component, call this sometime before your game object is destroyed.
        /// </summary>
        /// <param name="enemy">the enemy to remove the skinner of</param>
        public static void RemoveSkinner(GameObject enemy)
        {
            sessionState.GetSkinner(enemy)?.Remove(enemy);
            sessionState.RemoveInstance(enemy);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Removed skinner from enemy instance");
        }

        /// <summary>
        /// Register an enemy with this mod. 
        /// </summary>
        /// <param name="enemyId">unique string identifying this enemy. It's arbitrary, but to help ensure uniqueness it's recommended to include the mod guid in this identifier</param>
        /// <param name="label">The label to appear in the menu</param>
        /// <param name="spawnLocation">The usual spawn location for this enemy. Used to determine what frequency to use when indoor/outdoor config has been disabled</param>
        public static void RegisterEnemy(string enemyId, string label, SpawnLocation spawnLocation)
        {
            try
            {
                enemies.RegisterEnemy(enemyId, label, spawnLocation);
                if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Registered enemy with name \"{label}\"");
            }
            catch (Exception e)
            {
                if (e is DuplicateEnemyException || e is InvalidEnemyTypeException)
                {
                    if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError(e.Message);
                }
                else
                {
                    throw e;
                }
            }
        }

        public static void ReassignSkin(GameObject enemy, string newSkinId)
        {
            Skinner skinner = sessionState.GetSkinner(enemy);
            string enemyType = sessionState.GetEnemyType(enemy);
            if(enemyType!= null && skinner!=null)
            {
                Skin newSkin = skins.GetSkin(newSkinId);
                if (newSkin != null && enemyType.Equals(skins.GetSkin(newSkinId).EnemyId))
                {
                    skinner.Remove(enemy);
                    sessionState.ClearSkinner(enemy);
                    Skinner newSkinner = newSkin?.CreateSkinner();
                    sessionState.AddSkinner(enemy, newSkin?.Id, newSkin.EnemyId, newSkinner);
                    newSkinner.Apply(enemy);
                }
                else if(newSkin == null)
                {
                    if (LogLevelSetting >= LogLevel.WARN) SkinLogger.LogWarning($"No skin was found for id \"{newSkinId}.\" This enemy will be left as is.");
                }
                else if(!enemyType.Equals(skins.GetSkin(newSkinId).EnemyId))
                {
                    if (LogLevelSetting >= LogLevel.WARN) SkinLogger.LogWarning($"You're trying to assign a skin with enemy type \"{newSkin.EnemyId}\" to an enemy with type \"{enemyType}.\" This enemy will be left as is.");
                }
            }
            else
            {
                if (LogLevelSetting >= LogLevel.WARN) SkinLogger.LogWarning($"No skinner was found for object \"{enemy.name}.\" Use EnemySkinRegistry.ApplySkin for enemies that do not have a skinner instance on them for whatever reason.");
            }
        }

        /// <summary>
        /// Gets the id of the skin for this enemy if it exists
        /// </summary>
        /// <param name="enemy">Usually the game object that the EnemyAI component is on, but it could be different for custom enemies</param>
        /// <returns>the id of the skin being applied to the enemy, null if no skin is applied</returns>
        public static string GetSkinId(GameObject enemy)
        {
            return sessionState.GetSkinId(enemy);
        }

        /// <summary>
        /// Gets the skin for the given id if it exists
        /// </summary>
        /// <param name="skinId">the unique id of the skin</param>
        /// <returns>the skin data corresponding to the given id, or null if it doesn't exist</returns>
        public Skin GetSkinData(string skinId)
        {
            return skins.GetSkin(skinId);
        }

        /// <summary>
        /// Gets skin registry enemy type id (this is distinct from the label in the vanilla EnemyType class)
        /// </summary>
        /// <param name="enemy">Usually the game object that the EnemyAI component is on, but it could be different for custom enemies</param>
        /// <returns>the skin registry's id for this enemy type</returns>
        public static string GetEnemyType(GameObject enemy)
        {
            return sessionState.GetEnemyType(enemy);
        }

        /// <summary>
        /// Gets skinner instance applied to this game object if it exists
        /// </summary>
        /// <param name="enemy">Usually the game object that the EnemyAI component is on, but it could be different for custom enemies</param>
        /// <returns>the skinner instance attached to this enemy</returns>
        public static Skinner GetSkinnerInstance(GameObject enemy)
        {
            return sessionState.GetSkinner(enemy);
        }

        /// <summary>
        /// Register an enemy with this mod. 
        /// </summary>
        /// <param name="enemyId">unique string identifying this enemy. It's arbitrary, but to help ensure uniqueness it's recommended to include the mod guid in this identifier</param>
        /// <param name="label">The label to appear in the menu</param>
        [Obsolete("The old signature for register enemy is deprecatecd. If you were previously using this method to register your enemy, call RegisterEnemy(string, string, Spawnlocation) instead to configure it's usual spawn location type. Ths method defaults to indoor.")]
        public static void RegisterEnemy(string enemyId, string label)
        {
            RegisterEnemy(enemyId, label, SpawnLocation.INDOOR);
        }

        internal static void UpdateConfiguration(Profile updatedProfile)
        {
            if (!(configClient.Running && AttemptSyncSetting))
            {
                skinConfig.UpdateConfiguration(updatedProfile.Copy());
            }
            if(configServer.Running)
            {
                configServer.BroadcastSyncMessage(skinConfig.GetWorkingProfile());
            }
            applyDeactivatedSkins();
        }

        private static void applyDeactivatedSkins()
        {
            //Remove any skins that have been marked as inactive.
            //The skins can't be immediately re-rolled because the armature reflector requires the skeleton to be in a bind pose when applying.
            //"But shouldn't this component not care what your default implementation of a skinner is doing???"
            //Yeah.
            //But don't tell nobody. Maybe I'll figure out how to swap a skinned mesh like a sane person one of these days, in spite of unity, blender, and zeekerss.
            foreach (GameObject enemyGameObject in sessionState.GetSkinnedObject())
            {
                string enemyType = sessionState.GetEnemyType(enemyGameObject);
                string skinId = sessionState.GetSkinId(enemyGameObject);
                Skinner skinner = sessionState.GetSkinner(enemyGameObject);
                if (enemyType != null && skinId != null && skinner != null)
                {
                    string[] active = skinConfig.GetWorkingProfile().GetEnemyConfig(enemyType)?.ActiveSkins ?? new string[0];
                    if (!active.Contains(skinId))
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
        /// <param name="handler">the handler to remove</param>
        public static void RemoveEnemyEventHandler(EnemyAI enemy, EnemyEventHandler handler)
        {
            EventHandlers.RemoveEnemyEventHandler(enemy, handler);
            if (LogLevelSetting >= LogLevel.INFO) SkinLogger.LogInfo($"Removed event handler for enemy.");
        }

        public static void StageSkin(GameObject spawnedEnemy, EnemyAINestSpawnObject spawner)
        {
            sessionState.StageSkinForSpawn(spawner, spawnedEnemy);
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
                case RedLocustBees bees:
                    return CIRCUIT_BEES_ID;
                case DocileLocustBeesAI locusts:
                    return ROAMING_LOCUST_ID;
                case DoublewingAI manticoil:
                    return MANTICOIL_ID;
                //v50
                case ButlerEnemyAI:
                    return BUTLER_ID;
                case RadMechAI mech:
                    return OLD_BIRD_ID;
                case FlowerSnakeEnemy snake:
                    return TULIP_SNAKE_ID;
                //v55
                case ClaySurgeonAI klayman:
                    return BARBER_ID;
                case BushWolfEnemy fox:
                    return KIDNAPPER_FOX_ID;
                //v60
                case CaveDwellerAI maneater:
                    return MANEATER_ID;
                default:
                    return null;
            }
        }

        internal static void ApplyDefaultSkinConfiguration(string skinId, Profile config)
        {
            if(skinId != null && skins.Skins.ContainsKey(skinId))
            {
                skinConfig.ApplyDefaultSkinConfig(skinId, config, skins.Skins[skinId].EnemyId, moons.Moons.Keys.ToArray(), moons.MoonTags.ToArray());
            }
            else
            {
                if (LogLevelSetting >= LogLevel.WARN) SkinLogger.LogWarning($"Skipping default configuration for skin with id \"{skinId}\" because it is not installed.");
            }

        }

        internal static void ApplyDefaultMoonConfiguration(string moonId, Profile config)
        {
            if(moonId!= null && moons.Moons.ContainsKey(moonId))
            {
                skinConfig.ApplyDefaultMoonConfig(moonId, config, skins.Skins.Keys.ToArray());
            }
            else
            {
                if (LogLevelSetting >= LogLevel.WARN) SkinLogger.LogWarning($"Skipping default configuration for moon with id \"{moonId}\" because it is not installed.");
            }
        }

        /// <summary>
        /// Gets a reference to the working profile
        /// </summary>
        /// <returns>a refence to this registry's working spawn profile. Edit at your own risk.</returns>
        internal static Profile GetWorkingProfile()
        {
            return skinConfig.GetWorkingProfile();
        }

        internal static void SyncWithRemoteProfile(Profile cfg)
        {
            List<string> enemyDiff = cfg.ConfigData
                //get server enemy ids
                .Select((EnemyConfiguration enemyConfig) => enemyConfig.EnemyId).ToHashSet()
                //diff with client enemy ids
                .Except(enemies.Enemies.Keys.ToHashSet()).ToList();

            List<string> skinDiff = cfg.ConfigData
                //get server enemy configs
                .Select((EnemyConfiguration enemyConfig) => enemyConfig.ActiveSkins)
                //stuff all of the active skin ids into a set (I reckon this is about as permissive as I can sensibly get)
                .Aggregate(new HashSet<string>(), (list, arr) => list.Union(arr).ToHashSet())
                //diff with client skin ids
                .Except(skins.Skins.Keys.ToHashSet()).ToList();

            List<string> moonDiff = cfg.ConfigData
                //get all the map configs
                .Select((EnemyConfiguration enemyConfig) => enemyConfig.MapConfigs)
                //turn the configs into a set of all the moon ids that show up in them
                .Aggregate(new HashSet<string>(), (list, moonCfgs) => list.Union(moonCfgs.Select(moonCfg => moonCfg.Id)).ToHashSet())
                //diff with this client's moon ids
                .Except(moons.Moons.Keys.ToHashSet()).ToList();
            if (enemyDiff.Count > 0)
            {
                if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError($"Could not sync with remote profile. This client is missing the following enemies: {enemyDiff.Aggregate("", (current, enemy) => current + (string.IsNullOrEmpty(current) ? "" : ", ") + enemy)}");
            }
            if (skinDiff.Count > 0)
            {
                if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError($"Could not sync with remote profile. This client is missing the following skins: {skinDiff.Aggregate("", (current, skin) => current + (string.IsNullOrEmpty(current) ? "" : ", ") + skin)}");
            }
            if (moonDiff.Count > 0)
            {
                if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError($"Could not sync with remote profile. This client is missing the following moons: {moonDiff.Aggregate("", (current, moon) => current + (string.IsNullOrEmpty(current) ? "" : ", ") + moon)}");
            }
            if(moonDiff.Count == 0 && skinDiff.Count == 0 && enemyDiff.Count == 0)
            {
                /*foreach(EnemyConfiguration enemyConfig in cfg.ConfigData)
                {
                    enemyConfig.RemoveInactiveSkins();
                }*/
                skinConfig.SyncConfiguration(cfg);
                applyDeactivatedSkins();
            }
        }

        internal static string[] GetStoredProfiles()
        {
            return skinConfig.GetProfiles();
        }

        internal static string StoreProfile(Profile tempProfile, string label)
        {
            Regex aplhaNumDash = new Regex("[^a-zA-Z0-9_-]");
            string sanitizedLabel = aplhaNumDash.Replace(label, "");
            if (string.IsNullOrEmpty(sanitizedLabel))
            {
                //if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError();
                throw new ProfileNameException();
            }
            else
            {
                skinConfig.StoreProfile(tempProfile, sanitizedLabel);
                return sanitizedLabel;
            }
        }

        internal static string OverwriteProfile(Profile tempProfile, string label)
        {
            Regex aplhaNumDash = new Regex("[^a-zA-Z0-9_-]");
            string sanitizedLabel = aplhaNumDash.Replace(label, "");
            if(string.IsNullOrEmpty(sanitizedLabel))
            {
                //if (LogLevelSetting >= LogLevel.ERROR) SkinLogger.LogError();
                throw new ProfileNameException();
            }
            else
            {
                skinConfig.OverwriteProfile(tempProfile, sanitizedLabel);
                return sanitizedLabel;
            }
        }

        internal static Profile GetProfile(string profile)
        {
            if (skinConfig.GetProfiles().Contains(profile))
            {
                return skinConfig.LoadProfile(profile);
            }
            throw new KeyNotFoundException();
        }

        internal static void StartConfigServer()
        {
            //SkinLogger.LogError($"Netmanager is null: {NetworkManager.Singleton==null}");
            //SkinLogger.LogError($"Is host: {NetworkManager.Singleton?.IsHost}");
            if (configServer != null && skinConfig!= null && !configServer.Running)
            {
                configServer.Start();
                configServer.BroadcastSyncMessage(skinConfig.GetWorkingProfile());
            }
        }

        internal static void StopConfigServer()
        {
            if (configServer != null && configServer.Running)
            {
                configServer.Stop();
            }
        }

        internal static void StartConfigClient()
        {
            if (configClient != null && !configClient.Running)
            {
                configClient.Start();
                configClient.SendSyncRequestToServer();
            }
        }

        internal static void StopConfigClient()
        {
            if(configClient != null && configClient.Running)
            {
                configClient.Stop();
                skinConfig.LoadLocalProfile();
            }
        }
    }

    public enum LogLevel
    {
        NONE,
        ERROR,
        WARN,
        INFO
    }

    public struct DefaultSkinConfigData
    {
        internal DefaultSkinConfigEntry[] defaultEntries;
        internal float defaultIndoorFrequency;
        internal float vanillaFallbackIndoorFrequency;
        internal float defaultOutdoorFrequency;
        internal float vanillaFallbackOutdoorFrequency;
        /// <summary>
        /// DefaultSkinConfigData Constructor. Only for use when registering a skin
        /// </summary>
        /// <param name="entries">all moon/frequency pairs for this default skin config</param>
        /// <param name="defaultOutdoorFrequency">the outdoor frequency of this skin on all non-configured moons</param>
        /// <param name="vanillafallbackOutdoor">the outdoor frequency to give the vanilla skin in the case that this default config generates a new map config for the relevant enemy</param>
        /// <param name="defaultIndoorFrequency">the indoor frequency of this skin on all non-configured moons</param>
        /// <param name="vanillafallbackIndoor">the indoor frequency to give the vanilla skin in the case that this default config generates a new map config for the relevant enemy</param>
        public DefaultSkinConfigData(DefaultSkinConfigEntry[] entries, float defaultOutdoorFrequency, float vanillafallbackOutdoor, float defaultIndoorFrequency, float vanillafallbackIndoor)
        {
            defaultEntries = entries;
            this.defaultOutdoorFrequency = defaultOutdoorFrequency;
            this.defaultIndoorFrequency = defaultIndoorFrequency;
            vanillaFallbackOutdoorFrequency = vanillafallbackOutdoor;
            vanillaFallbackIndoorFrequency = vanillafallbackIndoor;
        }

        /// <summary>
        /// DefaultSkinConfigData Constructor. Only for use when registering a skin
        /// </summary>
        /// <param name="entries">all moon/frequency pairs for this default skin config</param>
        /// <param name="defaultFrequency">the frequency of this skin on all non-configured moons</param>
        /// <param name="vanillafallback">the frequency to give the vanilla skin in the case that this default config generates a new map config for the relevant enemy</param>
        public DefaultSkinConfigData(DefaultSkinConfigEntry[] entries, float defaultFrequency = 1.0f, float vanillafallback = 0.0f)
        {
            defaultEntries = entries;
            defaultIndoorFrequency = defaultFrequency;
            defaultOutdoorFrequency = defaultFrequency;
            vanillaFallbackIndoorFrequency = vanillafallback;
            vanillaFallbackOutdoorFrequency = vanillafallback;
        }
    }

    class ProfileNameException : Exception
    {
        public ProfileNameException() : base("Skin names need at least one alphanumeric character, dash, or underscore") { }
    }


    public enum SpawnLocation
    {
        INDOOR,
        OUTDOOR
    }

    internal struct EnemyInfo
    {
        public string Name { get; }
        public string Id { get; }
        public SpawnLocation DefaultSpawnLocation { get; }

        public EnemyInfo(string name, string id, SpawnLocation spawnLocation)
        {
            Name = name;
            Id = id;
            DefaultSpawnLocation = spawnLocation;
        }
    }

    internal struct MoonInfo
    {
        public string Name { get; }
        public string Id { get; }

        public HashSet<string> Tags => tags;

        private HashSet<string> tags;

        internal MoonInfo(string name, string id, HashSet<string> tags)
        {
            Name = name;
            Id = id;
            this.tags = tags ?? new HashSet<string>();
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

    internal struct ModDiff
    {
        public string[] Skins { get; }
        public string[] Moons { get; }
        public string[] Enemies { get; }

        public ModDiff(string[] skins, string[] moons, string[] enemies)
        {
            Skins = skins;
            Moons = moons;
            Enemies = enemies;
        }
    }
}