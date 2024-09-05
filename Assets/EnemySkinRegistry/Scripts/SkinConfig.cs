using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DunGen;
using Unity.Collections;
using BepInEx;
using System.IO;

namespace AntlerShed.SkinRegistry
{
    class SkinConfig
    {
        //private const string WORKING_PROFILE_SECTION = "SkinConfig";

        private const string CONFIG_SUBDIR = "AntlerShed.EnemySkinRegistry";

        private const string CONFIG_FILE = "config.json";

        //private ConfigFile configFile;
        private ConfigConverter configConverter = new ConfigConverter();
        private ProfileConverter profileConverter = new ProfileConverter();
        //private EnemyConfigConverter enemyConfigConverter = new EnemyConfigConverter();
        private Dictionary<string, DefaultMapConfiguration> defaultMoonConfigs = new Dictionary<string, DefaultMapConfiguration>();
        internal Dictionary<string, DefaultMapConfiguration> DefaultMoonConfigs => new Dictionary<string, DefaultMapConfiguration>(defaultMoonConfigs);
        private Dictionary<string, DefaultSkinConfiguration> defaultSkinConfigs = new Dictionary<string, DefaultSkinConfiguration>();
        internal Dictionary<string, DefaultSkinConfiguration> DefaultSkinConfigs => new Dictionary<string, DefaultSkinConfiguration>(defaultSkinConfigs);
        private ConfigFile configFile;
        private Config config;
        private bool configInitialized = false;
        //private bool freshConfigFlag = false;
        internal ConfigEntry<LogLevel> LogLevelSetting { get; private set; }
        internal ConfigEntry<bool> AttemptSyncSetting { get; private set; }
        internal ConfigEntry<bool> AllowSyncSetting { get; private set; }
        internal ConfigEntry<bool> IndoorOutdoorSetting { get; private set; }
        //private IDictionary<string, MoonConfigEntry> configEntries;

        internal SkinConfig(ConfigFile bepinConfig)
        {
            if (!Directory.Exists(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR)))
            {
                Directory.CreateDirectory(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR));
                //freshConfigFlag = true;
            }

