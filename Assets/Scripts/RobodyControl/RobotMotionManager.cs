using System;
using System.Collections.Generic;
using UnityEngine;

namespace RobodyControl
{
    /// <summary>
    /// Enables and disables moving the robot body.
    /// </summary>
    public class RobotMotionManager : Singleton<RobotMotionManager>
    {

        private BioIK.BioIK[] bioIks;

        private void Start()
        {
            bioIks = FindObjectsOfType<BioIK.BioIK>();
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
                body.enabled = enable;
            }
        }

        private List<float> savedJoints;
        /// <summary>
        /// Saves the current joint positions of the Roboy model (not the real Robody!) to be restored later.
        /// Used when transitioning from the real world back to the training scene.
        /// </summary>
        public void SaveJoints()
        {
            savedJoints = GetLatestJointState();
        }

        /// <summary>
        /// Returns a list of current joint values of the Roboy model.
        /// <see cref="ServerConnection.ServerData.GetLatestJointValues"/> returns the values of the real Robody.
        /// </summary>
        /// <returns></returns>
        public List<float> GetLatestJointState()
        {
            List<float> joints = new List<float>();
            foreach (var body in bioIks)
            {
                foreach (var segment in body.Segments)
                {
                    if (segment.Joint != null)
                    {
                        joints.Add((float)segment.Joint.X.CurrentValue);
                    }
                }
            }

            return joints;
        }

        /// <summary>
        /// Resets all joints to the actual state of the real robot (or initial state). Used before switching to the real world.
        /// </summary>
        public void ResetRobody(bool useSavedJoints = true)
        {
            if (!useSavedJoints)
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
                if (savedJoints.Count > 0)
                {
                    int i = 0;
                    foreach (var body in bioIks)
                    {
                        foreach (var segment in body.Segments)
                        {
                            if (segment.Joint != null)
                            {
                                segment.Joint.X.SetTargetValue(savedJoints[i]);

                                i++;
                            }
                        }
                    }
                }
            }
        }
    }
}