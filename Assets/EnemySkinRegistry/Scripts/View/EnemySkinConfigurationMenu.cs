using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AntlerShed.SkinRegistry.View
{
    public class EnemySkinConfigurationMenu : MonoBehaviour
    {
        [SerializeField]
        private TMP_Text enemyLabel;

        [SerializeField]
        private AvailableSkinView skinViewPrefab;

        [SerializeField]
        private RectTransform skinViewContainer;

        private List<AvailableSkinView> activeSkinToggles = new List<AvailableSkinView>();

        [SerializeField]
        private MapConfigurationView defaultConfig;

        [SerializeField]
        private MapConfigurationView mapConfigPrefab;

        [SerializeField]
        private RectTransform mapConfigContainer;

        private List<MapConfigurationView> mapConfigs = new List<MapConfigurationView>();

        [SerializeField]
        private TMP_Dropdown moonDropdown;

        [SerializeField]
        private Button nextEnemyButton;

        [SerializeField]
        private Button previousEnemyButton;

        [SerializeField]
        private EventTrigger activeSkinsToolTipHover;

        [SerializeField]
        private EventTrigger mapConfigToolTipHover;

        [SerializeField]
        private EventTrigger defaultConfigToolTipHover;

        [SerializeField]
        private RectTransform activeSkinsToolTip;

        [SerializeField]
        private RectTransform mapConfigToolTip;

        [SerializeField]
        private RectTransform defaultConfigToolTip;

        [SerializeField]
        private Button saveConfigButton;

        [SerializeField]
        private Button exitButton;

        [SerializeField]
        private RectTransform exitDialog;

        [SerializeField]
        private Button saveDialogButton;

        [SerializeField]
        private Button discardDialogButton;

        [SerializeField]
        private Button cancelDialogButton;

        [SerializeField]
        private TMP_Dropdown configTypeDropdown;

        [SerializeField]
        private TMP_Dropdown defaultConfigDropdown;

        [SerializeField]
        private Button reapplyConfigButton;

        [SerializeField]
        private Toggle insideToggle;

        [SerializeField]
        private Toggle outsideToggle;

        [SerializeField]
        private RectTransform indoorOutdoorToggleGroup;

        [SerializeField]
        private Button storeButton;

        [SerializeField]
        private SaveProfilePopover profilePopover;

        internal void Init(ConfigurationViewModel viewModel)
        {
            saveConfigButton.onClick.AddListener(() => viewModel.Save());
            nextEnemyButton.onClick.AddListener(() => viewModel.SwitchToNextEnemy());
            previousEnemyButton.onClick.AddListener(() => viewModel.SwitchToPreviousEnemy());
            indoorOutdoorToggleGroup.gameObject.SetActive(EnemySkinRegistry.AllowIndoorOutdoorConfig);
            exitButton.onClick.AddListener
            (
                () =>
                {
                    if (viewModel.UnsavedChanges)
                    {
                        exitDialog.gameObject.SetActive(true);
                    }
                    else
                    {
                        viewModel.Exit();
                        Destroy(gameObject);
                    }
                }
            );
            saveDialogButton.onClick.AddListener
            (
                () =>
                {
                    viewModel.Save();
                    viewModel.Exit();
                    Destroy(gameObject);
                }
            );
            discardDialogButton.onClick.AddListener
            (
                () =>
                {
                    viewModel.Exit();
                    Destroy(gameObject);
                }
            );
            cancelDialogButton.onClick.AddListener
            (
                () =>
                {
                    exitDialog.gameObject.SetActive(false);
                }
            );
            insideToggle.onValueChanged.AddListener
            (
                (isOn) =>
                {
                    if (isOn)
                    {
                        viewModel.SetSkinConfigToIndoor();
                    }
                }
            );
            outsideToggle.onValueChanged.AddListener
            (
                (isOn) =>
                {
                    if (isOn)
                    {
                        viewModel.SetSkinConfigToOutdoor();
                    }
                }
            );

            //a bunch of boilerplate that enables tooltip hovers to work
            EventTrigger.Entry activeSkinsEnterTrigger = new EventTrigger.Entry();
            activeSkinsEnterTrigger.eventID = EventTriggerType.PointerEnter;
            activeSkinsEnterTrigger.callback.AddListener((eventData) => activeSkinsToolTip.gameObject.SetActive(true));
            activeSkinsToolTipHover.triggers.Add(activeSkinsEnterTrigger);

            EventTrigger.Entry activeSkinsExitTrigger = new EventTrigger.Entry();
            activeSkinsExitTrigger.eventID = EventTriggerType.PointerExit;
            activeSkinsExitTrigger.callback.AddListener((eventData) => activeSkinsToolTip.gameObject.SetActive(false));
            activeSkinsToolTipHover.triggers.Add(activeSkinsExitTrigger);

            EventTrigger.Entry mapConfigEnterTrigger = new EventTrigger.Entry();
            mapConfigEnterTrigger.eventID = EventTriggerType.PointerEnter;
            mapConfigEnterTrigger.callback.AddListener((eventData) => mapConfigToolTip.gameObject.SetActive(true));
            mapConfigToolTipHover.triggers.Add(mapConfigEnterTrigger);

            EventTrigger.Entry mapConfigExitTrigger = new EventTrigger.Entry();
            mapConfigExitTrigger.eventID = EventTriggerType.PointerExit;
            mapConfigExitTrigger.callback.AddListener((eventData) => mapConfigToolTip.gameObject.SetActive(false));
            mapConfigToolTipHover.triggers.Add(mapConfigExitTrigger);

            EventTrigger.Entry defaultEnterTrigger = new EventTrigger.Entry();
            defaultEnterTrigger.eventID = EventTriggerType.PointerEnter;
            defaultEnterTrigger.callback.AddListener((eventData) => defaultConfigToolTip.gameObject.SetActive(true));
            defaultConfigToolTipHover.triggers.Add(defaultEnterTrigger);

            EventTrigger.Entry defaultExitTrigger = new EventTrigger.Entry();
            defaultExitTrigger.eventID = EventTriggerType.PointerExit;
            defaultExitTrigger.callback.AddListener((eventData) => defaultConfigToolTip.gameObject.SetActive(false));
            defaultConfigToolTipHover.triggers.Add(defaultExitTrigger);

            //callbacks for how the ui should react to changes in the viewmodel

            viewModel.skinToggled += () => Rebuild(viewModel);
            viewModel.enemySwitched += () => Rebuild(viewModel);
            viewModel.mapReconfigured += (mapId) => mapConfigs[Array.FindIndex(viewModel.SelectedConfig.MapConfigs, (cfg) => cfg.Id.Equals(mapId))].Rebuild(viewModel, mapId);
            viewModel.skinAddedToMoon += (mapId) => mapConfigs[Array.FindIndex(viewModel.SelectedConfig.MapConfigs, (cfg) => cfg.Id.Equals(mapId))].Rebuild(viewModel, mapId);
            viewModel.skinRemovedFromMoon += (mapId) => mapConfigs[Array.FindIndex(viewModel.SelectedConfig.MapConfigs, (cfg) => cfg.Id.Equals(mapId))].Rebuild(viewModel, mapId);
            viewModel.defaultReconfigured += () => defaultConfig.Rebuild(viewModel, null, true);
            viewModel.skinAddedToDefault += () => defaultConfig.Rebuild(viewModel, null, true);
            viewModel.skinRemovedFromDefault += () => defaultConfig.Rebuild(viewModel, null, true);
            viewModel.mapAdded += () => Rebuild(viewModel);
            viewModel.mapRemoved += () => Rebuild(viewModel);
            viewModel.spawnLocationChanged += () => Rebuild(viewModel);
            viewModel.defaultSkinConfigApplied += () => Rebuild(viewModel);
            viewModel.defaultMoonConfigApplied += () => Rebuild(viewModel);
            viewModel.customProfileLoaded += () => Rebuild(viewModel);
            viewModel.configTypeSwitched += () => Rebuild(viewModel);

            configTypeDropdown.AddOptions(Enum.GetNames(typeof(ViewConfigType)).Select((name) => name.ToLower()).ToList());

            configTypeDropdown.onValueChanged.AddListener((value) => viewModel.SwitchConfigType((ViewConfigType)value));

            defaultConfigDropdown.onValueChanged.AddListener
            (
                (value) =>
                {
                    defaultConfigDropdown.RefreshShownValue();
                }
            );
            defaultConfigDropdown.value = -1;

            reapplyConfigButton.onClick.AddListener
            (
                () =>
                {
                    if (defaultConfigDropdown.value >= 0)
                    {
                        if (viewModel.SelectedConfigType == ViewConfigType.MOON)
                        {
                            viewModel.ReapplyDefaultMoonConfiguration
                            (
                                viewModel.DefaultMoonConfigs[defaultConfigDropdown.value].Key
                            );
                            viewModel.defaultMoonConfigApplied?.Invoke();
                        }
                        else if (viewModel.SelectedConfigType == ViewConfigType.SKIN)
                        {
                            viewModel.ReapplyDefaultSkinConfiguration
                            (
                                viewModel.DefaultSkinConfigs[defaultConfigDropdown.value].Key
                            );
                            viewModel.defaultSkinConfigApplied?.Invoke();
                        }
                        else if (viewModel.SelectedConfigType == ViewConfigType.CUSTOM)
                        {
                            viewModel.LoadProfile(viewModel.LoadedProfiles[defaultConfigDropdown.value]);
                        }
                    }
                }
            );

            storeButton.onClick.AddListener(() => profilePopover.gameObject.SetActive(true));
            profilePopover.Init(viewModel);
            Rebuild(viewModel);
        }

        internal void Rebuild(ConfigurationViewModel viewModel)
        {
            //To add this back, make the frequency ui update event fire a full rebuild. Right now I don't feel like it's worth the trouble
            //saveConfigButton.enabled = viewModel.UnsavedChanges;
            defaultConfigDropdown.ClearOptions();
            if (viewModel.SelectedConfigType == ViewConfigType.MOON)
            {
                defaultConfigDropdown.AddOptions(viewModel.DefaultMoonConfigs.Select((cfg) => cfg.Value).ToList());
            }
            else if (viewModel.SelectedConfigType == ViewConfigType.SKIN)
            {
                defaultConfigDropdown.AddOptions(viewModel.DefaultSkinConfigs.Select((cfg) => cfg.Value).ToList());
            }
            else if (viewModel.SelectedConfigType == ViewConfigType.CUSTOM)
            {
                defaultConfigDropdown.AddOptions(viewModel.LoadedProfiles.ToList());
            }
            enemyLabel.text = viewModel.SelectedConfig.EnemyId == null ? "<null enemy>" : viewModel.Enemies.ContainsKey(viewModel.SelectedConfig.EnemyId) ? viewModel.Enemies[viewModel.SelectedConfig.EnemyId].Name : viewModel.SelectedConfig.EnemyId;

            //Skin Icons
            if (viewModel.AvailableSkins.Length > activeSkinToggles.Count)
            {
                int diff = viewModel.AvailableSkins.Length - activeSkinToggles.Count;
                for (int i = 0; i < diff; i++)
                {
                    activeSkinToggles.Add(Instantiate(skinViewPrefab, skinViewContainer));
                }
            }
            if (viewModel.AvailableSkins.Length < activeSkinToggles.Count)
            {
                int diff = activeSkinToggles.Count - viewModel.AvailableSkins.Length;
                for (int i = 0; i < diff; i++)
                {
                    Destroy(activeSkinToggles.First().gameObject);
                    activeSkinToggles.RemoveAt(0);
                }
            }
            for (int i = 0; i < activeSkinToggles.Count; i++)
            {
                activeSkinToggles[i].Rebuild(viewModel, viewModel.AvailableSkins[i].Id);
            }

            defaultConfig.Rebuild(viewModel, null, true);

            //Map configs
            if (viewModel.SelectedConfig.MapConfigs.Length > mapConfigs.Count)
            {
                int diff = viewModel.SelectedConfig.MapConfigs.Length - mapConfigs.Count;
                for (int i = 0; i < diff; i++)
                {
                    mapConfigs.Add(Instantiate(mapConfigPrefab, mapConfigContainer));
                }
            }
            if (viewModel.SelectedConfig.MapConfigs.Length < mapConfigs.Count)
            {
                int diff = mapConfigs.Count - viewModel.SelectedConfig.MapConfigs.Length;
                for (int i = 0; i < diff; i++)
                {
                    Destroy(mapConfigs.First().gameObject);
                    mapConfigs.RemoveAt(0);
                }
            }
            for (int i = 0; i < mapConfigs.Count; i++)
            {
                mapConfigs[i].Rebuild(viewModel, viewModel.SelectedConfig.MapConfigs[i].Id);
            }
            LayoutRebuilder.MarkLayoutForRebuild(mapConfigContainer);
            //LayoutRebuilder.ForceRebuildLayoutImmediate(mapConfigContainer);

            MoonInfo[] selectableMoons = viewModel.Moons.Where((moon) => !viewModel.SelectedConfig.MapConfigs.Any((cfgMoon) => cfgMoon.Id.Equals(moon.Id))).ToArray();
            string[] selectableTags = viewModel.AvailableTags.Where((tag) => !viewModel.SelectedConfig.MapConfigs.Any((cfgMoon) => cfgMoon.Id.Equals(tag))).ToArray();
            moonDropdown.ClearOptions();
            moonDropdown.AddOptions(new List<string>() { "" });
            moonDropdown.AddOptions(selectableMoons.Select((moon) => moon.Name).ToList());
            moonDropdown.AddOptions(selectableTags.ToList());
            moonDropdown.onValueChanged.RemoveAllListeners();
            moonDropdown.onValueChanged.AddListener
            (
                (index) =>
                {
                    if (index != 0 && index <= selectableMoons.Length)
                    {
                        viewModel.AddMoonConfig(selectableMoons[index - 1].Id);
                    }
                    else if(index > selectableMoons.Length)
                    {
                        viewModel.AddMoonConfig(selectableTags[index - 1 - selectableMoons.Length]);
                    }

                }
            );
            if(EnemySkinRegistry.ClientSyncActive)
            {
                saveConfigButton.interactable = false;
                //storeButton.interactable = false;
                reapplyConfigButton.interactable = false;
            }
        }
    }
}
