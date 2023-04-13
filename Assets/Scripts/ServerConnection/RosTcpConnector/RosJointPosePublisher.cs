using System;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using JointStateMsg=RosMessageTypes.Sensor.JointStateMsg;
using System.Collections.Generic;
using InputDevices.VRControllers;
using RobodyControl;
using UnityEngine.XR.Interaction.Toolkit.Transformers;

namespace ServerConnection.RosTcpConnector
{
    public class RosJointPosePublisher : MonoBehaviour
    {
        ROSConnection ros;
        public string topicName = "/operator/joint_target";
        public float publishMessageFrequency = 0.01f;

        private BioIK.BioIK BodyIK;
        private BioIK.BioIK HeadIK;

        private float timeElapsed;
        private JointStateMsg msg;
        List<double> positions, velocities;
        List<string> names;

        // TODO move finger joint names definitions somewhere sane
        private List<string> fingerJointNames = new List<string> { "thumb_", "index_", "middle_", "pinky_" };

        // Start is called before the first frame update
        void Start()
        {
            BodyIK = ServerBase.Instance.BodyIK;
            HeadIK = ServerBase.Instance.HeadIK;

            ros = ROSConnection.GetOrCreateInstance();
            ros.RegisterPublisher<JointStateMsg>(topicName);

            msg = new JointStateMsg();
            msg.header = new RosMessageTypes.Std.HeaderMsg();

            positions = new List<double>();
            velocities = new List<double>();
            names = new List<string>();
        }

        /// <summary>
        /// Called if script active - set in <see cref="ServerBase.SetMotorOn"/>
        /// </summary>
        void Update()
        {
            // reset joints to 0 when the headset is not on
            // if (!VRControllerInputSystem.IsUserActive())
            // {
            //     Debug.Log("user inactive, resetting joints to 0");
            //     RobotMotionManager.Instance.ResetRobody(useSavedJoints: false, instantaneous: false);
            //     ros.Publish(topicName, GetLatestJointStates());
            // }

            timeElapsed += Time.deltaTime;

            if (timeElapsed > publishMessageFrequency)
            {
                ros.Publish(topicName, GetLatestJointStates());
                timeElapsed = 0;
            }
        }

        private float leftGrip, rightGrip;
        public void SaveGripState(float left, float right)
        {
            leftGrip = left;
            rightGrip = right;
        }

        private void OnEnable()
        {
            // Change grip through the input system, don't send it directly from reading the controllers.
            // This way changing the grip state can be paused, e.g. when a menu is open.
            VRControllerInputSystem.OnGripChange += SaveGripState;
        }

        private void OnDisable()
        {
            VRControllerInputSystem.OnGripChange -= SaveGripState;
        }

        JointStateMsg GetLatestJointStates()
        {
            positions = new List<double>();
            velocities = new List<double>();
            names = new List<string>();

            // head joints
            foreach (var segment in HeadIK.Segments)
            {
                if (segment.Joint != null)
                {
                    // TODO investigate axis2
                    //if (segment.Joint.name == "head_axis2") continue;
                    names.Add(segment.Joint.name);
                    velocities.Add(0f);
                    positions.Add((float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                    //Debug.Log($"publishing head {segment.Joint.name} with value {(float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad}");
                }
            }

            // torso joints
            foreach (var segment in BodyIK.Segments)
            {
                //Debug.Log(segment.name);
                if (segment.Joint != null)
                {
                    //Debug.Log($"{motorAngles.Count - 1}: {segment.gameObject.name} {segment.Joint.X.CurrentValue}");
                    positions.Add((float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                    velocities.Add(0);
                    names.Add(segment.Joint.name);
                    //latestJointValues.Add((float)segment.Joint.X.CurrentValue);
                }
            }

            // Distribute angle on elbow_*_axis0 to axis0 and axis1 equally
            // TODO sort our these magic numbers
            const int elbowRightAxis0 = 6;
            const int elbowLeftAxis0 = 14;
            positions[elbowRightAxis0 + 1] = -positions[elbowRightAxis0] / 2;
            positions[elbowRightAxis0] /= 2;
            positions[elbowLeftAxis0 + 1] = positions[elbowLeftAxis0] / 2;
            positions[elbowLeftAxis0] /= 2;

            // hand
            // 4 values for right and left
            for (int i = 0; i < 4; i++)
            {
                positions.Add(rightGrip);
                velocities.Add(0);
                names.Add(fingerJointNames[i] + "right");
            }

            for (int i = 0; i < 4; i++)
            {
                positions.Add(leftGrip);
                velocities.Add(0);
                names.Add(fingerJointNames[i] + "left");
            }

            //// wheelchair - moved to a separate script
            //positions.Add(0f);
            //velocities.Add(InputManager.Instance.GetControllerJoystick(true).y); //linear velocity
            //names.Add("wheelchair_linear");
            //positions.Add(0f);
            //velocities.Add(-InputManager.Instance.GetControllerJoystick(false).x); // angular velocity
            //names.Add("wheelchair_angular");

            //TODO fill header

            msg.position = positions.ToArray();
            msg.velocity = velocities.ToArray();
            msg.effort = new double[msg.position.Length];
            msg.name = names.ToArray();

            return msg;

        }
    }
}
