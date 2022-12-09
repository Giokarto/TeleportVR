using UnityEngine;
using Widgets;

namespace Training
{
    public class Welcome : Step
    {
        /// <summary>
        /// Welcome to the tutorial, info.
        /// </summary>
        public override void Enter()
        {
            audioManager.ScheduleAudioClip(miscAudio.welcome, queue: false,
                onStart: () => WidgetFactory.Instance.CreateToastrWidget("Welcome to Teleport VR!", miscAudio.welcome.length)
            );
            audioManager.ScheduleAudioClip(miscAudio.imAria, queue: true,
                onStart: () => WidgetFactory.Instance.CreateToastrWidget("I am Amala - your personal telepresence trainer.", miscAudio.imAria.length + 2),
                onEnd: () => NextStep()
            );
        }

        public override void Exit()
        {
            
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