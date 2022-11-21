using BioIK;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Widgets;

namespace InputDevices
{
    public class EnableControlManager : Singleton<EnableControlManager>
    {
        public BioSegment left_hand, right_hand;
        public BioIK.BioIK left_fingers, right_fingers;

        public BioIKGroup leftBioIKGroup, rightBioIKGroup;
        private UnityEngine.XR.InputDevice leftController, rightController;
        private bool leftControllerFound = false, rightControllerFound = false;


        public struct BioIKGroup
        {
            public BioSegment hand_segment;
            public BioIK.BioIK hand_body;
            private bool enabled;

            public BioIKGroup(BioSegment _segment, BioIK.BioIK _body)
            {
                hand_segment = _segment;
                hand_body = _body;
                enabled = false;
            }

            public void WeakSetEnabled(bool _enabled)
            {
                if (!enabled)
                {
                    return;
                }

                SetEnabled(_enabled);
            }

            public void SetEnabled(bool _enabled)
            {
                enabled = _enabled;
                //if (enabled)
                //{
                //    controller.SendHapticImpulse(0, 0.005f);
                //}
                //else
                //{
                //    controller.StopHaptics();
                //}
                foreach (var objective in hand_segment.Objectives)
                {
                    objective.enabled = enabled;
                }
#if SENSEGLOVE
            foreach (var segment in hand_body.Segments)
            {
                // toggle joints, not objective on hands as they are driven by custom scripts, not BioIK
                if (segment.Joint != null)
                {
                    segment.Joint.enabled = enabled;
                }
            }
#endif
            }

            public bool IsEnabled()
            {
                return enabled;
            }

            public void UpdateFingers(double value)
            {
                foreach (var segment in hand_body.Segments)
                {

                    if (segment.Joint != null)
                    {
                        //if (segment.Joint.name.Contains("TH") && !segment.Joint.name.Contains("J1"))
                        //{

                        //}
                        if ((segment.Joint.name.Contains("TH") &&
                             !segment.Joint.name.Contains("J5")) || // && !segment.Joint.name.Contains("J2")) ||
                            (segment.Joint.name.Contains("LF") &&
                             !segment.Joint.name.Contains("J5")) || // && !segment.Joint.name.Contains("J4")) ||
                            (!segment.Joint.name.Contains("J4")))
                        {
                            var range = segment.Joint.X.UpperLimit - segment.Joint.X.LowerLimit;

                            segment.Joint.X.SetTargetValue(value * range - segment.Joint.X.LowerLimit);
                        }

                    }
                }
            }
        }

        void FindControllers()
        {
            if (ControllerInputManager.Instance.GetLeftController())
            {
                leftController = ControllerInputManager.Instance.controllerLeft[0];
                leftControllerFound = true;
            }

            if (ControllerInputManager.Instance.GetRightController())
            {
                rightController = ControllerInputManager.Instance.controllerRight[0];
                rightControllerFound = true;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            leftBioIKGroup = new BioIKGroup(left_hand, left_fingers);
            rightBioIKGroup = new BioIKGroup(right_hand, right_fingers);
            FindControllers();
        }


        // Update is called once per frame
        void Update()
        {

            if (leftController == null || rightController == null || !leftController.isValid ||
                !rightController.isValid)
            {
                FindControllers();
            }

            if (leftControllerFound)
            {
                ReadControllers(leftBioIKGroup, leftController, true);
            }

            //else
            //{
            //    leftBioIKGroup.SetEnabled(false);
            //}
            if (rightControllerFound)
            {
                ReadControllers(rightBioIKGroup, rightController, false);
            }
            //else
            //{
            //    rightBioIKGroup.SetEnabled(false);
            //}
        }

        void ReadControllers(BioIKGroup group, UnityEngine.XR.InputDevice controller, bool isLeft)
        {
            if (controller == null)
            {
                return;
            }

            var _enabled = 0.0f;
            var trigger = false;
            if (controller.isValid)
            {
                controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out _enabled);
                controller.TryGetFeatureValue(UnityEngine.XR.CommonUsages.gripButton, out trigger);
            }

            int i = isLeft ? 0 : 1;
            // Show that the arm is active in the state manager
            //WidgetInteraction.SetBodyPartActive(53 - i, _enabled > 0.9f);

            // Show that the fingers are active in the state manager
            //WidgetInteraction.SetBodyPartActive(55 - i, trigger);

#if SENSEGLOVE
        group.WeakSetEnabled(true);
#else
            group.SetEnabled(_enabled > 0.9f);
            group.UpdateFingers(System.Convert.ToDouble(trigger));
#endif
        }
    }
}