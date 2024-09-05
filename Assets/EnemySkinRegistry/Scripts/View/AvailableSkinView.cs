using System;
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
                Skin skin = viewModel.AvailableSkins.First((skin) => skin.Id.Equals(skinId));
                skinLabel.text = skin.Label;
                skinLabel.color = isSkinActive ? 
                    new Color(skinLabel.color.r, skinLabel.color.g, skinLabel.color.b, 1.0f) :
                    new Color(skinLabel.color.r, skinLabel.color.g, skinLabel.color.b, 0.4f);
                if(skin.Icon == null)
                {
                    skinAvatar.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0.0f, 0.0f, 1.0f, 1.0f), Vector2.zero);
                }
                else
                {
                    skinAvatar.sprite = Sprite.Create(skin.Icon, new Rect(0.0f, 0.0f, skin.Icon.width, skin.Icon.height), skinAvatar.rectTransform.pivot);
                }
                if(EnemySkinRegistry.ClientSyncActive)
                {
                    skinAvatar.canvasRenderer.SetAlpha(0.5f);
                    skinLabel.color = new Color(skinLabel.color.r, skinLabel.color.g, skinLabel.color.b, 0.4f);
                }
                
            }
            catch (InvalidOperationException e) { }
            toggle.onValueChanged.RemoveAllListeners();
            toggle.onValueChanged.AddListener((isOn)=>viewModel.SetSkinActive(skinId, !isOn));
            toggle.interactable = !EnemySkinRegistry.ClientSyncActive;
        }
    }
}
