using OperatorUserInterface;
using UnityEngine;

namespace RobodyControl
{
    public class BioIKLatencyManager : MonoBehaviour
    {
        [System.Serializable]
        public class VelAcc
        {
            public float velocity, acceleration;

            public VelAcc(float velocity, float acceleration)
            {
                this.velocity = velocity;
                this.acceleration = acceleration;
            }
        }

        public BioIK.BioIK arms, leftHand, rightHand;
        public VelAcc armControlLimits = new VelAcc(velocity: 10, acceleration: 20);
        public VelAcc handControlLimits = new VelAcc(velocity: 10, acceleration: 20);

        private VelAcc armDefault, handDefault;
        private Scene currentState;

        // Start is called before the first frame update
        void Start()
        {
            armDefault = new VelAcc(arms.MaximumVelocity, arms.MaximumAcceleration);
            handDefault = new VelAcc(Mathf.Max(leftHand.MaximumVelocity, rightHand.MaximumVelocity),
                Mathf.Max(leftHand.MaximumAcceleration, rightHand.MaximumAcceleration));
            currentState = Scene.MAIN;
        }

        // Update is called once per frame
        void Update()
        {
            if (currentState == SceneManager.Instance.currentScene)
            {
                return;
            }

            currentState = SceneManager.Instance.currentScene;
            switch (currentState)
            {
                case Scene.MAIN:
                    arms.MaximumVelocity = armControlLimits.velocity;
                    arms.MaximumAcceleration = armControlLimits.acceleration;
                    leftHand.MaximumVelocity = handControlLimits.velocity;
                    leftHand.MaximumAcceleration = handControlLimits.acceleration;
                    rightHand.MaximumVelocity = handControlLimits.velocity;
                    rightHand.MaximumAcceleration = handControlLimits.acceleration;
                    break;
                default:
                    arms.MaximumVelocity = armDefault.velocity;
                    arms.MaximumAcceleration = armDefault.acceleration;
                    leftHand.MaximumVelocity = handDefault.velocity;
                    leftHand.MaximumAcceleration = handDefault.acceleration;
                    rightHand.MaximumVelocity = handDefault.velocity;
                    rightHand.MaximumAcceleration = handDefault.acceleration;
                    break;
            }
        }
    }
}
