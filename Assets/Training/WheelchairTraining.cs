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
            BACKWARD,
            FORWARD,
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
            ariaTrigger.TriggerEnterCallback((pos) =>
            {
                var diff = (pos - ariaNavigation.target).magnitude;
                Debug.Log($"Aria Trigger, posDiff: {diff}, waiting: {waitingForTrigger}");
                if (diff < 1f && waitingForTrigger)
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
            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start_intro, queue: false);
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start, queue: true,
                    onStart: () =>
                    {
                        ariaNavigation.target = backwardGoal.position;
                        TutorialSteps.PublishNotification("Press the left pedal to go backward");
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
                );
            };

            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.forward,
                    onStart: () =>
                    {
                        ariaNavigation.target = forwardGoal.position;
                        waitingForTrigger = true;
                        TutorialSteps.PublishNotification("Press the right pedal to go forward", rudderWheelchairAudio.backwards.length);
                    },
                    onEnd: () =>
                    {
                    }
                );

            };

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_left,
                    onStart: () =>
                    {
                        ariaNavigation.target = turnLeftGoal.position;
                        TutorialSteps.PublishNotification("Turn left", rudderWheelchairAudio.turn_left.length);
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
                    );

            };

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_right_intro);
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.turn_right, queue: true,
                    onStart: () =>
                    {
                        ariaNavigation.target = turnRightGoal.position;
                        TutorialSteps.PublishNotification("Turn right", rudderWheelchairAudio.turn_right_intro.length + 2);
                        waitingForTrigger = true;
                    }
                );
            };

#else
            stateMachine.onEnter[State.BACKWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(rudderWheelchairAudio.start_intro, queue: false);
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.howto, queue: true);
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.back, queue: true,
                    onStart: () =>
                    {
                        ariaNavigation.target = backwardGoal.position;
                        TutorialSteps.PublishNotification("Use the left joystick to drive back");
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
            );

            };

            stateMachine.onEnter[State.FORWARD] = (state) =>
            {
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.front, queue: false,
                    onStart: () =>
                    {
                        ariaNavigation.target = forwardGoal.position;
                        TutorialSteps.PublishNotification("Drive forwards");
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
                );
            };

            stateMachine.onEnter[State.TURN_RIGHT] = (state) =>
            {

                audioManager.ScheduleAudioClip(joystickWheelchairAudio.right, queue: false,
                    onStart: () =>
                    {
                        ariaNavigation.target = turnRightGoal.position;
                        TutorialSteps.PublishNotification("Turn right", joystickWheelchairAudio.right.length + 2);
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
                );
            };

            stateMachine.onEnter[State.TURN_LEFT] = (state) =>
            {
                audioManager.ScheduleAudioClip(joystickWheelchairAudio.left,
                    onStart: () =>
                    {
                        ariaNavigation.target = turnLeftGoal.position;
                        TutorialSteps.PublishNotification("Turn left", joystickWheelchairAudio.left.length + 2);
                        waitingForTrigger = true;
                    },
                    onEnd: () =>
                    {
                    }
                );
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
