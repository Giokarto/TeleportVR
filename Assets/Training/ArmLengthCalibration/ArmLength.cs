using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training.Calibration.ArmLength
{
    public class ArmLength : Automaton<ArmLength.State>
    {
        public enum State
        {
            START,
            RIGHT_SHOULDER_TOUCH,
            LEFT_SHOULDER_TOUCH,
            LEFT_SCALE,
            RIGHT_SCALE,
            DONE
        }

        public Transform leftHand, leftHandObjective, rightHand, rightHandObjective, leftShoulder, rightShoulder,
            leftArmTouchpoint, rightArmTouchpoint;

        public StayInVolume leftShoulderVolume, rightShoulderVolume;

        private Callbacks<State> onDoneCallbacks = new Callbacks<State>();
        private bool latch = false;
        private const string FQN = "Training.Calibration.ArmLength.ArmLength";

        void Start()
        {
            currentState = State.START;
            Load();

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
            leftShoulder = GameObject.Find("shoulder_left_axis0").transform;
            rightShoulder = GameObject.Find("shoulder_right_axis0").transform;

            #region StateDefinitions
            stateMachine.onEnter[State.RIGHT_SHOULDER_TOUCH] = (state) =>
            {
                TutorialSteps.PublishNotification("Touch your right shoulder with your left arm");

                rightShoulderVolume.OnDone((b) =>
                {
                    leftArmTouchpoint.position = new Vector3(
                        leftHandObjective.position.x,
                        leftShoulder.position.y,
                        leftShoulder.position.z
                    );
                    Next();
                }, once: true);
                rightShoulderVolume.StartWaiting();
            };
            stateMachine.onExit[State.RIGHT_SHOULDER_TOUCH] = (state) =>
            {
                rightShoulderVolume.StopWaiting();
            };

            stateMachine.onEnter[State.LEFT_SHOULDER_TOUCH] = (state) =>
            {
                TutorialSteps.PublishNotification("Touch your left shoulder with your right arm");

                leftShoulderVolume.OnDone((b) =>
                {
                    rightArmTouchpoint.position = new Vector3(
                        rightHandObjective.position.x,
                        rightShoulder.position.y,
                        rightShoulder.position.z
                    );
                    Next();
                }, once: true);
                leftShoulderVolume.StartWaiting();
            };
            stateMachine.onExit[State.LEFT_SHOULDER_TOUCH] = (state) =>
            {
                leftShoulderVolume.StopWaiting();
            };


            stateMachine.onEnter[State.LEFT_SCALE] = (state) =>
            {
                TutorialSteps.PublishNotification("Strech your left arm fully");
                TutorialSteps.PublishNotification("Left thumbs up to calibrate");
                UserInteractionManager.Instance.Confirm((b) =>
                {
                    FitLeft();
                    Next();
                }, left: true, once: true);
            };

            stateMachine.onEnter[State.RIGHT_SCALE] = (state) =>
            {
                TutorialSteps.PublishNotification("Strech your right arm fully");
                TutorialSteps.PublishNotification("Right thumbs up to calibrate");
                UserInteractionManager.Instance.Confirm((b) =>
                {
                    FitRight();
                    Next();
                }, left: false, once: true);
            };

            stateMachine.onEnter[State.DONE] = (state) =>
            {
                Save();
                onDoneCallbacks.Call(State.DONE);
            };
            #endregion
        }

        private void Fit(Transform hand, Transform objective, Transform shoulder, Transform touchPoint)
        {

            float origArmLength = (hand.position - touchPoint.position).magnitude;
            float newArmLength = (objective.position - touchPoint.position).magnitude;
            float factor = newArmLength / origArmLength;
            Debug.Log($"ArmLengthCalibration: Scale {shoulder} by {factor}");
            shoulder.localScale *= factor;
        }

        public void FitLeft()
        {
            Fit(leftHand, leftHandObjective, leftShoulder, rightArmTouchpoint);
        }

        public void FitRight()
        {
            Fit(rightHand, rightHandObjective, rightShoulder, leftArmTouchpoint);
        }

        public void StartCalibration()
        {
            currentState = State.START;
            Next();
        }

        public void StopCalibration() => currentState = State.DONE;

        public void OnDone(System.Action<State> callback, bool once = false) => onDoneCallbacks.Add(callback, once);


        private void Save()
        {
            PlayerPrefX.SetVector3($"{FQN}_leftShoulderScale", leftShoulder.localScale);
            PlayerPrefX.SetVector3($"{FQN}_leftArmTouchPoint_position", leftArmTouchpoint.position);
            PlayerPrefX.SetVector3($"{FQN}_rightShoulderScale", rightShoulder.localScale);
            PlayerPrefX.SetVector3($"{FQN}_rightArmTouchPoint_position", rightArmTouchpoint.position);
        }

        private void Load()
        {
            leftShoulder.localScale = PlayerPrefX.GetVector3($"{FQN}_leftShoulderScale", leftShoulder.localScale);
            leftArmTouchpoint.position = PlayerPrefX.GetVector3($"{FQN}_leftArmTouchPoint_position", leftArmTouchpoint.position);
            rightShoulder.localScale = PlayerPrefX.GetVector3($"{FQN}_rightShoulderScale", rightShoulder.localScale);
            rightArmTouchpoint.position = PlayerPrefX.GetVector3($"{FQN}_rightArmTouchPoint_position", rightArmTouchpoint.position);
        }
    }
}
