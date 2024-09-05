using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


namespace AntlerShed.SkinRegistry.View
{
    public class MapConfigurationView : MonoBehaviour
    {
        [SerializeField]
        private SkinFrequencyView skinFreqPrefab;

        [SerializeField]
        private GameObject skinFreqContainer;

        private List<SkinFrequencyView> freqViews = new List<SkinFrequencyView>();

        [SerializeField]
        private TextMeshProUGUI moonLabel;

        [SerializeField]
        private Slider vanillaSlider;

        [SerializeField]
        private TextMeshProUGUI vanillaLabel;

        [SerializeField]
        private Button removeMoonButton;

        [SerializeField]
        private Button collapseButton;

        [SerializeField]
        private Button expandButton;

        [SerializeField]
        private RectTransform collapseRoot;

        [SerializeField]
        private RectTransform expandRoot;

        [SerializeField]
        private TMP_Dropdown addSkinDropdown;

        private void Start()
        {
            collapseButton.onClick.AddListener
            (
                () =>
                {
                    skinFreqContainer.SetActive(false);
                    collapseRoot.gameObject.SetActive(false);
                    expandRoot.gameObject.SetActive(true);
                }
            );

            expandButton.onClick.AddListener
            (
                () =>
                {
                    skinFreqContainer.SetActive(true);
                    collapseRoot.gameObject.SetActive(true);
                    expandRoot.gameObject.SetActive(false);
                }
            );
        }

        internal void Rebuild(ConfigurationViewModel viewModel, string moonId, bool isDefault = false)
        {
            try
            {
                MapConfiguration cfg = isDefault ? viewModel.SelectedConfig.DefaultConfiguration : viewModel.SelectedConfig.MapConfigs.First((moon) => moon.Id.Equals(moonId));
                try
                {
                    moonLabel.text = isDefault ? "Default" : viewModel.Moons.First((moon)=>moon.Id.Equals(moonId)).Name;
                }
                catch(InvalidOperationException e)
                {
                    moonLabel.text = isDefault ? "Default" : cfg.Id;
                }
                
                Skin[] selectable = viewModel.AvailableSkins.Where((skin) => !cfg.Distribution.Any((skCfg) => skCfg.SkinId.Equals(skin.Id))).ToArray();
                addSkinDropdown.ClearOptions();
                //This wacky little lad makes it so the onValueChanged callback fires. I don't know how to get away with hiding it.
                addSkinDropdown.AddOptions(new List<string>{ "" });
                addSkinDropdown.AddOptions(selectable.Select((skin)=>skin.Label).ToList());
                addSkinDropdown.onValueChanged.RemoveAllListeners();
                addSkinDropdown.value = 0;
                addSkinDropdown.onValueChanged.AddListener
                (
                    (index) =>
                    {
                        if(index!=0)
                        {
                            if (isDefault)
                            {
                                viewModel.AddSkinToDefault(selectable[index - 1].Id);
                            }
                            else
                            {
                                viewModel.AddSkinToMoon(selectable[index - 1].Id, moonId);
                            }
                        }
                    }
                );

                if (isDefault)
                {
                    removeMoonButton.gameObject.SetActive(false);
                }
                else
                {
                    removeMoonButton.onClick.AddListener(() => viewModel.RemoveMoonConfig(moonId));
                }

                float vanillaFreq = viewModel.SelectedSpawn == SpawnLocation.INDOOR ? cfg.IndoorVanillaFrequency : cfg.OutdoorVanillaFrequency;
                float total = cfg.Distribution.Aggregate(0.0f, (float value, SkinConfigEntry entry) => value + (viewModel.SelectedSpawn == SpawnLocation.INDOOR ? entry.IndoorFrequency : entry.OutdoorFrequency));
                total += vanillaFreq;

                //Default distribution
                vanillaLabel.text = $"Vanilla: {(total > 0.0f ? (int)Mathf.Round(vanillaFreq / total * 100.0f) : 100)}%";
                vanillaSlider.onValueChanged.RemoveAllListeners();
                vanillaSlider.value = viewModel.SelectedSpawn == SpawnLocation.INDOOR ? cfg.IndoorVanillaFrequency : cfg.OutdoorVanillaFrequency;
                vanillaSlider.onValueChanged.AddListener
                (
                    (value) =>
                    {
                        if(isDefault)
                        {
                            viewModel.SetDefaultVanillaRatio(value);
                        }
                        else
                        {
                            viewModel.SetMapVanillaRatio(moonId, value);
                        }
                    }
                );

                //Moon distributions
                if (cfg.Distribution.Length > freqViews.Count)
                {
                    int diff = cfg.Distribution.Length - freqViews.Count;
                    for (int i = 0; i < diff; i++)
                    {
                        freqViews.Add(Instantiate(skinFreqPrefab, skinFreqContainer.transform));
                    }
                }

                if (cfg.Distribution.Length < freqViews.Count)
                {
                    int diff = freqViews.Count - cfg.Distribution.Length;
                    for (int i = 0; i < diff; i++)
                    {
                        Destroy(freqViews.First().gameObject);
                        freqViews.RemoveAt(0);
                    }
                }

                for (int i = 0; i < freqViews.Count; i++)
                {
                    freqViews[i].Rebuild(viewModel, cfg.Distribution[i], moonId, isDefault, total);
                }

                if (EnemySkinRegistry.ClientSyncActive)
                {
                    addSkinDropdown.interactable = false;
                    vanillaSlider.interactable = false;
                    vanillaSlider.fillRect.GetComponent<Image>().canvasRenderer.SetAlpha(0.5f);
                }
            }
            catch(InvalidOperationException e) { }

        }
    }
}
