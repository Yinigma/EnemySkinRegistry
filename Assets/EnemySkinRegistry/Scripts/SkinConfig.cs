using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AntlerShed.SkinRegistry
{
    class SkinConfig
    {
        private const string SKIN_SECTION = "SkinConfig";
        private ConfigFile configFile;
        private EnemyConfigConverter jsonConverter = new EnemyConfigConverter();
        private bool configInitialized = false;
        internal ConfigEntry<LogLevel> LogLevelSetting { get; private set; }
        //private IDictionary<string, MoonConfigEntry> configEntries;

        internal SkinConfig(ConfigFile config)
        {
            config.SaveOnConfigSet = false;
            configFile = config;
            //config.SettingChanged += SettingChanged;
            LogLevelSetting = configFile.Bind("Logging", "LogLevel", LogLevel.ERROR, "Flag for activating debug messages.");
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
        internal void InitConfigForAllSkins(EnemyInfo[] loadedEnemies, Skin[] loadedSkins, string[] loadedMoons)
        {
            if (!configInitialized)
            {
                foreach (EnemyInfo info in loadedEnemies)
                {
                    //EnemySkins.SkinLogger.LogInfo($"Binding enemy config");
                    Skin[] enemySkins = loadedSkins.Where((skin) => skin.EnemyId.Equals(info.Id)).ToArray();
                    //Once again I have been slighted with external limitation. It seems I have no recourse save resorting to yet another evil trick. Go! JSON Serialization!
                    //EnemySkins.SkinLogger.LogInfo($"Serializing default value as : {JsonConvert.SerializeObject(new EnemyConfiguration(info.Id, enemySkins), new JsonConverter[] { jsonConverter })}");
                    ConfigEntry<string> entry = configFile.Bind(SKIN_SECTION, info.Id, JsonConvert.SerializeObject(new EnemyConfiguration(info.Id, enemySkins), new JsonConverter[] { jsonConverter }), $"Configuration data for the \"{info.Name}\" enemy.");
                    try
                    {
                        EnemyConfiguration configEntry = JsonConvert.DeserializeObject<EnemyConfiguration>(entry.Value, new JsonConverter[] { jsonConverter });
                        //Remove skins that are no longer installed and add skins that have been installed as active and as frequent as possible in the default config
                        SkinConfigEntry[] sanitizedDefaultDistribution = enemySkins.Select
                        (
                            (lskin)=> new SkinConfigEntry
                            ( 
                                configEntry.DefaultConfiguration.Distribution.Any((dSkin)=>dSkin.SkinId.Equals(lskin.Id)) ? 
                                    configEntry.DefaultConfiguration.Distribution.First((dConfig)=>dConfig.SkinId.Equals(lskin.Id)).Frequency : 
                                    1.0f,
                                lskin.Id
                            )
                        ).ToArray();
                        string[] sanitizedActiveSkins = enemySkins
                            .Select((lskin)=>lskin.Id)
                            .Where((id)=> !configEntry.DefaultConfiguration.Distribution.Any((dSkin) => dSkin.SkinId.Equals(id)) || configEntry.ActiveSkins.Contains(id)).ToArray();
                        EnemyConfiguration sanitizedConfigEntry = new EnemyConfiguration
                        (
                            configEntry.EnemyId,
                            new MapConfiguration(configEntry.DefaultConfiguration.VanillaFrequency, sanitizedDefaultDistribution),
                            configEntry.MapConfigs.Where((mapConfig) => loadedMoons.Any((id) => id.Equals(mapConfig.Id))).Select
                            (
                                (mapConfig) => new MapConfiguration(mapConfig.Id, mapConfig.VanillaFrequency, mapConfig.Distribution.Where((skin) => enemySkins.Any((lskin) => lskin.Id.Equals(skin.SkinId))).ToArray())
                            ).ToArray(),
                            sanitizedActiveSkins
                        );
                        entry.Value = JsonConvert.SerializeObject(sanitizedConfigEntry, new JsonConverter[] { jsonConverter });
                    }
                    catch (JsonSerializationException e)
                    {
                        if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN)
                        {
                            EnemySkinRegistry.SkinLogger.LogWarning($"Failed to read configuration data for \"{info.Name}.\" Resetting to default settings.");
                            EnemySkinRegistry.SkinLogger.LogWarning(e.Message);
                        }
                        entry.Value = JsonConvert.SerializeObject(JsonConvert.SerializeObject(new EnemyConfiguration(info.Id, enemySkins)), new JsonConverter[] { jsonConverter });
                    }
                }
                List<ConfigDefinition> staleEntries = configFile.Where((entry) => !loadedEnemies.Any((enemy) => enemy.Id.Equals(entry.Key.Key))).Select((entry)=>entry.Key).ToList();
                foreach (ConfigDefinition staleEntry in staleEntries)
                {
                    configFile.Remove(staleEntry);
                }
                configFile.Save();
                configInitialized = true;
            }
        }

        /// <summary>
        /// Gets the configured skin distribution for a given enemy spawning on a given moon
        /// </summary>
        /// <param name="moonId">The moon to get the distribution for</param>
        /// <param name="enemyType">The enemy type to get the distribution for</param>
        /// <returns>An alphabetically-sorted list of skin-distribution threshold pairs to pull from when assigning an enemy skin. 
        /// Distributions are cumulative for convenience when using an random number generator, so an even distribution of four skins will have thresholds 0.25, 0.5, 0.75, 1.0</returns>
        internal SkinDistribution GetConfiguredDistribution(string moonId, string enemyType)
        {
            try
            {
                EnemyConfiguration enemyConfig = JsonConvert.DeserializeObject<EnemyConfiguration>(configFile.First((entry) => entry.Key.Key.Equals(enemyType)).Value.BoxedValue as string, new JsonConverter[] { jsonConverter });
                string[] activeSkins = enemyConfig.ActiveSkins;
                MapConfiguration mapConfig = enemyConfig.MapConfigs.Any((mapCfg) => mapCfg.Id.Equals(moonId)) ? enemyConfig.MapConfigs.First((mapCfg) => mapCfg.Id.Equals(moonId)) : enemyConfig.DefaultConfiguration;

                float activeSkinSum = mapConfig.Distribution.Aggregate(0.0f, (sum, skin) => sum += activeSkins.Contains(skin.SkinId) ? skin.Frequency : 0.0f);
                activeSkinSum += mapConfig.VanillaFrequency;

                List<SkinThreshold> thresholdList = new List<SkinThreshold>();
                List<SkinConfigEntry> sortedSkins = mapConfig.Distribution.Where((skin) => activeSkins.Contains(skin.SkinId)).ToList();
                sortedSkins.Sort((a, b) => a.SkinId.CompareTo(b.SkinId));
                float runningTotal = 0.0f;
                if(activeSkinSum > 0.0)
                {
                    foreach (SkinConfigEntry skin in sortedSkins)
                    {
                        runningTotal += skin.Frequency / activeSkinSum;
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

        internal void UpdateConfiguration(EnemyConfiguration[] updatedEntries)
        {
            foreach(EnemyConfiguration entry in updatedEntries)
            {
                ConfigDefinition key = new ConfigDefinition(SKIN_SECTION, entry.EnemyId);
                if (configFile.ContainsKey(key))
                {
                    configFile[key].BoxedValue = JsonConvert.SerializeObject(entry, new JsonConverter[] { jsonConverter });
                }
            }
            configFile.Save();
        }

        internal EnemyConfiguration? GetConfiguration(string enemyId)
        {
            ConfigDefinition key = new ConfigDefinition(SKIN_SECTION, enemyId);
            if (configFile.ContainsKey(key))
            {
                try
                {
                    return JsonConvert.DeserializeObject<EnemyConfiguration>((configFile[key] as ConfigEntry<string>).Value, new JsonConverter[]{ jsonConverter });
                }
                catch(JsonSerializationException e)
                {
                    if (EnemySkinRegistry.LogLevelSetting >= LogLevel.ERROR)
                    {
                        EnemySkinRegistry.SkinLogger.LogError($"Failed to deserialize configuration for {enemyId}");
                        EnemySkinRegistry.SkinLogger.LogError(e.Message);
                    }
                }
            }
            else
            {
                if (EnemySkinRegistry.LogLevelSetting >= LogLevel.WARN) EnemySkinRegistry.SkinLogger.LogWarning($"Attempted to read config entry for {enemyId}, but no entry exists in the config");
            }
            return null;
        }
    }

    public struct EnemyConfiguration
    {
        public string EnemyId { get; }
        public string[] ActiveSkins => activeSkins.ToArray();
        private string[] activeSkins;
        public MapConfiguration DefaultConfiguration { get; }
        public MapConfiguration[] MapConfigs => mapConfigs.ToArray();
        private MapConfiguration[] mapConfigs;

        public EnemyConfiguration(string enemyType, MapConfiguration defaultConfiguration, MapConfiguration[] mapConfigs, string[] active)
        {
            EnemyId = enemyType;
            DefaultConfiguration = defaultConfiguration;
            this.mapConfigs = mapConfigs.Select(mapEntry => new MapConfiguration(mapEntry.Id, mapEntry.VanillaFrequency, mapEntry.Distribution)).ToArray();
            Array.Sort(this.mapConfigs, (a, b) => a.Id.CompareTo(b.Id));
            activeSkins = active.ToArray();
        }

        internal EnemyConfiguration(string enemyType, Skin[] availableSkins) : this(enemyType, new MapConfiguration(0.0f, availableSkins.Select((skin) => new SkinConfigEntry(1.0f, skin.Id)).ToArray()), new MapConfiguration[]{ }, availableSkins.Select((skin)=>skin.Id).ToArray()) { }

        /*internal EnemyConfiguration Copy()
        {
            return new EnemyConfiguration(EnemyType, DefaultConfiguration.Copy(), mapConfigs.Select((cfg)=>cfg.Copy()).ToArray(), ActiveSkins);
        }*/
    }

    public struct MapConfiguration
    {
        public string Id { get; }
        private SkinConfigEntry[] distribution;
        public SkinConfigEntry[] Distribution => distribution.ToArray();

        public float VanillaFrequency { get; }

        public MapConfiguration(string id, float vanillaFrequency, SkinConfigEntry[] distribution)
        {
            Id = id;
            this.distribution = distribution.ToArray();
            VanillaFrequency = Math.Clamp(vanillaFrequency, 0.0f, 1.0f);
            Array.Sort(this.distribution, (a, b) => a.SkinId.CompareTo(b.SkinId));
        }

        internal MapConfiguration(float vanillaFrequency, SkinConfigEntry[] distribution) : this("default", vanillaFrequency, distribution){ }

        /*internal MapConfiguration Copy()
        {
            return new MapConfiguration(Id, distribution.Select((skin)=>skin.Copy()).ToArray());
        }*/
    }

    public struct SkinConfigEntry
    {
        public float Frequency { get; }
        public string SkinId { get; }

        internal SkinConfigEntry(float frequency, string skinId)
        {
            Frequency = Math.Clamp(frequency, 0.0f, 1.0f); ;
            SkinId = skinId;
        }

        /*internal SkinConfigEntry Copy()
        {
            return new SkinConfigEntry(Frequency, SkinId);
        }*/
    }

    public class EnemyConfigConverter : JsonConverter<EnemyConfiguration>
    {
        private const string ENEMY_ID_KEY = "enemyId";
        private const string ACTIVE_KEY = "activeSkins";
        private const string DEFAULT_KEY = "defaultConfiguration";
        private const string MAPS_KEY = "mapConfigurations";

        private const string MAP_ID_KEY = "mapId";
        private const string VANILLA_KEY = "vanillaFrequency";
        private const string DISTRIBUTION_KEY = "skinDistribution";

        private const string SKIN_ID_KEY = "skinId";
        private const string FREQUENCY_KEY = "frequency";

        public override EnemyConfiguration ReadJson(JsonReader reader, Type objectType, EnemyConfiguration existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject obj = (JObject)JToken.ReadFrom(reader);
            return new EnemyConfiguration
            (
                (string)obj[ENEMY_ID_KEY],
                DeserializeMapConfig((JObject)obj[DEFAULT_KEY]), 
                obj[MAPS_KEY].ToArray().Select((token)=>DeserializeMapConfig((JObject)token)).ToArray(),
                obj[ACTIVE_KEY].ToArray().Select((token)=>(string)token).ToArray()
            );
        }

        public override void WriteJson(JsonWriter writer, EnemyConfiguration value, JsonSerializer serializer)
        {
            JObject obj = new JObject();
            obj[ENEMY_ID_KEY] = value.EnemyId;
            obj[ACTIVE_KEY] = new JArray(value.ActiveSkins);
            obj[DEFAULT_KEY] = SerializeMapConfig(value.DefaultConfiguration);
            obj[MAPS_KEY] = new JArray(value.MapConfigs.Select((cfg) => SerializeMapConfig(cfg)));
            obj.WriteTo(writer);
        }

        private JObject SerializeMapConfig(MapConfiguration value)
        {
            JObject obj = new JObject();
            obj[MAP_ID_KEY] = value.Id.Equals("default") ? "" : value.Id;
            obj[VANILLA_KEY] = value.VanillaFrequency;
            obj[DISTRIBUTION_KEY] = new JArray(value.Distribution.Select((cfg) => SerializeSkinConfig(cfg)));
            return obj;
        }

        private MapConfiguration DeserializeMapConfig(JObject obj)
        {
            return string.IsNullOrEmpty((string)obj[MAP_ID_KEY]) ?
                new MapConfiguration
                (
                    (float)obj[VANILLA_KEY],
                    obj[DISTRIBUTION_KEY].ToArray().Select((token) => DeserializeSkinConfig((JObject)token)).ToArray()
                ) :
                new MapConfiguration
                (
                    (string)obj[MAP_ID_KEY],
                    (float)obj[VANILLA_KEY],
                    obj[DISTRIBUTION_KEY].ToArray().Select((token)=>DeserializeSkinConfig((JObject)token)).ToArray()
                );
        }

        private JObject SerializeSkinConfig(SkinConfigEntry value)
        {
            JObject obj = new JObject();
            obj[SKIN_ID_KEY] = value.SkinId;
            obj[FREQUENCY_KEY] = value.Frequency;
            return obj;
        }

        private SkinConfigEntry DeserializeSkinConfig(JObject obj)
        {
            return new SkinConfigEntry((float)obj[FREQUENCY_KEY], (string)obj[SKIN_ID_KEY]);
        }
    }
}