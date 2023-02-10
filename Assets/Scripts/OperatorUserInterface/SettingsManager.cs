using System;
using InputDevices;
using UnityEngine;

namespace OperatorUserInterface
{
    /// <summary>
    /// Opens and closes settings.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public GameObject Settings;
        public GameObject HUD;

        public void ChangeSettingsState()
        {
            bool settingsActive = !Settings.activeSelf;
            Settings.SetActive(settingsActive);
            HUD.SetActive(!settingsActive);
            
            // While settings are active, turn off robot motions
            // TODO: find out how to do it, this doesn't work
            // EnableControlManager.Instance.leftBioIKGroup.SetEnabled(!Settings.activeSelf);
            // EnableControlManager.Instance.rightBioIKGroup.SetEnabled(!Settings.activeSelf);
        }
        
        private void OnEnable()
        {
            InputSystem.OnLeftPrimaryButton += ChangeSettingsState;
        }
        
        private void OnDisable()
        {
            InputSystem.OnLeftPrimaryButton -= ChangeSettingsState;
        }
    }
}