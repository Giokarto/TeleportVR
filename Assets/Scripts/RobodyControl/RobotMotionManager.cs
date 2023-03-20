using System;
using System.Collections.Generic;
using ServerConnection;
using UnityEngine;

namespace RobodyControl
{
    /// <summary>
    /// Controls the joints in Roboy model:
    /// - Enables and disables movements (e.g. when Settings are open, the body shouldn't move).
    /// - Can save the current state and restore it later
    /// - Gathers all the joint states, to be sent as goal for the real Robody
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

        /// <summary>
        /// List of joint values (the last state of Roboy when leaving the real world).
        /// Only use in this class, the order of the values in this list might get confused otherwise.
        /// </summary>
        private List<float> savedJoints;
        /// <summary>
        /// Saves the current joint positions of the Roboy model (not the real Robody!) to be restored later.
        /// Used when transitioning from the real world back to the training scene.
        /// </summary>
        public void SaveJoints()
        {
            savedJoints = new List<float>();
            foreach (var body in bioIks)
            {
                foreach (var segment in body.Segments)
                {
                    if (segment.Joint != null)
                    {
                        savedJoints.Add((float)segment.Joint.X.CurrentValue);
                    }
                }
            }
        }

        private Dictionary<string, float> currentJointState = new Dictionary<string, float>();
        /// <summary>
        /// Returns the current state of all joints.
        /// To be used by the server class (<see cref="ServerBase"/>) to send the goal states to the server.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, float> GetAllCurrentJointStates()
        {
            currentJointState.Clear();
            foreach (var body in bioIks)
            {
                if (!body.gameObject.name.Contains("shadow"))
                {
                    foreach (var segment in body.Segments)
                    {
                        if (segment.Joint != null)
                        {
                            currentJointState.Add(segment.Joint.name, (float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                        }
                    }
                }
            }
            return currentJointState;
        }

        /// <summary>
        /// Resets all joints to the actual state of the real robot (or initial state). Used before switching to the real world.
        /// </summary>
        public void ResetRobody(bool useSavedJoints = true, bool instantaneous = true)
        {
            if (!useSavedJoints)
            {
                foreach (var body in bioIks)
                {
                    // switch to instantaneous movement type for BioIK so that the transition to joint targets 0 is immediate
                    if (instantaneous)
                    {
                        body.MotionType = BioIK.MotionType.Instantaneous;
                    }

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