using InputDevices;
using UnityEngine;
using Widgets;

namespace Tutorial
{
    /// <summary>
    /// Try to move head and nod.
    /// </summary>
    public class MoveHead : Step
    {
        public override void Enter()
        {
            audioManager.ScheduleAudioClip(miscAudio.head,
                onStart: () => WidgetFactory.Instance.CreateToastrWidget("Try moving your head around", miscAudio.head.length)
            );
            audioManager.ScheduleAudioClip(miscAudio.nod, queue: true,
                onStart: () =>
                {
                    WidgetFactory.Instance.CreateToastrWidget("Give me a nod to continue", miscAudio.nod.length + 2);
                    VRGestureRecognizer.Nodded += OnNodded;
                });
        }
        
        private void OnNodded()
        {
            Debug.Log("user nodded");
            NextStep();
        }

        public override void Exit()
        {
            VRGestureRecognizer.Nodded -= OnNodded;
        }

        public override void Break()
        {
            VRGestureRecognizer.Nodded -= OnNodded;
        }

        public override void Continue()
        {
            Enter();
        }
    }
}