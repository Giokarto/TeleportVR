using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using JointStateMsg=RosMessageTypes.Sensor.JointStateMsg;
using System.Collections.Generic;

public class JointPosePublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/operator/joint_target";
    public float publishMessageFrequency = 0.01f;

    public BioIK.BioIK BodyIK;
    public BioIK.BioIK HeadIK;

    private float timeElapsed;
    private JointStateMsg msg;
    List<double> positions, velocities;
    List<string> names;
    
    private bool motionEnabled, lastMenuBtn;

    // TODO move finger joint names definitions somewhere sane
    private List<string> fingerJointNames = new List<string>{ "thumb_", "index_", "middle_", "pinky_"};
    
    // Start is called before the first frame update
    void Start()
    {
        
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JointStateMsg>(topicName);

        msg = new JointStateMsg();
        msg.header = new RosMessageTypes.Std.HeaderMsg();

        positions = new List<double>();
        velocities = new List<double>();
        names = new List<string>();

        motionEnabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        bool btn;
        if (InputManager.Instance.ControllerLeft.TryGetFeatureValue(UnityEngine.XR.CommonUsages.menuButton, out btn) && btn && !lastMenuBtn)
        {
            motionEnabled = !motionEnabled;
        }
        lastMenuBtn = btn;

        // reset joints to 0 when the headset is not on
        //if (!InputManager.Instance.IsUserActive())
        //{
        //    ResetJoints();
        //    ros.Publish(topicName, GetLatestJointStates());
        //    motionEnabled = false;
        //}

        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            if (motionEnabled) ros.Publish(topicName, GetLatestJointStates());
            timeElapsed = 0;
        }
    }

    void ResetJoints()
    {
        //foreach (var segment in HeadIK.Segments)
        //{
        //    //HeadIK.ResetPosture(segment);
        //    if (segment.Joint != null)
        //    {
        //        segment.Joint.
        //    }
        //}
        foreach (var segment in BodyIK.Segments)
        {
            BodyIK.ResetPosture(segment);
        }
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
                if (segment.Joint.name == "head_axis2") continue;
                names.Add(segment.Joint.name);
                velocities.Add(0f);
                positions.Add((float)segment.Joint.X.CurrentValue * Mathf.Deg2Rad);
                
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

        // Distribure angle on elbow_*_axis0 to axis0 and axis1 equally
        // TODO sort our these magic numbers
        const int elbowRightAxis0 = 6;
        const int elbowLeftAxis0 = 14;
        positions[elbowRightAxis0 + 1] = -positions[elbowRightAxis0] / 2;
        positions[elbowRightAxis0] /= 2;
        positions[elbowLeftAxis0 + 1] = positions[elbowLeftAxis0] / 2;
        positions[elbowLeftAxis0] /= 2;

        // hand
        float left_open = 0, right_open = 0;
        if (InputManager.Instance.GetLeftController())
            InputManager.Instance.ControllerLeft
                .TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out left_open);

        if (InputManager.Instance.GetRightController())
            InputManager.Instance.ControllerRight
                .TryGetFeatureValue(UnityEngine.XR.CommonUsages.grip, out right_open);


        // 4 values for right and left
        for (int i = 0; i < 4; i++)
        {
            positions.Add(right_open);
            velocities.Add(0);
            names.Add(fingerJointNames[i] + "right");
        }
        for (int i = 0; i < 4; i++)
        {
            positions.Add(left_open);
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