            //create config if it doesn't exist
            if (!File.Exists(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, CONFIG_FILE)))
            {
                writeConfig(Config.EmptyConfig());
            }
            config = readConfig();
            configFile = bepinConfig;
            LogLevelSetting = configFile.Bind("Logging", "LogLevel", LogLevel.ERROR, "Controls which types of debug messages are being printed. Might help get you useful info if you run into trouble.");
            AttemptSyncSetting = configFile.Bind("Profile Sync", "AttemptSync", false, "Set this flag to attempt to sync profiles with the host when running on a client.");
            bepinConfig.SettingChanged += OnSettingChanged;
            AllowSyncSetting = configFile.Bind("Profile Sync", "AllowSync", true, "Set this flag to allow clients to copy your working profile when you're hosting.");
            IndoorOutdoorSetting = configFile.Bind("Indoor and Outdoor Configuration", "IndoorOutdoor", false, "Set this flag to show additional controls for configuring indoor and outdoor spawn frequencies separately. Only relevant if you have a mod that allows enemies to spawn both inside and outside.");
        }

        private void OnSettingChanged(object sender, SettingChangedEventArgs e)
        {
            if(e.ChangedSetting.Definition.Key.Equals("AttemptSync") && e.ChangedSetting.Definition.Section.Equals("Profile Sync"))
            {
                if (AttemptSyncSetting.Value)
                {
                    EnemySkinRegistry.StartConfigClient();
                }
                else
                {
                    EnemySkinRegistry.StopConfigClient();
                }
            }
            else if (e.ChangedSetting.Definition.Key.Equals("AllowSync") && e.ChangedSetting.Definition.Section.Equals("Profile Sync"))
            {
                if (AttemptSyncSetting.Value)
                {
                    EnemySkinRegistry.StartConfigServer();
                }
                else
                {
                    EnemySkinRegistry.StopConfigServer();
                }
            }
        }

        private void writeConfig(Config config)
        {
            using (StreamWriter configFileWriter = new StreamWriter(File.Open(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, CONFIG_FILE), FileMode.Create)))
            {
                configFileWriter.WriteLine(JsonConvert.SerializeObject(config, Formatting.Indented, new JsonConverter[] { configConverter }));
            }
        }

        private Config readConfig()
        {
            Config config;
            using (StreamReader configFileReader = new StreamReader(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, CONFIG_FILE)))
            {
                config = JsonConvert.DeserializeObject<Config>(configFileReader.ReadToEnd(), new JsonConverter[] { configConverter });
            }
            return config;
        }

        /*private void SettingChanged(object sender, SettingChangedEventArgs e)
        {
        }*/

        /// <summary>
        /// Initializes the config file. Both creates the initial config file and adds entries for any new enemies/skins that have been added since the previous session.
        /// This also removes any config entries for skins/enemies/maps that are no longer installed
        /// </summary>
        /// <param name="loadedEnemies">List of enemies, both vanilla and modded, that have been loaded</param>
        /// <param name="loadedSkins">List of skins that have been loaded</param> //mmm... loaded skins...
        /// <param name="loadedMoons">List of loaded moon ids</param>
        internal void InitConfigForAllSkins(EnemyInfo[] loadedEnemies, Skin[] loadedSkins, string[] loadedMoons, HashSet<string> tags)
        {
            if (!configInitialized)
            {

                /*if (freshConfigFlag)
                {
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo("Porting over legacy configuration");
                    foreach (Skin skin in loadedSkins)
                    {
                        configFile.Bind(LEGACY_SKIN_BOOKKEEPING_SECTION, skin.Id, false);
                    }
                    foreach (string moonId in loadedMoons)
                    {
                        configFile.Bind(LEGACY_MOON_BOOKKEEPING_SECTION, moonId, false);
                    }
                    foreach (EnemyInfo info in loadedEnemies)
                    {
                        Skin[] enemySkins = loadedSkins.Where((skin) => skin.EnemyId.Equals(info.Id)).ToArray();
                        ConfigEntry<string> entry = configFile.Bind(LEGACY_SKIN_SECTION, info.Id, JsonConvert.SerializeObject(new EnemyConfiguration(info.Id, enemySkins), new JsonConverter[] { enemyConfigConverter }), $"Configuration data for the \"{info.Name}\" enemy.");
                    }
                    foreach (ConfigDefinition def in configFile.Keys.Where((ConfigDefinition def) => def.Section.Equals(LEGACY_SKIN_SECTION)))
                    {
                        config.WorkingProfile.AddEnemyConfiguration(JsonConvert.DeserializeObject<EnemyConfiguration>((string)configFile[def].BoxedValue, new JsonConverter[] { enemyConfigConverter }));
                    }
                    if (configFile.Keys.Any((ConfigDefinition def) => def.Section.Equals(LEGACY_SKIN_SECTION) || def.Section.Equals(LEGACY_SKIN_BOOKKEEPING_SECTION) || def.Section.Equals(LEGACY_MOON_BOOKKEEPING_SECTION)))
                    {
                        //Read old config file entries then delete them
                        foreach (ConfigDefinition def in configFile.Keys.Where((ConfigDefinition def) => def.Section.Equals(LEGACY_SKIN_SECTION)))
                        {
                            config.WorkingProfile.AddEnemyConfiguration(JsonConvert.DeserializeObject<EnemyConfiguration>((string)configFile[def].BoxedValue, new JsonConverter[] { enemyConfigConverter }));
                        }
                        config.KnownSkins.Union
                        (
                            configFile.Keys
                            .Where((ConfigDefinition def) => def.Section.Equals(LEGACY_SKIN_BOOKKEEPING_SECTION))
                            .Select((ConfigDefinition def) => def.Key)
                            .ToArray()
                        );
                        config.KnownMoons.Union
                        (
                            configFile.Keys
                            .Where((ConfigDefinition def) => def.Section.Equals(LEGACY_MOON_BOOKKEEPING_SECTION))
                            .Select((ConfigDefinition def) => def.Key)
                            .ToArray()
                        );
                    }
                }*/

                config.Init(loadedEnemies, loadedSkins, loadedMoons, tags.ToArray(), defaultSkinConfigs.Values.ToArray(), defaultMoonConfigs.Values.ToArray());
                writeConfig(config);
                configInitialized = true;

                /*if (freshConfigFlag)
                {
                    foreach(ConfigDefinition entry in configFile.Keys.Where((ConfigDefinition entry) => entry.Section == LEGACY_MOON_BOOKKEEPING_SECTION || entry.Section == LEGACY_SKIN_BOOKKEEPING_SECTION || entry.Section == LEGACY_SKIN_SECTION))
                    {
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"removing key \"{entry.Section}:{entry.Key}\"");
                        configFile.Remove(entry);
                    }
                    configFile.Save();
                    freshConfigFlag = false;
                }*/
            }
        }

        /// <summary>
        /// Gets the configured skin distribution for a given enemy spawning on a given moon
        /// </summary>
        /// <param name="moonId">The moon to get the distribution for</param>
        /// <param name="enemyType">The enemy type to get the distribution for</param>
        /// <returns>An alphabetically-sorted list of skin-distribution threshold pairs to pull from when assigning an enemy skin. 
        /// Distributions are cumulative for convenience when using an random number generator, so an even distribution of four skins will have thresholds 0.25, 0.5, 0.75, 1.0</returns>
        internal SkinDistribution GetConfiguredDistribution(string moonId, string[] moonTags, string enemyType, SpawnLocation location)
        {
            try
            {
                EnemyConfiguration enemyConfig = config.WorkingProfile.ConfigData.First((EnemyConfiguration cfg) => cfg.EnemyId.Equals(enemyType));
                string[] activeSkins = enemyConfig.ActiveSkins;
                MapConfiguration mapConfig = enemyConfig.MapConfigs.FirstOrDefault((MapConfiguration cfg) => cfg.Id.Equals(moonId)) ?? 
                    enemyConfig.TagConfig(moonTags) ??
                    enemyConfig.DefaultConfiguration;
       
                float activeSkinSum = mapConfig.Distribution.Aggregate(0.0f, (sum, skin) => sum += activeSkins.Contains(skin.SkinId) ? (location == SpawnLocation.INDOOR ? skin.IndoorFrequency : skin.OutdoorFrequency) : 0.0f);
                activeSkinSum += mapConfig.OutdoorVanillaFrequency;

                List<SkinThreshold> thresholdList = new List<SkinThreshold>();
                List<SkinConfigEntry> sortedSkins = mapConfig.Distribution.Where((skin) => activeSkins.Contains(skin.SkinId)).ToList();
                sortedSkins.Sort((a, b) => a.SkinId.CompareTo(b.SkinId));
                float runningTotal = 0.0f;
                if(activeSkinSum > 0.0)
                {
                    foreach (SkinConfigEntry skin in sortedSkins)
                    {
                        runningTotal += (location == SpawnLocation.INDOOR ? skin.IndoorFrequency : skin.OutdoorFrequency) / activeSkinSum;
                        thresholdList.Add(new SkinThreshold(skin.SkinId, runningTotal));
                    }
                    return new SkinDistribution(thresholdList.ToArray());
                }
                else
                {
                    return new SkinDistribution();
                }
            }
            catch(JsonSerializationException e)
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.ERROR) EnemySkinRegistry.SkinLogger.LogError(e.Message);
                return new SkinDistribution();
            }
        }

        internal void UpdateConfiguration(Profile profile)
        {
            config.WorkingProfile = profile;
            writeConfig(config);
        }

        internal void SyncConfiguration(Profile profile)
        {
            if(AttemptSyncSetting.Value)
            {
                config.WorkingProfile = profile;
            }
            else
            {
                if (LogLevelSetting.Value >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning("Attempted to synchronize config while server sync was inactive. If you're trying to update the config, consider calling UpdateConfiguration instead.");
            }
        }

        internal Profile GetWorkingProfile()
        {
            return config.WorkingProfile;
        }

        private DefaultMapConfiguration? GetDefaultMoonConfig(string moonId)
        {
            if (moonId!=null && defaultMoonConfigs.ContainsKey(moonId))
            {
                return defaultMoonConfigs[moonId];
            }
            return null;
        }

        private DefaultSkinConfiguration? GetDefaultSkinConfig(string skinId)
        {
            if (skinId != null && defaultSkinConfigs.ContainsKey(skinId))
            {
                return defaultSkinConfigs[skinId];
            }
            return null;
        }

        internal void AddDefaultMoonConfig(DefaultMapConfiguration config)
        {
            defaultMoonConfigs.Add(config.MoonId, config);
        }

        internal void ApplyDefaultMoonConfig(string moonId, Profile profile, string[] skins)
        {
            if(defaultMoonConfigs.ContainsKey(moonId))
            {
                profile.ApplyDefaultMoonConfig(defaultMoonConfigs[moonId], skins);
            }
        }

        internal void AddDefaultSkinConfig(DefaultSkinConfiguration config)
        {
            defaultSkinConfigs.Add(config.SkinId, config);
        }

        internal void ApplyDefaultSkinConfig(string skinId, Profile profile, string enemy, string[] moons, string[] tags)
        {
            if (defaultSkinConfigs.ContainsKey(skinId))
            {
                profile.ApplyDefaultSkinConfig(defaultSkinConfigs[skinId], enemy, moons, tags);
            }
        }

        internal string[] GetProfiles()
        {
            return Directory.EnumerateFiles(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR), "*.json")
                .Select((string filepath)=> Path.GetFileName(filepath))
                .Where((string filename) => !filename.Equals(CONFIG_FILE))
                .Select((string filename) => filename.Substring(0, filename.Length - 5))
                .ToArray();
        }

        internal Profile LoadProfile(string label)
        {
            Profile storedProfile;
            using (StreamReader profileFileReader = new StreamReader(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, label + ".json")))
            {
                storedProfile = JsonConvert.DeserializeObject<Profile>(profileFileReader.ReadToEnd(), new JsonConverter[] { profileConverter });
            }
            return storedProfile;
        }

        internal void StoreProfile(Profile profile, string label)
        {
            int counter = 0;
            string adjLabel = label;
            while(File.Exists(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, adjLabel + ".json")))
            {
                counter++;
                adjLabel = label + "_" + counter;
            }
            using (StreamWriter profileFileWriter = new StreamWriter(File.Open(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, adjLabel + ".json"), FileMode.Create)))
            {
                profileFileWriter.WriteLine(JsonConvert.SerializeObject(profile, Formatting.Indented, new JsonConverter[] { profileConverter }));
            }
        }

        internal void OverwriteProfile(Profile profile, string label)
        {
            using (StreamWriter profileFileWriter = new StreamWriter(File.Open(Path.Combine(Paths.ConfigPath, CONFIG_SUBDIR, label + ".json"), FileMode.Create)))
            {
                profileFileWriter.WriteLine(JsonConvert.SerializeObject(profile, Formatting.Indented, new JsonConverter[] { profileConverter }));
            }
        }

        internal void LoadLocalProfile()
        {
            config = readConfig();
        }
    }

    public struct DefaultMapConfiguration
    {
        /// <summary>
        /// list of enemy-specific configurations for this moon
        /// </summary>
        private DefaultMapConfigEntry[] enemyConfigs;
        public DefaultMapConfigEntry[] EnemyConfigs => enemyConfigs.ToArray();

        /// <summary>
        /// the id of the moon this config applies to
        /// </summary>
        public string MoonId { get; }

        public DefaultMapConfiguration(string moonId, DefaultMapConfigEntry[] enemyConfigs)
        {
            MoonId = moonId;
            this.enemyConfigs = enemyConfigs;
        }
    }

    public struct DefaultMapConfigEntry
    {
        /// <summary>
        /// The id of the enemy for this configuration entry
        /// </summary>
        public string EnemyId { get; }

        /// <summary>
        /// The frequency at which the vanilla appearance will spawn
        /// </summary>
        public float IndoorVanillaFrequency { get; }
        /// <summary>
        /// The frequency at which the vanilla appearance will spawn
        /// </summary>
        public float OutdoorVanillaFrequency { get; }


        private SkinConfigEntry[] distribution;
        /// <summary>
        /// Skin to spawn frequency pairs that describe how often skins will spawn inside
        /// </summary>
        public SkinConfigEntry[] Distribution => distribution.ToArray();

        /// <summary>
        /// DefaultMapConfigEntry Constructor
        /// </summary>
        /// <param name="enemyId">the id of the enemy this entry will apply to</param>
        /// <param name="vanillaFrequency">the frequency at which the vanilla appearance will spawn</param>
        /// <param name="distribution">all specific skin/frequency pairs that make up the config entry</param>
        public DefaultMapConfigEntry(string enemyId, float vanillaFrequency, SkinConfigEntry[] distribution)
        {
            EnemyId = enemyId;
            OutdoorVanillaFrequency = vanillaFrequency;
            IndoorVanillaFrequency = vanillaFrequency;
            this.distribution = distribution;
        }

        /// <summary>
        /// DefaultMapConfigEntry Constructor
        /// </summary>
        /// <param name="enemyId">the id of the enemy this entry will apply to</param>
        /// <param name="indoorVanillaFrequency">the frequency at which the vanilla appearance will spawn</param>
        /// <param name="outdoorVanillaFrequency">the frequency at which the vanilla appearance will spawn</param>
        /// <param name="distribution">all specific skin/frequency pairs that make up the config entry</param>
        public DefaultMapConfigEntry(string enemyId, float outdoorVanillaFrequency, float indoorVanillaFrequency, SkinConfigEntry[] distribution)
        {
            EnemyId = enemyId;
            OutdoorVanillaFrequency = outdoorVanillaFrequency;
            IndoorVanillaFrequency = indoorVanillaFrequency;
            this.distribution = distribution;
        }
    }

    public struct DefaultSkinConfiguration
    {
        public string SkinId { get; }
        private DefaultSkinConfigEntry[] defaultEntries;
        public DefaultSkinConfigEntry[] DefaultEntries => defaultEntries.ToArray();
        public float OutdoorDefaultFrequency { get; }

        public float IndoorDefaultFrequency { get; }
        public float OutdoorVanillaFallbackFrequency { get; }
        
        public float IndoorVanillaFallbackFrequency { get; }

        /// <summary>
        /// Default skin config constructor
        /// </summary>
        /// <param name="skinId">the id of the skin</param>
        /// <param name="entries">all moon/frequency pairs for this default skin config</param>
        /// <param name="defaultFrequency">the frequency of this skin on all non-configured moons</param>
        /// <param name="vanillafallback">the frequency to give the vanilla skin in the case that this default config generates a new map config for the relevant enemy</param>
        public DefaultSkinConfiguration(string skinId, DefaultSkinConfigEntry[] entries, float defaultFrequency = 1.0f, float vanillafallback = 0.0f)
        {
            SkinId = skinId;
            defaultEntries = entries;
            OutdoorDefaultFrequency = defaultFrequency;
            IndoorDefaultFrequency = defaultFrequency;
            OutdoorVanillaFallbackFrequency = vanillafallback;
            IndoorVanillaFallbackFrequency = vanillafallback;
        }

        public DefaultSkinConfiguration(string skinId, DefaultSkinConfigEntry[] entries, float defaultOutdoorFrequency, float vanillafallbackOutdoor, float defaultIndoorFrequency, float vanillafallbackIndoor)
        {
            SkinId = skinId;
            defaultEntries = entries;
            OutdoorDefaultFrequency = defaultOutdoorFrequency;
            IndoorDefaultFrequency = defaultIndoorFrequency;
            OutdoorVanillaFallbackFrequency = vanillafallbackOutdoor;
            IndoorVanillaFallbackFrequency = vanillafallbackIndoor;
        }
    }

    public struct DefaultSkinConfigEntry
    {
        public string MoonId { get; }
        public float IndoorFrequency { get; }
        public float OutdoorFrequency { get; }
        internal bool LocationSet { get; }
        public DefaultSkinConfigEntry(string moonId, float frequency)
        {
            MoonId = moonId;
            IndoorFrequency = frequency;
            OutdoorFrequency = frequency;
            LocationSet = true;
        }

        public DefaultSkinConfigEntry(string moonId, float outdoorFrequency, float indoorFrequency)
        {
            MoonId = moonId;
            OutdoorFrequency = outdoorFrequency;
            IndoorFrequency = indoorFrequency;
            LocationSet = false;
        }
    }

    /// <summary>
    /// All the stuff that has to persist
    /// </summary>
    public class Config
    {
        private HashSet<string> knownSkins;
        private HashSet<string> knownMoons;

        //These three entries really have no business sticking around after init, but I'll leave them here for now
        public HashSet<string> KnownSkins => knownSkins.ToHashSet();
        public HashSet<string> KnownMoons => knownMoons.ToHashSet();
        public string Version{ get; }
        
        internal Profile WorkingProfile { get; set; }

        public Config(string[] knownSkins, string[] knownMoons, string version, Profile workingProfile)
        {
            this.knownSkins = knownSkins.ToHashSet();
            this.knownMoons = knownMoons.ToHashSet();
            Version = version;
            WorkingProfile = workingProfile;
        }

        public static Config EmptyConfig()
        {
            return new Config
            (
                new string[0],
                new string[0],
                EnemySkinRegistry.modVersion,
                Profile.CreateDefaultProfile(new string[0], new Skin[0])
            );
        }

        internal void Init
        (
            EnemyInfo[] loadedEnemies, 
            Skin[] loadedSkins, 
            string[] loadedMoons,
            string[] loadedTags,
            DefaultSkinConfiguration[] defaultSkinConfigs, 
            DefaultMapConfiguration[] defaultMapConfigs
        )
        {
            //Remove anything that's old
            WorkingProfile.SyncWithLoadedMods(loadedEnemies, loadedSkins, loadedMoons, loadedTags);
            
            foreach (DefaultSkinConfiguration defaultSkinConfig in defaultSkinConfigs)
            {
                if(!knownSkins.Contains(defaultSkinConfig.SkinId))
                {
                    if(loadedSkins.Any((Skin skin)=>skin.Id.Equals(defaultSkinConfig.SkinId)))
                    {
                        WorkingProfile.ApplyDefaultSkinConfig(defaultSkinConfig, loadedSkins.First((Skin skin) => skin.Id.Equals(defaultSkinConfig.SkinId)).EnemyId, loadedMoons, loadedTags);
                    }
                    else
                    {
                        if(EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"No skin found for default skin configuration with id \"{defaultSkinConfig.SkinId}\"");
                    }
                }
            }
            string[] loadedSkinIds = loadedSkins.Select((skin) => skin.Id).ToArray();
            foreach (DefaultMapConfiguration defaultMapConfig in defaultMapConfigs)
            {
                if(!knownMoons.Contains(defaultMapConfig.MoonId) && loadedMoons.Contains(defaultMapConfig.MoonId))
                {
                    WorkingProfile.ApplyDefaultMoonConfig(defaultMapConfig, loadedSkinIds);
                }
            }
            //Set any non-configured new skins to max weight
            WorkingProfile.AddNewSkinsToDefaultMap(loadedSkins.Where((Skin skin) => !knownSkins.Any( (string id) => skin.Id.Equals(id) ) && !defaultSkinConfigs.Any((defSkin) => skin.Id.Equals(defSkin.SkinId))).ToArray() );
            knownSkins = loadedSkins.Select((Skin skin) => skin.Id).ToHashSet();
            knownMoons = loadedMoons.ToHashSet();
        }
    }

    /// <summary>
    /// All the stuff that server sync affects
    /// </summary>
    public class Profile
    {
        public EnemyConfiguration[] ConfigData => configData.ToArray();
        private List<EnemyConfiguration> configData;

        internal Profile(EnemyConfiguration[] configData)
        {
            this.configData = configData.ToList();
            this.configData.Sort((a,b) => a.EnemyId.CompareTo(b.EnemyId));
        }

        internal EnemyConfiguration GetEnemyConfig(string enemyId)
        {
            if(configData.Any((EnemyConfiguration cfg) => cfg.EnemyId.Equals(enemyId)))
            {
                return configData.First((EnemyConfiguration cfg) => cfg.EnemyId.Equals(enemyId));
            }
            return null;
        }

        internal void AddEnemyConfiguration(EnemyConfiguration enemyConfig)
        {
            configData.RemoveAll((config)=>config.EnemyId.Equals(enemyConfig.EnemyId));
            configData.Add(enemyConfig);
            configData.Sort((a, b) => a.EnemyId.CompareTo(b.EnemyId));
        }

        internal static Profile CreateDefaultProfile(string[] enemies, Skin[] availableSkins)
        {
            return new Profile
            (
                enemies.Select
                (
                    (string enemy) => new EnemyConfiguration
                    (
                        enemy,
                        availableSkins.Where((Skin skin) => skin.EnemyId.Equals(enemy)).ToArray()
                    )
                ).ToArray()
            );
        }

        internal void SyncWithLoadedMods(EnemyInfo[] loadedEnemies, Skin[] loadedSkins, string[] loadedMoons, string[] loadedTags)
        {
            foreach(EnemyConfiguration enemyConfig in configData.Where((EnemyConfiguration config) => !loadedEnemies.Any((EnemyInfo info) => info.Id.Equals(config.EnemyId))))
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Removing Enemy with id \"{enemyConfig.EnemyId}\" from config because the mod is no longer present");
            }
            configData.RemoveAll((EnemyConfiguration config)=>!loadedEnemies.Any((EnemyInfo info) => info.Id.Equals(config.EnemyId)));
            foreach (EnemyInfo info in loadedEnemies)
            {
                if (!configData.Any((EnemyConfiguration config) => config.EnemyId.Equals(info.Id)))
                {
                    configData.Add(new EnemyConfiguration(info.Id, loadedSkins));
                }
            }
            foreach (EnemyConfiguration config in configData)
            {
                config.SyncWithLoadedMods(loadedSkins, loadedMoons, loadedTags);
            }
        }

        internal void ApplyDefaultMoonConfig(DefaultMapConfiguration moonConfig, string[] skins)
        {
            
            foreach(DefaultMapConfigEntry entry in moonConfig.EnemyConfigs)
            {
                foreach(EnemyConfiguration config in configData)
                {
                    //Apply default map config entry will just skip if it doesn't have the same enemy id, so I don't bother checking twice
                    config.ApplyDefaultMoonConfig(entry, moonConfig.MoonId, skins);
                }
            }
        }

        internal void ApplyDefaultSkinConfig(DefaultSkinConfiguration skinConfig, string enemy, string[] installedMoons, string[] moonTags)
        {
            if(ConfigData.Any((EnemyConfiguration cfg)=> cfg.EnemyId.Equals(enemy)))
            {
                ConfigData.First((EnemyConfiguration cfg) => cfg.EnemyId.Equals(enemy)).ApplyDefaultSkinConfig(skinConfig, installedMoons, moonTags);
            }
        }

        internal Profile Copy()
        {
            return new Profile(configData.Select((EnemyConfiguration cfg)=>cfg.Copy()).ToArray());
        }

        internal void AddNewSkinsToDefaultMap(Skin[] newSkins)
        {
            foreach(Skin skin in newSkins)
            {
                foreach(EnemyConfiguration configuration in configData)
                {
                    if(configuration.EnemyId.Equals(skin.EnemyId))
                    {
                        configuration.AddNewSkinToDefaultMap(skin.Id);
                    }
                }
            }
        }
    }

    public class EnemyConfiguration
    {
        public string EnemyId { get; }
        public string[] ActiveSkins => activeSkins.ToArray();
        private HashSet<string> activeSkins;
        public MapConfiguration DefaultConfiguration { get; }
        public MapConfiguration[] MapConfigs => mapConfigs.ToArray();
        private List<MapConfiguration> mapConfigs;

        public EnemyConfiguration(string enemyType, MapConfiguration defaultConfiguration, MapConfiguration[] mapConfigs, string[] active)
        {
            EnemyId = enemyType;
            DefaultConfiguration = defaultConfiguration;
            this.mapConfigs = mapConfigs.ToList();
            this.mapConfigs.Sort((a, b) => a.Id.CompareTo(b.Id));
            activeSkins = active.ToHashSet();
        }

        internal EnemyConfiguration(string enemyType, Skin[] availableSkins) : this(enemyType, new MapConfiguration(0.0f, 0.0f, availableSkins.Where((skin)=>skin.EnemyId.Equals(enemyType)).Select((skin) => new SkinConfigEntry(1.0f, skin.Id)).ToArray()), new MapConfiguration[] { }, availableSkins.Where((skin) => skin.EnemyId.Equals(enemyType)).Select((skin) => skin.Id).ToArray()) { }

        internal void ActivateSkin(Skin skin)
        {
            if(skin.EnemyId.Equals(EnemyId))
            {
                activeSkins.Add(skin.Id);
            }
        }

        internal void DeactivateSkin(string skinId)
        {
            activeSkins.Remove(skinId);
        }

        internal MapConfiguration TagConfig(string[] tags)
        {
            float vanillaOutdoor = 0.0f;
            float vanillaIndoor = 0.0f;
            Dictionary<string, float> skinIndoor = new Dictionary<string, float>();
            Dictionary<string, float> skinOutdoor = new Dictionary<string, float>();
            int numEntries = 0;
            foreach(string tag in tags)
            {
                MapConfiguration config = mapConfigs.FirstOrDefault((MapConfiguration config) => config.Id.Equals(tag));
                if(config != null)
                {
                    vanillaIndoor += config.IndoorVanillaFrequency;
                    vanillaOutdoor += config.OutdoorVanillaFrequency;
                    foreach (SkinConfigEntry entry in config.Distribution)
                    {
                        if (!skinIndoor.ContainsKey(entry.SkinId))
                        {
                            skinIndoor.Add(entry.SkinId, 0.0f);
                            skinOutdoor.Add(entry.SkinId, 0.0f);
                        }
                        skinIndoor[entry.SkinId] = skinIndoor[entry.SkinId] + entry.IndoorFrequency;
                        skinOutdoor[entry.SkinId] = skinOutdoor[entry.SkinId] + entry.OutdoorFrequency;
                    }
                    numEntries++;
                }
            }
            if(numEntries > 0)
            {
                return new MapConfiguration("tempTags", vanillaOutdoor / numEntries, vanillaIndoor / numEntries, skinIndoor.Keys.Select((string skin) => new SkinConfigEntry(skinIndoor[skin] / numEntries, skinOutdoor[skin] / numEntries, skin)).ToArray());
            }
            else
            {
                return null;
            }
            
        }

        /*internal void RemoveInactiveSkins()
        {
            foreach(MapConfiguration config in mapConfigs)
            {
                foreach(SkinConfigEntry entry in config.Distribution)
                {
                    if(activeSkins.Contains(entry.SkinId))
                    {
                        config.RemoveEntry(entry.SkinId);
                    }
                }
            }
        }*/

        internal void AddMoon(string moonId)
        {
            mapConfigs.RemoveAll((MapConfiguration stored) => stored.Id.Equals(moonId));
            mapConfigs.Add(new MapConfiguration(moonId, 1.0f, 1.0f, new SkinConfigEntry[]{}));
            mapConfigs.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        internal void RemoveMoon(string moonId)
        {
            mapConfigs.RemoveAll((MapConfiguration stored) => stored.Id.Equals(moonId));
        }

        internal void SyncWithLoadedMods(Skin[] loadedSkins, string[] loadedMoons, string[] loadedTags)
        {
            activeSkins = activeSkins.Intersect(loadedSkins.Select(skin=>skin.Id)).ToHashSet();
            //activeSkins.RemoveWhere((string active) => !loadedSkins.Any((Skin skin) => active.Equals(skin.Id)));
            foreach (MapConfiguration mapConfig in mapConfigs.Where((MapConfiguration cfg) => !loadedMoons.Any((string moonId) => moonId.Equals(cfg.Id)) && !loadedTags.Any((string tagId) => tagId.Equals(cfg.Id))))
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Removing Moon or Tag with id \"{mapConfig.Id}\" from config because the mod it's from is no longer present");
            }
            DefaultConfiguration.RemoveStaleSkins(loadedSkins);
            mapConfigs.RemoveAll((MapConfiguration cfg) => !loadedMoons.Any((string moonId) => moonId.Equals(cfg.Id)) && !loadedTags.Any((string tagId) => tagId.Equals(cfg.Id)));
            foreach (MapConfiguration cfg in mapConfigs)
            {
                cfg.RemoveStaleSkins(loadedSkins);
            }
        }

        internal void ApplyDefaultMoonConfig(DefaultMapConfigEntry entry, string moonId, string[] skins)
        {
            if (entry.EnemyId.Equals(EnemyId))
            {
                if(!mapConfigs.Any((cfg)=>cfg.Id.Equals(moonId)))
                {
                    mapConfigs.Add(new MapConfiguration(moonId, 0.0f, 0.0f, new SkinConfigEntry[] { }));
                }
                mapConfigs.First((cfg) => cfg.Id.Equals(moonId)).ApplyDefaultConfig(entry, skins);
            }
        }

        internal MapConfiguration GetMoonConfig(string moonId)
        {
            if(mapConfigs.Any((MapConfiguration cfg) => cfg.Id.Equals(moonId)))
            {
                return mapConfigs.First((MapConfiguration cfg) => cfg.Id.Equals(moonId));
            }
            return null;
        }

        internal void ApplyDefaultSkinConfig(DefaultSkinConfiguration skinConfig, string[] installedMoons, string[] moonTags)
        {
            activeSkins.Add(skinConfig.SkinId);
            DefaultConfiguration.AddEntry(new SkinConfigEntry(skinConfig.OutdoorDefaultFrequency, skinConfig.IndoorDefaultFrequency, skinConfig.SkinId));
            foreach (DefaultSkinConfigEntry entry in skinConfig.DefaultEntries)
            {
                if (!installedMoons.Contains(entry.MoonId) && !moonTags.Contains(entry.MoonId))
                {
                    EnemySkinRegistry.SkinLogger.LogWarning($"Moon or Tag \"{entry.MoonId}\" listed in default skin config for \"{skinConfig.SkinId}\" is not installed. Skipping.");
                }
                else
                {
                    if (mapConfigs.Any((MapConfiguration cfg) => cfg.Id.Equals(entry.MoonId)))
                    {
                        mapConfigs.First((MapConfiguration cfg) => cfg.Id.Equals(entry.MoonId)).ApplyDefaultSkinConfigEntry(entry, skinConfig.SkinId);
                    }
                    else
                    {
                        mapConfigs.Add
                        (
                            new MapConfiguration
                            (
                                entry.MoonId,
                                skinConfig.OutdoorVanillaFallbackFrequency,
                                skinConfig.IndoorVanillaFallbackFrequency,
                                new SkinConfigEntry[] { new SkinConfigEntry(entry.OutdoorFrequency, entry.IndoorFrequency, skinConfig.SkinId) }
                            )
                        );
                    }
                }
            }
            mapConfigs.Sort((a, b) => a.Id.CompareTo(b.Id));
        }

        internal EnemyConfiguration Copy()
        {
            return new EnemyConfiguration
            (
                EnemyId, 
                DefaultConfiguration.Copy(), 
                mapConfigs.Select((MapConfiguration cfg) => cfg.Copy()).ToArray(), 
                activeSkins.ToArray()
            );
        }

        internal void AddNewSkinToDefaultMap(string id)
        {
            activeSkins.Add(id);
            DefaultConfiguration.AddEntry(new SkinConfigEntry(1.0f, id));
        }
    }

    public class MapConfiguration
    {
        public string Id { get; }
        private List<SkinConfigEntry> distribution;
        public SkinConfigEntry[] Distribution => distribution.ToArray();

        public float OutdoorVanillaFrequency
        {
            get => outdoorVanillaFrequency;
            set
            {
                outdoorVanillaFrequency = Math.Clamp(value, 0.0f, 1.0f);
            }
        }
        private float outdoorVanillaFrequency;

        public float IndoorVanillaFrequency
        {
            get => indoorVanillaFrequency;
            set
            {
                indoorVanillaFrequency = Math.Clamp(value, 0.0f, 1.0f);
            }
        }
        private float indoorVanillaFrequency;

        public MapConfiguration(string id, float outdoorVanillaFrequency, float indoorVanillaFrequency, SkinConfigEntry[] distribution)
        {
            Id = id;
            this.distribution = distribution.ToList();
            this.indoorVanillaFrequency = Math.Clamp(indoorVanillaFrequency, 0.0f, 1.0f);
            this.outdoorVanillaFrequency = Math.Clamp(outdoorVanillaFrequency, 0.0f, 1.0f);
            this.distribution.Sort((a, b) => a.SkinId.CompareTo(b.SkinId));
        }

        //public MapConfiguration(string id, float vanillaFrequency, SkinConfigEntry[] distribution) : this(id, vanillaFrequency, distribution, distribution) { }

        internal MapConfiguration(float outdoorVanillaFrequency, float indoorVanillaFrequency, SkinConfigEntry[] distribution) : this("default", outdoorVanillaFrequency, indoorVanillaFrequency, distribution) { }

        //internal MapConfiguration(float vanillaFrequency, SkinConfigEntry[] distribution) : this(vanillaFrequency, distribution, distribution) { }
        internal void AddEntry(SkinConfigEntry entry)
        {
            distribution.RemoveAll((SkinConfigEntry stored) => stored.SkinId.Equals(entry.SkinId));
            distribution.Add(entry);
            distribution.Sort((a, b) => a.SkinId.CompareTo(b.SkinId));
        }

        internal void RemoveEntry(string skinId)
        {
            distribution.RemoveAll((SkinConfigEntry stored) => stored.SkinId.Equals(skinId));
        }

        internal void RemoveStaleSkins(Skin[] availableSkins)
        {
            foreach(SkinConfigEntry skinConfig in distribution.Where((SkinConfigEntry cfg) => !availableSkins.Any((Skin skin) => skin.Id.Equals(cfg.SkinId))))
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Removing Skin with id \"{skinConfig.SkinId}\" from config because the skin is not installed");
            }
            distribution.RemoveAll((SkinConfigEntry cfg) => !availableSkins.Any((Skin skin) => skin.Id.Equals(cfg.SkinId)));
        }

        internal void ApplyDefaultConfig(DefaultMapConfigEntry entry, string[] skins)
        {
            foreach(SkinConfigEntry skinFreq in entry.Distribution.Where((skinFreq) => !skins.Contains(skinFreq.SkinId)))
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Skipping skin with id\"{skinFreq.SkinId}\" in default configuration of moon \"{Id}\" because the skin is not installed.");
            }
            distribution = entry.Distribution.Where((skinFreq)=>skins.Contains(skinFreq.SkinId)).ToList();
            OutdoorVanillaFrequency = entry.OutdoorVanillaFrequency;
            IndoorVanillaFrequency = entry.IndoorVanillaFrequency;
        }

        internal void ApplyDefaultSkinConfigEntry(DefaultSkinConfigEntry entry, string skinId)
        {
            if (entry.MoonId.Equals(Id))
            {
                AddEntry(new SkinConfigEntry(entry.OutdoorFrequency, entry.IndoorFrequency, skinId));
            }
        }

        internal MapConfiguration Copy()
        {
            return new MapConfiguration
            (
                Id,
                OutdoorVanillaFrequency,
                IndoorVanillaFrequency,
                Distribution.Select((SkinConfigEntry entry) => entry.Copy()).ToArray()
            );
        }

        internal void SetIndoorSkinFrequency(string skinId, float frequency)
        {
            if (distribution.Any((SkinConfigEntry entry) => entry.SkinId.Equals(skinId)))
            {
                SkinConfigEntry entry = distribution.First((SkinConfigEntry entry) => entry.SkinId.Equals(skinId));
                entry.IndoorFrequency = frequency;
            }
        }

        internal void SetOutdoorSkinFrequency(string skinId, float frequency)
        {
            if (distribution.Any((SkinConfigEntry entry) => entry.SkinId.Equals(skinId)))
            {
                SkinConfigEntry entry = distribution.First((SkinConfigEntry entry) => entry.SkinId.Equals(skinId));
                entry.OutdoorFrequency = frequency;
            }
        }
    }

    public class SkinConfigEntry
    {
        private float outdoorFrequency;
        public float OutdoorFrequency
        {
            get { return outdoorFrequency; }
            set { outdoorFrequency = Math.Clamp(value, 0.0f, 1.0f); }
        }
        private float indoorFrequency;
        public float IndoorFrequency 
        {
            get 
            { 
                return indoorFrequency; 
            }
            set 
            { 
                indoorFrequency = Math.Clamp(value, 0.0f, 1.0f); 
            }
        }
        public string SkinId { get; }

        /// <summary>
        /// Skin Config Entry Constructor
        /// </summary>
        /// <param name="frequency">the frequency at which a skin will spawn when considering all other relevant skins</param>
        /// <param name="skinId">the unique id of the skin</param>
        internal SkinConfigEntry(float frequency, string skinId)
        {
            outdoorFrequency = Math.Clamp(frequency, 0.0f, 1.0f);
            indoorFrequency = Math.Clamp(frequency, 0.0f, 1.0f);
            SkinId = skinId;
        }

        internal SkinConfigEntry(float outdoorFrequency, float indoorFrequency, string skinId)
        {
            this.indoorFrequency = Math.Clamp(indoorFrequency, 0.0f, 1.0f);
            this.outdoorFrequency = Math.Clamp(outdoorFrequency, 0.0f, 1.0f);
            SkinId = skinId;
        }

        internal SkinConfigEntry Copy()
        {
            return new SkinConfigEntry(OutdoorFrequency, IndoorFrequency, SkinId);
        }
    }

    public class ConfigConverter : JsonConverter<Config>
    {
        public override Config ReadJson(JsonReader reader, Type objectType, Config existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = (JObject)JToken.ReadFrom(reader);
            return ConfigJsonSerialization.DeserializeConfig(obj);
        }

        public override void WriteJson(JsonWriter writer, Config value, JsonSerializer serializer)
        {
            
            ConfigJsonSerialization.SerializeConfig(value).WriteTo(writer);
        }
    }

    public class ProfileConverter : JsonConverter<Profile>
    {
        public override Profile ReadJson(JsonReader reader, Type objectType, Profile existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            
            JObject obj = (JObject)JToken.ReadFrom(reader);
            return ConfigJsonSerialization.DeserializeProfile(obj);
        }

        public override void WriteJson(JsonWriter writer, Profile value, JsonSerializer serializer)
        {
            ConfigJsonSerialization.SerializeProfile(value).WriteTo(writer);
        }
    }

    public class EnemyConfigConverter: JsonConverter<EnemyConfiguration>
    {
        public override void WriteJson(JsonWriter writer, EnemyConfiguration value, JsonSerializer serializer)
        {
            ConfigJsonSerialization.SerializeEnemyConfiguration(value).WriteTo(writer);
        }

        public override EnemyConfiguration ReadJson(JsonReader reader, Type objectType, EnemyConfiguration existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = (JObject)JToken.ReadFrom(reader);
            return ConfigJsonSerialization.DeserializeEnemyConfiguration(obj);
        }
    }

    internal class ConfigJsonSerialization
    {
        //Skin freq
        private const string SKIN_ID_KEY = "skinId";
        private const string INDOOR_FREQUENCY_KEY = "indoorFrequency";
        private const string OUTDOOR_FREQUENCY_KEY = "outdoorFrequency";
        private const string LEGACY_FREQUENCY_KEY = "frequency";

        //Enemy
        private const string ENEMY_ID_KEY = "enemyId";
        private const string ACTIVE_KEY = "activeSkins";
        private const string DEFAULT_KEY = "defaultConfiguration";
        private const string MAPS_KEY = "mapConfigurations";

        //Map
        private const string MAP_ID_KEY = "mapId";
        private const string LEGACY_VANILLA_KEY = "vanillaFrequency";
        private const string OUTDOOR_VANILLA_KEY = "outdoorVanillaFrequency";
        private const string INDOOR_VANILLA_KEY = "indoorVanillaFrequency";
        private const string DISTRIBUTION_KEY = "skinDistribution";

        //Profile
        private const string ENEMY_CONFIGS_KEY = "enemyConfigs";

        //Config
        //private const string LOG_KEY = "logLevel";
        //private const string SYNC_KEY = "syncWithServer";
        private const string VERSION_KEY = "version";
        private const string PROFILE_KEY = "mapConfigurations";

        private const string SKIN_BOOKKEEPING_KEY = "knownSkins";
        private const string MOON_BOOKKEEPING_KEY = "knownMoons";

        internal static JObject SerializeSkinConfig(SkinConfigEntry value)
        {
            JObject obj = new JObject();
            obj[SKIN_ID_KEY] = value.SkinId;
            obj[INDOOR_FREQUENCY_KEY] = value.IndoorFrequency;
            obj[OUTDOOR_FREQUENCY_KEY] = value.OutdoorFrequency;
            return obj;
        }

        internal static SkinConfigEntry DeserializeSkinConfig(JObject obj)
        {
            if(obj.ContainsKey(LEGACY_FREQUENCY_KEY))
            {
                return new SkinConfigEntry
                (
                    (float)obj[LEGACY_FREQUENCY_KEY],
                    (float)obj[LEGACY_FREQUENCY_KEY],
                    (string)obj[SKIN_ID_KEY]
                );
            }
            else
            {
                return new SkinConfigEntry
                (

                    (float)obj[OUTDOOR_FREQUENCY_KEY],
                    (float)obj[INDOOR_FREQUENCY_KEY],
                    (string)obj[SKIN_ID_KEY]
                );
            }
            
        }

        public static EnemyConfiguration DeserializeEnemyConfiguration(JObject obj)
        {
            return new EnemyConfiguration
            (
                (string)obj[ENEMY_ID_KEY],
                DeserializeMapConfig((JObject)obj[DEFAULT_KEY]),
                obj[MAPS_KEY].ToArray().Select((token) => DeserializeMapConfig((JObject)token)).ToArray(),
                obj[ACTIVE_KEY].ToArray().Select((token) => (string)token).ToArray()
            );
        }

        public static JObject SerializeEnemyConfiguration(EnemyConfiguration value)
        {
            JObject obj = new JObject();
            obj[ENEMY_ID_KEY] = value.EnemyId;
            obj[ACTIVE_KEY] = new JArray(value.ActiveSkins);
            obj[DEFAULT_KEY] = SerializeMapConfig(value.DefaultConfiguration);
            obj[MAPS_KEY] = new JArray(value.MapConfigs.Select((cfg) => SerializeMapConfig(cfg)));
            return obj;
        }

        private static JObject SerializeMapConfig(MapConfiguration value)
        {
            JObject obj = new JObject();
            obj[MAP_ID_KEY] = value.Id.Equals("default") ? "" : value.Id;
            obj[OUTDOOR_VANILLA_KEY] = value.OutdoorVanillaFrequency;
            obj[INDOOR_VANILLA_KEY] = value.IndoorVanillaFrequency;
            obj[DISTRIBUTION_KEY] = new JArray(value.Distribution.Select((cfg) => SerializeSkinConfig(cfg)));
            return obj;
        }

        private static MapConfiguration DeserializeMapConfig(JObject obj)
        {
            return string.IsNullOrEmpty((string)obj[MAP_ID_KEY]) ?
                obj.ContainsKey(LEGACY_VANILLA_KEY) ?
                    new MapConfiguration
                    (
                        (float)obj[LEGACY_VANILLA_KEY],
                        (float)obj[LEGACY_VANILLA_KEY],
                        obj[DISTRIBUTION_KEY].ToArray().Select((token) => DeserializeSkinConfig((JObject)token)).ToArray()
                    ) :
                    new MapConfiguration
                    (
                        (float)obj[OUTDOOR_VANILLA_KEY],
                        (float)obj[INDOOR_VANILLA_KEY],
                        obj[DISTRIBUTION_KEY].ToArray().Select((token) => DeserializeSkinConfig((JObject)token)).ToArray()
                    )
                :
                obj.ContainsKey(LEGACY_VANILLA_KEY) ?
                    new MapConfiguration
                    (
                        (string)obj[MAP_ID_KEY],
                        (float)obj[LEGACY_VANILLA_KEY],
                        (float)obj[LEGACY_VANILLA_KEY],
                        obj[DISTRIBUTION_KEY].ToArray().Select((token) => DeserializeSkinConfig((JObject)token)).ToArray()
                    ) :
                    new MapConfiguration
                    (
                        (string)obj[MAP_ID_KEY],
                        (float)obj[OUTDOOR_VANILLA_KEY],
                        (float)obj[INDOOR_VANILLA_KEY],
                        obj[DISTRIBUTION_KEY].ToArray().Select((token) => DeserializeSkinConfig((JObject)token)).ToArray()
                    );
                
        }

        public static JObject SerializeProfile(Profile value)
        {
            JObject obj = new JObject();
            obj[ENEMY_CONFIGS_KEY] = new JArray(value.ConfigData.Select((cfg) => SerializeEnemyConfiguration(cfg)));
            return obj;
        }

        public static Profile DeserializeProfile(JObject obj)
        {
            return new Profile
            (
                obj[ENEMY_CONFIGS_KEY].ToArray().Select((token) => DeserializeEnemyConfiguration(token as JObject)).ToArray()
            );
        }

        internal static Config DeserializeConfig(JObject obj)
        {
            return new Config
            (
                obj[SKIN_BOOKKEEPING_KEY].ToArray().Select((token) => (string)token).ToArray(),
                obj[MOON_BOOKKEEPING_KEY].ToArray().Select((token) => (string)token).ToArray(),
                (string)obj[VERSION_KEY],
                DeserializeProfile((JObject)obj[PROFILE_KEY])
            );
        }

        internal static JObject SerializeConfig(Config value)
        {
            JObject obj = new JObject();
            obj[SKIN_BOOKKEEPING_KEY] = new JArray(value.KnownSkins.ToArray());
            obj[MOON_BOOKKEEPING_KEY] = new JArray(value.KnownMoons.ToArray());
            obj[VERSION_KEY] = value.Version;
            obj[PROFILE_KEY] = SerializeProfile(value.WorkingProfile);
            return obj;
        }
    }
}