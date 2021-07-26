using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Training
{
    public class FitWorkspace : Singleton<FitWorkspace>
    {
        public enum State
        {
            START,
            LEFT,
            RIGHT,
            DONE
        }

        public Transform leftHand, leftHandObjective, rightHand, rightHandObjective, leftArm, rightArm;
        private StateMachine<State> stateMachine = new StateMachine<State>();

        public State currentState
        {
            get { return stateMachine.State; }
            set { stateMachine.State = value; }
        }

        void Start()
        {
            stateMachine.onEnter[State.START] = (state) =>
            {
                
            };
            stateMachine.onEnter[State.LEFT] = (state) =>
            {

            };
            stateMachine.onEnter[State.RIGHT] = (state) =>
            {
            };
            stateMachine.onEnter[State.DONE] = (state) =>
            {
            };
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.F))
            {
                //FitLeft();
                FitRight();
            }
        }

        private void Fit(Transform hand, Transform objective, Transform target)
        {
            float factor = objective.position.x / hand.position.x;
            Debug.Log($"Scaled {target} by {factor}");
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

        public void Next()
        {
            currentState = currentState.Next();
            Debug.Log($"Current {typeof(State).FullName} changed to {currentState}");
        }

        public void StartCalibration()
        {
            currentState = State.START;
        }

        public void StopCalibration()
        {
            currentState = State.DONE;
        }
    }
}
