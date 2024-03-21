using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AntlerShed.SkinRegistry.View
{
    public class AvailableSkinView : MonoBehaviour
    {
        [SerializeField]
        private Image skinAvatar;

        [SerializeField]
        private Image toggleOverlay;

        [SerializeField]
        private Toggle toggle;

        [SerializeField]
        private TextMeshProUGUI skinLabel;

        internal void Rebuild(ConfigurationViewModel viewModel, string skinId)
        {
            bool isSkinActive = viewModel.SelectedConfig.ActiveSkins.Contains(skinId);
            toggle.isOn = !isSkinActive;
            toggleOverlay.color = isSkinActive ? 
                new Color(toggleOverlay.color.r, toggleOverlay.color.g, toggleOverlay.color.b, 0.0f) : 
                new Color(toggleOverlay.color.r, toggleOverlay.color.g, toggleOverlay.color.b, 0.9f);
            try
            {
                skinLabel.text = viewModel.AvailableSkins.First((skin) => skin.Id.Equals(skinId)).Label;
                skinLabel.color = isSkinActive ? 
                    new Color(skinLabel.color.r, skinLabel.color.g, skinLabel.color.b, 1.0f) :
                    new Color(skinLabel.color.r, skinLabel.color.g, skinLabel.color.b, 0.4f);
            }
            catch (InvalidOperationException e) { }
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn)=>viewModel.SetSkinActive(skinId, !isOn));
        }
    }
}
