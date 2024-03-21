using AntlerShed.SkinRegistry;
using AntlerShed.SkinRegistry.View;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
    private RectTransform activeSkinsToolTip;

    [SerializeField]
    private RectTransform mapConfigToolTip;

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

    internal void Init(ConfigurationViewModel viewModel)
    {
        saveConfigButton.onClick.AddListener(()=>viewModel.Save());
        nextEnemyButton.onClick.AddListener(() => viewModel.SwitchToNextEnemy());
        previousEnemyButton.onClick.AddListener(() => viewModel.SwitchToPreviousEnemy());
        exitButton.onClick.AddListener
        (
            () => 
            { 
                if(viewModel.UnsavedChanges)
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
            ()=>
            {
                exitDialog.gameObject.SetActive(false);
            }
        );
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
        Rebuild(viewModel);
    }

    internal void Rebuild(ConfigurationViewModel viewModel)
    {
        saveConfigButton.enabled = viewModel.UnsavedChanges;

        enemyLabel.text = viewModel.SelectedConfig.EnemyId==null ? "<null enemy>" : viewModel.Enemies.ContainsKey(viewModel.SelectedConfig.EnemyId) ? viewModel.Enemies[viewModel.SelectedConfig.EnemyId].Name : viewModel.SelectedConfig.EnemyId;

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
        for(int i = 0; i < activeSkinToggles.Count; i++)
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

        MoonInfo[] selectableMoons = viewModel.Moons.Where((moon) => !viewModel.SelectedConfig.MapConfigs.Any((cfgMoon)=> cfgMoon.Id.Equals(moon.Id))).ToArray();
        moonDropdown.ClearOptions();
        moonDropdown.AddOptions(new List<string>() { "" });
        moonDropdown.AddOptions(selectableMoons.Select((moon) => moon.Name).ToList());
        moonDropdown.onValueChanged.RemoveAllListeners();
        moonDropdown.onValueChanged.AddListener
        (
            (index) =>
            {
                if (index != 0)
                {
                    viewModel.AddMoonConfig(selectableMoons[index - 1].Id);
                }
            }
        );
    }
}
