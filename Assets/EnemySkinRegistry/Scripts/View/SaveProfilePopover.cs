
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AntlerShed.SkinRegistry.View
{
    public class SaveProfilePopover : MonoBehaviour
    {
        [SerializeField]
        private Toggle overwrite;

        [SerializeField]
        private TMP_Dropdown profileSelector;

        [SerializeField]
        private TMP_InputField profileName;

        [SerializeField]
        private Button cancelButton;

        [SerializeField]
        private Button saveButton;

        [SerializeField]
        private TMP_Text errorText;

        [SerializeField]
        private TMP_Text warningText;

        internal void Init(ConfigurationViewModel viewModel)
        {
            cancelButton.onClick.AddListener(() => gameObject.SetActive(false));
            saveButton.onClick.AddListener
            (
                () =>
                {
                    if(overwrite.isOn)
                    {
                        viewModel.OverwriteProfile(profileSelector.itemText.text);
                    }
                    else
                    {
                        viewModel.SaveProfile(profileName.text);
                    }
                    gameObject.SetActive(false);
                }
            );
            overwrite.onValueChanged.AddListener
            (
                (value) =>
                {
                    profileSelector.gameObject.SetActive(value);
                    profileName.gameObject.SetActive(!value);
                }
            );
            viewModel.profilesLoaded += () => Rebuild(viewModel);
            viewModel.profileNameError += () => Rebuild(viewModel);

        }

        internal void Rebuild(ConfigurationViewModel viewModel)
        {
            profileSelector.ClearOptions();
            profileSelector.AddOptions(viewModel.LoadedProfiles.ToList());
            if(viewModel.ProfileAssignmentFailed)
            {
                errorText.gameObject.SetActive(true);
                warningText.gameObject.SetActive(false);
            }
            else if(!viewModel.OriginalProfileName.Equals(viewModel.ReassignedProfileName))
            {
                errorText.gameObject.SetActive(false);
                warningText.gameObject.SetActive(true);
                warningText.text = $"Original profile name \"{viewModel.OriginalProfileName}\" contained special characters and has been reassigned to \"{viewModel.ReassignedProfileName}\" for storage and future use.";
            }
            else
            {
                errorText.gameObject.SetActive(false);
                warningText.gameObject.SetActive(false);
            }
        }
    }
}
