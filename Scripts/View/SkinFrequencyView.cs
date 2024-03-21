using System;
using System.Collections;
using System.Collections.Generic;
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

        internal void Rebuild(ConfigurationViewModel viewModel, SkinConfigEntry skinEntry, string moon, bool isDefault)
        {
            slider.onValueChanged.RemoveAllListeners();
            slider.value = skinEntry.Frequency;
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
            percentage.text = $"- {(int)(skinEntry.Frequency * 100)}%";
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
        }
    }
}
