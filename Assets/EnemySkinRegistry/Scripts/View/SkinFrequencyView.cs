using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AntlerShed.SkinRegistry.View
{
    public class SkinFrequencyView : MonoBehaviour
    {
        [SerializeField]
        private Slider slider;

        [SerializeField]
        private Button removeButton;

        [SerializeField]
        private TextMeshProUGUI label;

        [SerializeField]
        private TextMeshProUGUI percentage;

        internal void Rebuild(ConfigurationViewModel viewModel, SkinConfigEntry skinEntry, string moon, bool isDefault, float total)
        {
            float frequency = viewModel.CurrentSpawn == SpawnLocation.INDOOR ? skinEntry.IndoorFrequency : skinEntry.OutdoorFrequency;
            slider.onValueChanged.RemoveAllListeners();
            slider.value = frequency;
            slider.onValueChanged.AddListener
            (
                (value) =>
                {
                    if (isDefault)
                    {
                        viewModel.AdjustDefaultSkinWeight(skinEntry.SkinId, value);
                    }
                    else
                    {
                        viewModel.AdjustMoonSkinWeight(moon, skinEntry.SkinId, value);
                    }
                }
            );
            try
            {
                label.text = $"{viewModel.AvailableSkins.First((skin) => skin.Id.Equals(skinEntry.SkinId)).Label}";
            }
            catch(InvalidOperationException e)
            {
                label.text = $"{skinEntry.SkinId}";
            }
            percentage.text = $": {(total > 0.0f ? (int)Mathf.Round(frequency / total * 100.0f) : 0)}%";
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener
            (
                () => 
                {
                    if (isDefault)
                    {
                        viewModel.RemoveSkinFromDefault(skinEntry.SkinId);
                    }
                    else
                    {
                        viewModel.RemoveSkinFromMoon(skinEntry.SkinId, moon);
                    }
                    
                }
            );
            if (EnemySkinRegistry.ClientSyncActive)
            {
                slider.interactable = false;
                slider.fillRect.GetComponent<Image>().canvasRenderer.SetAlpha(0.5f);
            }
        }
    }
}
