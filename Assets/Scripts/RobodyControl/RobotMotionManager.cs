using System;
using ServerConnection;
using UnityEngine;

namespace RobodyControl
{
    /// <summary>
    /// Enables and disables moving the robot body.
    /// </summary>
    public class RobotMotionManager : Singleton<RobotMotionManager>
    {

        private BioIK.BioIK[] bioIks;

        [SerializeField] private ServerData serverConnection;

        private void Start()
        {
            bioIks = FindObjectsOfType<BioIK.BioIK>();
            serverConnection = ServerData.Instance;
        }

        /// <summary>
        /// Immediately resets head orientation.
        /// To be used in <see cref="StartCalibrator"/> to speed up the calibration with rotation.
        /// </summary>
        public void ResetHead()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Enables / Disables motion of the Robody.
        /// </summary>
        public void EnableMotion(bool enable)
        {
            foreach (var body in bioIks)
            {
                foreach (var segment in body.Segments)
                {
                    segment.enabled = enable;
                    Debug.Log($"enable {enable} segment {segment}");
                }

                body.enabled = enable;
            }
        }

        /// <summary>
        /// Resets all joints to the actual state of the real robot (or initial state). Used before switching to the real world.
        /// </summary>
        public void ResetRobody(bool jointsFromServer = false)
        {
            if (!jointsFromServer)
            {
                foreach (var body in bioIks)
                {
                    // switch to instantaneous movement type for BioIK so that the transition to joint targets 0 is immediate 
                    body.MotionType = BioIK.MotionType.Instantaneous;

                    foreach (var segment in body.Segments)
                    {
                        body.ResetPosture(segment);
                    }
                }
            }
            else
            {
                // go to the latest joint targets before leaving HUD
                var jointValues = serverConnection.GetLatestJointValues();
                if (jointValues.Count > 0)
                {
                    int i = 0;
                    foreach (var body in bioIks)
                    {
                        if (body.name.Contains("shadow"))
                        {
                            continue;
                        }

                        foreach (var segment in body.Segments)
                        {
                            if (segment.Joint != null)
                            {
                                //Debug.Log($"{body.name}: {segment.Joint.name} {i}");
                                segment.Joint.X.SetTargetValue(jointValues[i]);

                                i++;
                            }
                        }
                    }
                }
            }
        }
    }
}