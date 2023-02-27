using System;
using InputDevices;
using RobodyControl;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace OperatorUserInterface
{
    /// <summary>
    /// Opens and closes settings.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public GameObject Settings;
        public GameObject HUD;

        private XRInteractorLineVisual[] PointerRenderers;

        public void ChangeSettingsState()
        {
            bool settingsActive = !Settings.activeSelf;
            Settings.SetActive(settingsActive);
            HUD.SetActive(!settingsActive);
            
            foreach (var interactorLineVisual in PointerRenderers)
            {
                interactorLineVisual.enabled = settingsActive;
            }
            
            // While settings are active, turn off robot motions
            RobotMotionManager.Instance.EnableMotion(!settingsActive);
        }

        private void Start()
        {
            PointerRenderers = FindObjectsOfType<XRInteractorLineVisual>();
            foreach (var interactorLineVisual in PointerRenderers)
            {
                interactorLineVisual.enabled = false;
            }
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