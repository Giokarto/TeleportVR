using InputDevices.VRControllers;
using UnityEngine;
using Widgets;

namespace Tutorial
{
    /// <summary>
    /// Move the left robot arm, touch an object.
    /// </summary>
    public class LeftArm : Step
    {
        public override void Enter()
        {
            correctedAtThisStep = false;
            audioManager.ScheduleAudioClip(controllerAudio.leftArm, queue: false);
            WidgetFactory.Instance.CreateToastrWidget("Press and hold the index trigger and try moving your left arm", controllerAudio.leftArm.length);
            
            audioManager.ScheduleAudioClip(controllerAudio.leftBall, queue: true,
                onStart: () => GameObject.Find("HandCollectableLeft").gameObject.SetActive(true)); // TODO: create from prefab instead
                
            VRControllerInputSystem.OnGripChange += OnGripLeftArm;
        }
        
        void OnGripLeftArm(float l, float r)
        {
            if (l > 0.5)
            {
                CorrectUser("index");
            }
        }

        public override void Exit()
        {
            VRControllerInputSystem.OnGripChange -= OnGripLeftArm;
            GameObject.Find("HandCollectableLeft").gameObject.SetActive(false);
        }

        public override void Break()
        {
            
        }

        public override void Continue()
        {
            Enter();
        }
    }
}