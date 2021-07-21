using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training
{
    public class WheelchairTraining : Automaton<WheelchairTraining.State>
    {
        public enum State
        {
            START,
            FORWARD,
            BACKWARD,
            TURN_LEFT,
            TURN_RIGHT,
            DONE
        }

        public Transform initialGoal, forwardGoal, backwardGoal, turnLeftGoal, turnRightGoal;
        public CollisionTrigger ariaTrigger;
        public AvatarNavigation ariaNavigation;

        public AudioClips.DriveWheelchair driveWheelchairAudio;

        private List<System.Action> onDoneCallbacks;

        // Start is called before the first frame update
        void Start()
        {

            currentState = State.START;
            onDoneCallbacks = new List<System.Action>();

            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.start, queue: true);
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.forward, queue: true);
                TutorialSteps.PublishNotification("Press the right pedal to go forward");

                ariaNavigation.target = forwardGoal.position;
                ariaTrigger.TriggerEnterCallback(() => Next(), once: true);
            };

            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.backwards);
                TutorialSteps.PublishNotification("Press the left pedal to go backward");
                ariaNavigation.target = backwardGoal.position;
                ariaTrigger.TriggerEnterCallback(() => Next(), once: true);
            };

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.turn_right);
                TutorialSteps.PublishNotification("Turn right");
                ariaNavigation.target = turnRightGoal.position;
                ariaTrigger.TriggerEnterCallback(() => Next(), once: true);
            };

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.turn_left);
                TutorialSteps.PublishNotification("Turn left");
                ariaNavigation.target = turnLeftGoal.position;
                ariaTrigger.TriggerEnterCallback(() => Next(), once: true);
            };

            stateMachine.onEnter[State.DONE] = (state) =>
            {
                ariaNavigation.target = initialGoal.position;
                foreach (var callback in onDoneCallbacks)
                {
                    callback();
                }
            };

        }

        public void Next()
        {
            currentState++;
            Debug.Log($"Current Wheelchair Tutorial Step {currentState}");
        }

        public void StartTraining() => currentState = State.FORWARD;

        public void StopTraining() => currentState = State.DONE;

        public void OnDone(System.Action callback) => onDoneCallbacks.Add(callback);
    }
}
