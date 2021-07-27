using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training
{
    public class ArmLengthCalibration : Automaton<ArmLengthCalibration.State>
    {
        public enum State
        {
            START,
            LEFT,
            RIGHT,
            DONE
        }

        public Transform leftHand, leftHandObjective, rightHand, rightHandObjective, leftArm, rightArm;

        private Callbacks<State> onDoneCallbacks = new Callbacks<State>();
        private bool latch = false;

        void Start()
        {
            currentState = State.START;

            // get objectives
            foreach (var comp in PlayerRig.Instance.gameObject.GetComponentsInChildren<XROffset>())
            {
                if (comp.isRight)
                {
                    rightHandObjective = comp.transform;
                    rightHand = comp.orientation.target;
                }
                else
                {
                    leftHandObjective = comp.transform;
                    leftHand = comp.orientation.target;
                }
            }
            // get arms
            leftArm = GameObject.Find("shoulder_left_axis0").transform;
            rightArm = GameObject.Find("shoulder_right_axis0").transform;

            stateMachine.onEnter[State.LEFT] = (state) =>
            {
                TutorialSteps.PublishNotification("Strech your left arm fully");
                TutorialSteps.PublishNotification("Left thumbs up to calibrate");
                UserInteractionManager.Instance.Confirm((b) =>
                {
                    FitLeft();
                    Next();
                }, once: true);
            };

            stateMachine.onEnter[State.RIGHT] = (state) =>
            {
                TutorialSteps.PublishNotification("Strech your right arm fully");
                TutorialSteps.PublishNotification("Right thumbs up to calibrate");
                UserInteractionManager.Instance.Confirm((b) =>
                {
                    FitRight();
                    Next();
                }, once: true);
            };

            stateMachine.onEnter[State.DONE] = (state) =>
            {
                onDoneCallbacks.Call(State.DONE);
            };
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.F))
            {
                latch = true;
            }
            else if (latch)
            {
                Next();
                latch = false;
            }
        }

        private void Fit(Transform hand, Transform objective, Transform target)
        {
            float factor = objective.position.x / hand.position.x;
            Debug.Log($"ArmLengthCalibration: Scale {target} by {factor}");
            target.localScale *= factor;
        }

        public void FitLeft()
        {
            Fit(leftHand, leftHandObjective, leftArm);
        }

        public void FitRight()
        {
            Fit(rightHand, rightHandObjective, rightArm);
        }

        public void StartCalibration()
        {
            currentState = State.LEFT;
        }

        public void StopCalibration() => currentState = State.DONE;

        public void OnDone(System.Action<State> callback, bool once = false) => onDoneCallbacks.Add(callback, once);
    }
}
