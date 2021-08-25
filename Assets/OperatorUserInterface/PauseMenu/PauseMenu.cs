using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PauseMenu
{
    public class PauseMenu : Singleton<PauseMenu>
    {
        public bool show;
        public GameObject child;

        [Header("UI Elements")]
        public TouchButton switchScene;

        public GameObject matchHands;
        public Widgets.Completion matchHandsCompletion;
        public float minSwitchWait = 3f;


        private bool switchScenePressed = false;
        private bool oldWheelchairVisibility;


        // Start is called before the first frame update
        void Start()
        {
            // recover values presence detector when this script is reloaded
            show = RudderPedals.PresenceDetector.Instance.isPaused;
            switchScenePressed = RudderPedals.PresenceDetector.Instance.isPaused;

            // buttons init
            switchScene.OnTouchEnter((t) =>
            {
                Debug.Log($"switch Scene {switchScenePressed}");
                if (switchScenePressed) return;
                if (Time.time - StateManager.Instance.lastSwitch < minSwitchWait)
                {
                    Debug.Log($"Attempted to switch scenes but button interaction was too early by {minSwitchWait - Time.time + StateManager.Instance.lastSwitch}s");
                    return;
                }

                switchScenePressed = true;
                switch (StateManager.Instance.currentState)
                {
                    case StateManager.States.Training:
                        Debug.Log("Changing scene to HUD");
                        StateManager.Instance.GoToState(StateManager.States.HUD, () =>
                        {
                            oldWheelchairVisibility = WheelchairStateManager.Instance.visible;
                            WheelchairStateManager.Instance.SetVisibility(true,
                                StateManager.Instance.currentState == StateManager.States.HUD ? WheelchairStateManager.HUDAlpha : 1);
                        });
                        break;
                    case StateManager.States.HUD:
                        Debug.Log("Changing scene to Traning");
                        StateManager.Instance.GoToState(StateManager.States.Training);
                        break;
                }
            });
            switchScene.OnTouchExit((t) =>
            {
                switchScenePressed = false;
            });
        }


        // Update is called once per frame
        void Update()
        {
            if (Application.IsPlaying(gameObject))
            {
                switch (StateManager.Instance.currentState)
                {
                    case StateManager.States.Training:
                        switchScene.text = "Control";
                        break;
                    case StateManager.States.HUD:
                        switchScene.text = "Training";
                        break;
                }
            }
            child.SetActive(show);
        }


        private void OnDestroy()
        {
            switchScene.ClearOnTouchEnter();
            switchScene.ClearOnTouchExit();
        }
    }

}
