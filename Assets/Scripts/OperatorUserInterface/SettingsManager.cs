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

        private Action[] buttonsRestore;

        public void ChangeSettingsState()
        {
            bool settingsActive = !Settings.activeSelf;
            Settings.SetActive(settingsActive);
            HUD.SetActive(!settingsActive);
            
            foreach (var interactorLineVisual in PointerRenderers)
            {
                // The default of CurvedUIInputModule is Right Hand. Even if usedHand is set to Both,
                // it prioritizes the right hand and input from the left doesn't get correctly processed.
                // Therefore, don't show the line visual from the left controller to not confuse the operator.
                // Possible future improvement: switch the line visual between hands depending on which
                // controller is currently active.
                if (!interactorLineVisual.gameObject.name.Contains("Left"))
                {
                    interactorLineVisual.enabled = settingsActive;
                }
            }
            
            // While settings are active, turn off robot motions
            RobotMotionManager.Instance.EnableMotion(!settingsActive);

            // While settings are active, remove actions from other buttons
            if (settingsActive)
            {
                buttonsRestore = InputSystem.StripActions();
                InputSystem.OnLeftPrimaryButton += ChangeSettingsState;
            }
            else
            {
                InputSystem.RestoreActions(buttonsRestore);
            }
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