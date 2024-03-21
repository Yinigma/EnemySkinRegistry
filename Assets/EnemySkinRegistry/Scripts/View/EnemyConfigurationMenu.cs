using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AntlerShed.SkinRegistry.View
{
    class ConfigurationViewModel
    {
        internal MoonInfo[] Moons { get; }
        internal Dictionary<string, Skin[]> Skins { get; }
        internal Dictionary<string, EnemyInfo> Enemies { get; }
        internal EnemyConfiguration[] Configs { get; }
        private Dictionary<string, EnemyConfiguration> updatedEntries;
        private string selectedEnemy;

        internal bool UnsavedChanges { get; private set; } = false;

        internal EnemyConfiguration SelectedConfig => updatedEntries[selectedEnemy];
        internal Skin[] AvailableSkins => Skins[selectedEnemy];

        internal Action enemySwitched;

        internal Action skinToggled;

        internal Action defaultReconfigured;

        internal Action<string> mapReconfigured;

        internal Action mapAdded;

        internal Action mapRemoved;

        internal Action<string> skinAddedToMoon;

        internal Action<string> skinRemovedFromMoon;

        internal Action skinAddedToDefault;

        internal Action skinRemovedFromDefault;

        public ConfigurationViewModel(MoonInfo[] moons, Dictionary<string, EnemyInfo> enemies, Dictionary<string, Skin[]> enemySkins, EnemyConfiguration[] configEntries)
        {
            Moons = moons.ToArray();
            Skins = new Dictionary<string, Skin[]>(enemySkins);
            Enemies = new Dictionary<string, EnemyInfo>(enemies);
            Configs = configEntries.ToArray();
            updatedEntries = configEntries.ToDictionary((cfg) => cfg.EnemyId, (cfg) => cfg);
            selectedEnemy = Configs[0].EnemyId;
        }

        internal void Exit(){ }

        internal void SwitchToNextEnemy()
        {
            int index = Array.FindIndex(Configs, (cfg) => cfg.EnemyId.Equals(selectedEnemy));
            selectedEnemy = Configs[(index + 1) % Configs.Length].EnemyId;
            enemySwitched?.Invoke();
        }

        internal void SwitchToPreviousEnemy()
        {
            int index = Array.FindIndex(Configs, (cfg) => cfg.EnemyId.Equals(selectedEnemy));
            selectedEnemy = Configs[(index + Configs.Length - 1) % Configs.Length].EnemyId;
            enemySwitched?.Invoke();
        }

        internal void AddMoonConfig(string moonId)
        {
            if(Moons.Any((moon)=>moon.Id.Equals(moonId)))
            {
                if(updatedEntries.ContainsKey(selectedEnemy))
                {
                    //if this moons does in fact not have an entry for this enemy already
                    if (!updatedEntries[selectedEnemy].MapConfigs.Any((cfg) => cfg.Id.Equals(moonId)))
                    {
                        List<MapConfiguration> mapCfg = updatedEntries[selectedEnemy].MapConfigs.ToList();
                        mapCfg.Add(new MapConfiguration(moonId, updatedEntries[selectedEnemy].DefaultConfiguration.VanillaFrequency, updatedEntries[selectedEnemy].DefaultConfiguration.Distribution));
                        updatedEntries[selectedEnemy] = new EnemyConfiguration
                        (
                            updatedEntries[selectedEnemy].EnemyId,
                            updatedEntries[selectedEnemy].DefaultConfiguration,
                            mapCfg.ToArray(),
                            updatedEntries[selectedEnemy].ActiveSkins
                        );
                        UnsavedChanges = true;
                        mapAdded?.Invoke();
                    }
                }
            }
        }

        internal void RemoveMoonConfig(string moonId)
        {
            if (Moons.Any((moon) => moon.Id.Equals(moonId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    try
                    {
                        List<MapConfiguration> mapCfg = updatedEntries[selectedEnemy].MapConfigs.ToList();
                        mapCfg.Remove(mapCfg.First((cfg)=>cfg.Id.Equals(moonId)));
                        updatedEntries[selectedEnemy] = new EnemyConfiguration
                        (
                            updatedEntries[selectedEnemy].EnemyId,
                            updatedEntries[selectedEnemy].DefaultConfiguration,
                            mapCfg.ToArray(),
                            updatedEntries[selectedEnemy].ActiveSkins
                        );
                        mapRemoved.Invoke();
                    }
                    //didn't have the entry in the first place... somehow...
                    catch (InvalidOperationException e) { }
                }
            }
        }

        internal void ClearMoonConfig(string moonId)
        {
            if (updatedEntries.ContainsKey(selectedEnemy))
            {
                try
                {
                    List<MapConfiguration> mapConfigs = updatedEntries[selectedEnemy].MapConfigs.ToList();
                    MapConfiguration mapConfig = mapConfigs.First((cfg) => cfg.Id.Equals(moonId));
                    mapConfigs[mapConfigs.IndexOf(mapConfig)] = new MapConfiguration(mapConfig.Id, 1.0f, new SkinConfigEntry[0]);
                    updatedEntries[selectedEnemy] = new EnemyConfiguration
                    (
                        updatedEntries[selectedEnemy].EnemyId,
                        updatedEntries[selectedEnemy].DefaultConfiguration,
                        mapConfigs.ToArray(),
                        updatedEntries[selectedEnemy].ActiveSkins
                    );
                }
                //again, if finding it doesn't work then we don't have a reason to care
                catch (InvalidOperationException e) { }
            }
        }

        internal void ClearDefaultConfig()
        {
            if (updatedEntries.ContainsKey(selectedEnemy))
            {
                updatedEntries[selectedEnemy] = new EnemyConfiguration
                (
                    updatedEntries[selectedEnemy].EnemyId,
                    new MapConfiguration(1.0f, new SkinConfigEntry[0]),
                    updatedEntries[selectedEnemy].MapConfigs,
                    updatedEntries[selectedEnemy].ActiveSkins
                );
            }
        }

        internal void SetSkinActive(string skinId, bool active)
        {
            if (Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((skin) => skin.Id.Equals(skinId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    List<string> activeList = updatedEntries[selectedEnemy].ActiveSkins.ToList();
                    if(active)
                    {
                        activeList.Add(skinId);
                    }
                    else
                    {
                        activeList.Remove(skinId);
                    }
                    updatedEntries[selectedEnemy] = new EnemyConfiguration
                    (
                        updatedEntries[selectedEnemy].EnemyId,
                        updatedEntries[selectedEnemy].DefaultConfiguration,
                        updatedEntries[selectedEnemy].MapConfigs,
                        activeList.ToArray()
                    );
                    UnsavedChanges = true;
                    skinToggled?.Invoke();
                }
            }
        }

        internal void AddSkinToMoon(string skinId, string moonId)
        {
            if (Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((skin) => skin.Id.Equals(skinId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    try
                    {
                        MapConfiguration cfg = updatedEntries[selectedEnemy].MapConfigs.First((cfg) => cfg.Id.Equals(moonId));
                        //skip if the skin is already present
                        if(!cfg.Distribution.Any((skin)=>skin.SkinId.Equals(skinId)))
                        {
                            MapConfiguration[] mapConfigs = updatedEntries[selectedEnemy].MapConfigs;
                            mapConfigs[Array.IndexOf(mapConfigs, cfg)] = AddSkinToMapConfig(cfg, skinId);
                            updatedEntries[selectedEnemy] = new EnemyConfiguration
                            (
                                updatedEntries[selectedEnemy].EnemyId,
                                updatedEntries[selectedEnemy].DefaultConfiguration,
                                mapConfigs,
                                updatedEntries[selectedEnemy].ActiveSkins
                            );
                            UnsavedChanges = true;
                            skinAddedToMoon?.Invoke(moonId);
                        }
                    }
                    catch (InvalidOperationException e) { }
                }
            }
        }

        internal void AddSkinToDefault(string skinId)
        {
            if (Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((skin) => skin.Id.Equals(skinId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    try
                    {
                        MapConfiguration cfg = updatedEntries[selectedEnemy].DefaultConfiguration;
                        //skip if the skin is already present
                        if (!cfg.Distribution.Any((skin) => skin.SkinId.Equals(skinId)))
                        {
                            updatedEntries[selectedEnemy] = new EnemyConfiguration
                            (
                                updatedEntries[selectedEnemy].EnemyId,
                                AddSkinToMapConfig(cfg, skinId),
                                updatedEntries[selectedEnemy].MapConfigs,
                                updatedEntries[selectedEnemy].ActiveSkins
                            );
                            UnsavedChanges = true;
                            skinAddedToDefault?.Invoke();
                        }
                    }
                    catch (InvalidOperationException e) { }
                }
            }
        }

        //It is still unreal what programming will let you say with a straight face
        internal void RemoveSkinFromMoon(string skinId, string moonId)
        {
            if (Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((skin) => skin.Id.Equals(skinId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    try
                    {
                        MapConfiguration cfg = updatedEntries[selectedEnemy].MapConfigs.First((cfg) => cfg.Id.Equals(moonId));
                        //skip if the skin isn't here
                        if (cfg.Distribution.Any((skin) => skin.SkinId.Equals(skinId)))
                        {
                            if (cfg.Distribution.Length == 1)
                            {
                                ClearMoonConfig(moonId);
                            }
                            else
                            {
                                MapConfiguration[] mapConfigs = updatedEntries[selectedEnemy].MapConfigs;
                                mapConfigs[Array.IndexOf(mapConfigs, cfg)] = RemoveSkinFromMapConfig(cfg, skinId);
                                updatedEntries[selectedEnemy] = new EnemyConfiguration
                                (
                                    updatedEntries[selectedEnemy].EnemyId,
                                    updatedEntries[selectedEnemy].DefaultConfiguration,
                                    mapConfigs,
                                    updatedEntries[selectedEnemy].ActiveSkins
                                );
                            }
                            UnsavedChanges = true;
                            skinRemovedFromMoon?.Invoke(moonId);
                        }
                    }
                    catch (InvalidOperationException e) { }
                }
            }
        }

        internal void RemoveSkinFromDefault(string skinId)
        {
            if (Skins.ContainsKey(selectedEnemy) && Skins[selectedEnemy].Any((skin) => skin.Id.Equals(skinId)))
            {
                if (updatedEntries.ContainsKey(selectedEnemy))
                {
                    try
                    {
                        MapConfiguration cfg = updatedEntries[selectedEnemy].DefaultConfiguration;
                        //skip if the skin isn't here
                        if (cfg.Distribution.Any((skin) => skin.SkinId.Equals(skinId)))
                        {
                            if (cfg.Distribution.Length == 1)
                            {
                                ClearDefaultConfig();
                            }
                            else
                            {
                                updatedEntries[selectedEnemy] = new EnemyConfiguration
                                (
                                    updatedEntries[selectedEnemy].EnemyId,
                                    RemoveSkinFromMapConfig(cfg, skinId),
                                    updatedEntries[selectedEnemy].MapConfigs,
                                    updatedEntries[selectedEnemy].ActiveSkins
                                );
                            }
                            UnsavedChanges = true;
                            skinRemovedFromDefault?.Invoke();
                        }
                    }
                    catch (InvalidOperationException e) { }
                }
            }
        }

        private MapConfiguration AddSkinToMapConfig(MapConfiguration config, string skinId)
        {
            List<SkinConfigEntry> mapDistro = config.Distribution.ToList();
            mapDistro.Add(new SkinConfigEntry(1.0f, skinId));
            return new MapConfiguration
            (
                config.Id,
                config.VanillaFrequency,
                mapDistro.ToArray()
            );
        }

        private MapConfiguration RemoveSkinFromMapConfig(MapConfiguration config, string skinId)
        {
            SkinConfigEntry[] origDistr = config.Distribution;
            List<SkinConfigEntry> mapDistro = config.Distribution
                .Where((skin)=>!skin.SkinId.Equals(skinId))
                .Select((skin) => new SkinConfigEntry(skin.Frequency, skin.SkinId)).ToList();
            return new MapConfiguration
            (
                config.Id,
                config.VanillaFrequency,
                mapDistro.ToArray()
            );
        }

        internal void SetMapVanillaRatio(string moonId, float newRatio)
        {
            if(updatedEntries.ContainsKey(selectedEnemy))
            {
                try
                {
                    MapConfiguration cfg = updatedEntries[selectedEnemy].MapConfigs.First((cfg) => cfg.Id.Equals(moonId));
                    MapConfiguration[] mapConfigs = updatedEntries[selectedEnemy].MapConfigs;
                    mapConfigs[Array.IndexOf(mapConfigs, cfg)] = SetVanillaRatio(cfg, newRatio);
                    updatedEntries[selectedEnemy] = new EnemyConfiguration
                    (
                        updatedEntries[selectedEnemy].EnemyId,
                        updatedEntries[selectedEnemy].DefaultConfiguration,
                        mapConfigs,
                        updatedEntries[selectedEnemy].ActiveSkins
                    );
                    UnsavedChanges = true;
                    mapReconfigured?.Invoke(moonId);
                }
                catch (InvalidOperationException e) { }
            }
        }

        internal void SetDefaultVanillaRatio(float newRatio)
        {
            if(updatedEntries.ContainsKey(selectedEnemy))
            {
                MapConfiguration cfg = updatedEntries[selectedEnemy].DefaultConfiguration;
                updatedEntries[selectedEnemy] = new EnemyConfiguration
                (
                    updatedEntries[selectedEnemy].EnemyId,
                    SetVanillaRatio(updatedEntries[selectedEnemy].DefaultConfiguration, newRatio),
                    updatedEntries[selectedEnemy].MapConfigs,
                    updatedEntries[selectedEnemy].ActiveSkins
                );
                UnsavedChanges = true;
                defaultReconfigured?.Invoke();
            }
        }

        private MapConfiguration SetVanillaRatio(MapConfiguration config, float newRatio) //mmm... vanilla...
        {
            return new MapConfiguration
            (
                config.Id,
                Math.Clamp(newRatio, 0.0f, 1.0f),
                config.Distribution
            );
        }

        internal void AdjustMoonSkinWeight(string moonId, string skinId, float newRatio)
        {
            if (updatedEntries.ContainsKey(selectedEnemy))
            {
                try
                {
                    MapConfiguration cfg = updatedEntries[selectedEnemy].MapConfigs.First((cfg) => cfg.Id.Equals(moonId));
                    MapConfiguration[] mapConfigs = updatedEntries[selectedEnemy].MapConfigs;
                    if (cfg.Distribution.Any((skin) => skin.SkinId.Equals(skinId)))
                    {
                        mapConfigs[Array.IndexOf(mapConfigs, cfg)] = AdjustSkinWeight(cfg, skinId, newRatio);
                        updatedEntries[selectedEnemy] = new EnemyConfiguration
                        (
                            updatedEntries[selectedEnemy].EnemyId,
                            updatedEntries[selectedEnemy].DefaultConfiguration,
                            mapConfigs,
                            updatedEntries[selectedEnemy].ActiveSkins
                        );
                        UnsavedChanges = true;
                        mapReconfigured?.Invoke(moonId);
                    }
                   
                }
                catch (InvalidOperationException e) { }
            }
        }

        internal void AdjustDefaultSkinWeight(string skinId, float newRatio)
        {
            if (updatedEntries.ContainsKey(selectedEnemy))
            {
                MapConfiguration cfg = updatedEntries[selectedEnemy].DefaultConfiguration;
                if (cfg.Distribution.Any((skin) => skin.SkinId.Equals(skinId)))
                {
                    updatedEntries[selectedEnemy] = new EnemyConfiguration
                    (
                        updatedEntries[selectedEnemy].EnemyId,
                        AdjustSkinWeight(cfg, skinId, newRatio),
                        updatedEntries[selectedEnemy].MapConfigs,
                        updatedEntries[selectedEnemy].ActiveSkins
                    );
                    UnsavedChanges = true;
                    defaultReconfigured?.Invoke();
                }
            }
        }

        private MapConfiguration AdjustSkinWeight(MapConfiguration config, string skinId, float newRatio)
        {
            return new MapConfiguration
            (
                config.Id,
                config.VanillaFrequency,
                config.Distribution.Select((skin) => new SkinConfigEntry(skin.SkinId.Equals(skinId) ? newRatio : skin.Frequency, skin.SkinId)).ToArray()
            );
        }

        internal void Save()
        {
            EnemySkinRegistry.UpdateConfiguration(updatedEntries.Values.ToArray());
            UnsavedChanges = false;
        }
    }
}