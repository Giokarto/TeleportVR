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

        private Callbacks<State> onDoneCallbacks = new Callbacks<State>();
        private bool waitingForTrigger = false;

        // Start is called before the first frame update
        void Start()
        {

            currentState = State.START;
            ariaTrigger.TriggerEnterCallback((move) =>
            {
                Debug.Log($"move: {move}");
                if (move < 0.01f && waitingForTrigger)
                {
                    Next();
                }
            });

            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.start, queue: true);
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.forward, queue: true,
                    onStart: () => TutorialSteps.PublishNotification("Press the right pedal to go forward", driveWheelchairAudio.forward.length)
                    );

                ariaNavigation.target = forwardGoal.position;
                waitingForTrigger = true;
            };
            stateMachine.onExit[State.FORWARD] = (state) => waitingForTrigger = false;

            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.backwards,
                    onStart: () => TutorialSteps.PublishNotification("Press the left pedal to go backward", driveWheelchairAudio.backwards.length)
                    );

                ariaNavigation.target = backwardGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };
            stateMachine.onExit[State.BACKWARD] = (state) => waitingForTrigger = false;

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.turn_right,
                    onStart: () => TutorialSteps.PublishNotification("Turn right", driveWheelchairAudio.turn_right.length)
                    );

                ariaNavigation.target = turnRightGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };
            stateMachine.onExit[State.TURN_RIGHT] = (state) => waitingForTrigger = false;

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                TutorialSteps.Instance.audioManager.ScheduleAudioClip(driveWheelchairAudio.turn_left,
                    onStart: () => TutorialSteps.PublishNotification("Turn left", driveWheelchairAudio.turn_left.length)
                    );

                ariaNavigation.target = turnLeftGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };
            stateMachine.onExit[State.TURN_LEFT] = (state) => waitingForTrigger = false;

            stateMachine.onEnter[State.DONE] = (state) =>
            {
                onDoneCallbacks.Call(State.DONE);
            };
        }

        public void StartTraining()
        {
            currentState = State.START;
            Next();
        }

        public void StopTraining() => currentState = State.DONE;

        public void OnDone(System.Action<State> callback, bool once = false) => onDoneCallbacks.Add(callback, once);
    }
}

