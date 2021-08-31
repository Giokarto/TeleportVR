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
        public AudioManager audioManager;

        public AudioClips.RudderWheelchair rudderWheelchairAudio;
        public AudioClips.JoystickWheelchair joystickWheelchairAudio;

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

            StateManager.Instance.onStateChangeTo[StateManager.States.HUD].Add((s) => StopTraining(), once: true);

            // states independent of input device
            stateMachine.onExit[State.FORWARD] = (state) => waitingForTrigger = false;
            stateMachine.onExit[State.BACKWARD] = (state) => waitingForTrigger = false;
            stateMachine.onExit[State.TURN_RIGHT] = (state) => waitingForTrigger = false;
            stateMachine.onExit[State.TURN_LEFT] = (state) => waitingForTrigger = false;
            stateMachine.onEnter[State.DONE] = (state) =>
            {
                onDoneCallbacks.Call(State.DONE);
            };

#if RUDDER
            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start_intro, queue: true);
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start, queue: true,
                    onStart: () => TutorialSteps.PublishNotification("Press the right pedal to go forward", rudderWheelchairAudio.start.length + 2)
                );

                ariaNavigation.target = forwardGoal.position;
                waitingForTrigger = true;
            };


            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.backwards,
                    onStart: () => TutorialSteps.PublishNotification("Press the left pedal to go backward", rudderWheelchairAudio.backwards.length)
                );

                ariaNavigation.target = backwardGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_left,
                    onStart: () => TutorialSteps.PublishNotification("Turn left", rudderWheelchairAudio.turn_left.length)
                    );

                ariaNavigation.target = turnLeftGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_right_intro);
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_right, queue: true,
                    onStart: () => TutorialSteps.PublishNotification("Turn right", rudderWheelchairAudio.turn_right_intro.length + 2)
                );

                ariaNavigation.target = turnRightGoal.position;
                waitingForTrigger = true;
                //ariaTrigger.TriggerEnterCallback((t) => Next(), once: true);
            };

#else
            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start_intro, queue: false);
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.howto, queue: true,
                    onStart: () => TutorialSteps.PublishNotification("Use the left joystick to drive around", joystickWheelchairAudio.howto.length + 2)
                );

                audioManager.ScheduleAudioClip(joystickWheelchairAudio.front, queue: true);
                ariaNavigation.target = forwardGoal.position;
                waitingForTrigger = true;
            };


            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.back, queue: false,
                    onStart: () => TutorialSteps.PublishNotification("Come get me in the back now"));

                ariaNavigation.target = backwardGoal.position;
                waitingForTrigger = true;
            };

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {

                audioManager.ScheduleAudioClip(joystickWheelchairAudio.right, queue: false,
                    onStart: () => TutorialSteps.PublishNotification("Turn right", joystickWheelchairAudio.right.length + 2)
                );

                ariaNavigation.target = turnRightGoal.position;
                waitingForTrigger = true;
            };

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.left,
                    onStart: () => TutorialSteps.PublishNotification("Turn left", joystickWheelchairAudio.left.length + 2)
                    );

                ariaNavigation.target = turnLeftGoal.position;
                waitingForTrigger = true;
            };
#endif


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
