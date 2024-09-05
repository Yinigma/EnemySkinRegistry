using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AntlerShed.SkinRegistry.View
{
    class ConfigurationViewModel
    {
        internal MoonInfo[] Moons { get; }
        internal Dictionary<string, Skin[]> Skins { get; }
        internal Dictionary<string, EnemyInfo> Enemies { get; }
        internal List<KeyValuePair<string, string>> DefaultMoonConfigs { get; }
        internal List<KeyValuePair<string, string>> DefaultSkinConfigs { get; }
        internal string[] AvailableTags { get; }
        internal string[] LoadedProfiles { get; private set; }
        internal string selectedEnemy;
        internal EnemyConfiguration SelectedConfig => TempProfile.GetEnemyConfig(selectedEnemy);
        internal SpawnLocation CurrentSpawn => EnemySkinRegistry.AllowIndoorOutdoorConfig ? SelectedSpawn : Enemies[selectedEnemy].DefaultSpawnLocation;
        internal SpawnLocation SelectedSpawn { get; private set; } = SpawnLocation.INDOOR;
        internal bool UnsavedChanges { get; private set; } = false;
        internal Profile TempProfile;
        internal Skin[] AvailableSkins => Skins[selectedEnemy];
        internal string OriginalProfileName { get; private set; } = "";
        internal string ReassignedProfileName { get; private set; } = "";
        internal bool ProfileAssignmentFailed { get; private set; }

        internal Action enemySwitched;

        internal Action skinToggled;

        internal Action defaultReconfigured;

        internal Action<string> mapReconfigured;

        internal Action mapAdded;

        internal Action mapRemoved;

        internal Action<string> skinAddedToMoon;

        internal Action<string> skinRemovedFromMoon;

        internal Action spawnLocationChanged;

        internal Action skinAddedToDefault;

        internal Action skinRemovedFromDefault;

        internal Action defaultSkinConfigApplied;

        internal Action defaultMoonConfigApplied;

        internal Action customProfileLoaded;

        internal Action profilesLoaded;

        internal Action profileNameError;

        internal Action configTypeSwitched;

        internal ViewConfigType SelectedConfigType;

        public ConfigurationViewModel
        (
            MoonInfo[] moons,
            string[] availableTags, 
            Dictionary<string, EnemyInfo> enemies, 
            Dictionary<string, Skin[]> enemySkins,
            string[] loadedProfiles,
            Profile profile,
            Dictionary<string, string> skinDefaults,
            Dictionary<string, string> moonDefaults
        )
        {
            Moons = moons.ToArray();
            AvailableTags = availableTags.ToArray();
            Skins = new Dictionary<string, Skin[]>(enemySkins);
            Enemies = new Dictionary<string, EnemyInfo>(enemies);
            TempProfile = profile;
            LoadedProfiles = loadedProfiles;
            selectedEnemy = profile.ConfigData[0].EnemyId;
            DefaultMoonConfigs = moonDefaults.Keys.Select((key) => new KeyValuePair<string, string>(key, moonDefaults[key])).ToList();
            DefaultMoonConfigs.Sort((a, b) => a.Value.CompareTo(b.Value));
            DefaultSkinConfigs = skinDefaults.Keys.Select((key) => new KeyValuePair<string, string>(key, skinDefaults[key])).ToList();
            DefaultSkinConfigs.Sort((a, b) => a.Value.CompareTo(b.Value));
        }

        internal void Exit(){ }

        internal void SwitchToNextEnemy()
        {
            EnemyConfiguration[] configs = TempProfile.ConfigData;
            int index = Array.FindIndex(configs, (cfg) => cfg.EnemyId.Equals(selectedEnemy));
            selectedEnemy = configs[(index + 1) % configs.Length].EnemyId;
            enemySwitched?.Invoke();
        }

        internal void SwitchToPreviousEnemy()
        {
            EnemyConfiguration[] configs = TempProfile.ConfigData;
            int index = Array.FindIndex(configs, (cfg) => cfg.EnemyId.Equals(selectedEnemy));
            selectedEnemy = configs[(index + configs.Length - 1) % configs.Length].EnemyId;
            enemySwitched?.Invoke();
        }

        internal void AddMoonConfig(string moonId)
        {
            if (!EnemySkinRegistry.ClientSyncActive)
            {
                EnemyConfiguration selectedConfig = TempProfile.GetEnemyConfig(selectedEnemy);
                if (selectedConfig != null && selectedConfig.GetMoonConfig(moonId) == null)
                {

                    selectedConfig.AddMoon(moonId);
                    UnsavedChanges = true;
                    mapAdded?.Invoke();
                }
            }
        }

        internal void RemoveMoonConfig(string moonId)
        {
            if (!EnemySkinRegistry.ClientSyncActive)
            {
                EnemyConfiguration selectedConfig = TempProfile.GetEnemyConfig(selectedEnemy);
                if (selectedConfig != null)
                {
                    selectedConfig.RemoveMoon(moonId);
                    UnsavedChanges = true;
                    mapRemoved.Invoke();
                }
            }
        }

        internal void SetSkinActive(string skinId, bool active)
        {
            if (!EnemySkinRegistry.ClientSyncActive)
            {
                EnemyConfiguration config = TempProfile.GetEnemyConfig(selectedEnemy);
                if (config != null && Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((Skin skin) => skin.Id.Equals(skinId)))
                {
                    if (active)
                    {
                        config.ActivateSkin(Skins[selectedEnemy].First((Skin skin) => skin.Id.Equals(skinId)));
                    }
                    else
                    {
                        config.DeactivateSkin(skinId); ;
                    }
                    UnsavedChanges = true;
                    skinToggled?.Invoke();
                }
            }
        }

        internal void AddSkinToMoon(string skinId, string moonId)
        {
            if (!EnemySkinRegistry.ClientSyncActive)
            {
                EnemyConfiguration config = TempProfile.GetEnemyConfig(selectedEnemy);
                if (config != null)
                {
                    MapConfiguration mapConfig = config.GetMoonConfig(moonId);
                    if (mapConfig != null && !mapConfig.Distribution.Any((SkinConfigEntry entry) => entry.SkinId.Equals(skinId)))
                    {
                        mapConfig.AddEntry(new SkinConfigEntry(1.0f, skinId));
                        UnsavedChanges = true;
                        skinAddedToMoon?.Invoke(moonId);
                    }
                }
            }
        }

        internal void AddSkinToDefault(string skinId)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration config = TempProfile.GetEnemyConfig(selectedEnemy);
            if (config!=null)
            {
                if(!config.DefaultConfiguration.Distribution.Any((SkinConfigEntry entry) => entry.SkinId.Equals(skinId)))
                {
                    config.DefaultConfiguration.AddEntry(new SkinConfigEntry(1.0f, skinId));
                    UnsavedChanges = true;
                    skinAddedToDefault?.Invoke();
                }
            }
        }

        //It is still unreal what programming will let you say with a straight face
        internal void RemoveSkinFromMoon(string skinId, string moonId)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration selectedConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if(selectedConfig!=null)
            {
                MapConfiguration moonConfig = selectedConfig.GetMoonConfig(moonId);
                if(moonConfig!=null)
                {
                    moonConfig.RemoveEntry(skinId);
                    UnsavedChanges = true;
                    skinRemovedFromMoon?.Invoke(moonId);
                }
            }
        }

        internal void RemoveSkinFromDefault(string skinId)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration selectedConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if (selectedConfig!=null)
            {
                selectedConfig.DefaultConfiguration.RemoveEntry(skinId);
                UnsavedChanges = true;
                skinRemovedFromDefault?.Invoke();
            }
        }

        internal void SetMapVanillaRatio(string moonId, float newRatio)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration enemyConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if (enemyConfig!=null)
            {
                MapConfiguration mapConfig = enemyConfig?.GetMoonConfig(moonId);
                if (mapConfig!=null)
                {
                    MapConfiguration config = mapConfig;
                    if (mapConfig!=null)
                    {
                        MapConfiguration configValue = mapConfig;
                        if (EnemySkinRegistry.AllowIndoorOutdoorConfig)
                        {
                            if (CurrentSpawn == SpawnLocation.INDOOR)
                            {
                                configValue.IndoorVanillaFrequency = newRatio;
                            }
                            else
                            {
                                configValue.OutdoorVanillaFrequency = newRatio;
                            }
                        }
                        else
                        {
                            configValue.IndoorVanillaFrequency = newRatio;
                            configValue.OutdoorVanillaFrequency = newRatio;
                        }

                        UnsavedChanges = true;
                        mapReconfigured?.Invoke(moonId);
                    }
                }
            }
        }

        internal void SetDefaultVanillaRatio(float newRatio)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration enemyConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if (enemyConfig!=null)
            {
                MapConfiguration mapConfig = enemyConfig?.DefaultConfiguration;
                if (mapConfig!=null)
                {
                    if(mapConfig!=null)
                    {
                        MapConfiguration configValue = mapConfig;
                        if (EnemySkinRegistry.AllowIndoorOutdoorConfig)
                        {
                            if (CurrentSpawn == SpawnLocation.INDOOR)
                            {
                                configValue.IndoorVanillaFrequency = newRatio;
                            }
                            else
                            {
                                configValue.OutdoorVanillaFrequency = newRatio;
                            }
                        }
                        else
                        {
                            configValue.OutdoorVanillaFrequency = newRatio;
                            configValue.IndoorVanillaFrequency = newRatio;
                        }

                        UnsavedChanges = true;
                        defaultReconfigured?.Invoke();
                    }
                }
            }
        }

        internal void AdjustMoonSkinWeight(string moonId, string skinId, float newRatio)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration enemyConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if (enemyConfig!=null)
            {
                MapConfiguration mapConfig = enemyConfig?.GetMoonConfig(moonId);
                if(mapConfig!=null)
                {
                    if(EnemySkinRegistry.AllowIndoorOutdoorConfig)
                    {
                        if (CurrentSpawn == SpawnLocation.INDOOR)
                        {
                            mapConfig.SetIndoorSkinFrequency(skinId, newRatio);
                        }
                        else
                        {
                            mapConfig.SetOutdoorSkinFrequency(skinId, newRatio);
                        }
                    }
                    else
                    {
                        mapConfig.SetIndoorSkinFrequency(skinId, newRatio);
                        mapConfig.SetOutdoorSkinFrequency(skinId, newRatio);
                    }
                    
                    UnsavedChanges = true;
                    mapReconfigured?.Invoke(moonId);
                }
            }
        }

        internal void AdjustDefaultSkinWeight(string skinId, float newRatio)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemyConfiguration enemyConfig = TempProfile.GetEnemyConfig(selectedEnemy);
            if(enemyConfig!=null)
            {
                if (EnemySkinRegistry.AllowIndoorOutdoorConfig)
                {
                    if (CurrentSpawn == SpawnLocation.INDOOR)
                    {
                        enemyConfig.DefaultConfiguration.SetIndoorSkinFrequency(skinId, newRatio);
                    }
                    else
                    {
                        enemyConfig.DefaultConfiguration.SetOutdoorSkinFrequency(skinId, newRatio);
                    }
                }
                else
                {
                    enemyConfig.DefaultConfiguration.SetIndoorSkinFrequency(skinId, newRatio);
                    enemyConfig.DefaultConfiguration.SetOutdoorSkinFrequency(skinId, newRatio);
                }
                UnsavedChanges = true;
                defaultReconfigured?.Invoke();
            }
        }

        internal void SetSkinConfigToIndoor()
        {
            SelectedSpawn = SpawnLocation.INDOOR;
            spawnLocationChanged?.Invoke();
        }

        internal void SetSkinConfigToOutdoor()
        {
            SelectedSpawn = SpawnLocation.OUTDOOR;
            spawnLocationChanged?.Invoke();
        }

        internal void ReapplyDefaultMoonConfiguration(string moonId)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"applying default moon config for id {moonId}");
            EnemySkinRegistry.ApplyDefaultMoonConfiguration(moonId, TempProfile);
            UnsavedChanges = true;
        }

        internal void ReapplyDefaultSkinConfiguration(string skinId)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            if (EnemySkinRegistry.LogLevelSetting >= LogLevel.INFO) EnemySkinRegistry.SkinLogger.LogInfo($"applying default skin config for id {skinId}");
            EnemySkinRegistry.ApplyDefaultSkinConfiguration(skinId, TempProfile);
            UnsavedChanges = true;
        }

        internal void Save()
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            EnemySkinRegistry.UpdateConfiguration(TempProfile);
            UnsavedChanges = false;
        }

        internal void OverwriteProfile(string label)
        {
            EnemySkinRegistry.StoreProfile(TempProfile, label);
        }

        internal void SaveProfile(string label)
        {
            try
            {
                string sanLabel = EnemySkinRegistry.StoreProfile(TempProfile, label);
                LoadedProfiles = EnemySkinRegistry.GetStoredProfiles();
                OriginalProfileName = label;
                ReassignedProfileName = sanLabel;
                profilesLoaded?.Invoke();
            }
            catch(ProfileNameException e)
            {
                profileNameError?.Invoke();
            }
        }

        internal void SwitchConfigType(ViewConfigType newType)
        {
            SelectedConfigType = newType;
            configTypeSwitched?.Invoke();
        }

        internal void LoadProfile(string profile)
        {
            if (EnemySkinRegistry.ClientSyncActive)
            {
                return;
            }
            TempProfile = EnemySkinRegistry.GetProfile(profile);
            customProfileLoaded?.Invoke();
        }
    }

    internal enum ViewConfigType
    {
        SKIN,
        MOON,
        CUSTOM
    }
}