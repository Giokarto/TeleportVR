using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseStampedMsg = RosMessageTypes.Geometry.PoseStampedMsg;
//using Widgets;
using TMPro;

public class RosEyePosePublisher : MonoBehaviour
{

    ROSConnection ros;
    public string topicName = "/roboy/pinky/eyes";
    public float publishMessageFrequency = 0.1f;

    public GameObject LeftEye;

    private float timeElapsed;
    private PoseStampedMsg msg;

    private float prevX = 0;
    private float prevY = 0;
    private float targetX = 0;
    private float targetY = 0;

    //private ToastrWidget widget;
    public TextMeshProUGUI text;


    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<PoseStampedMsg>(topicName);

        msg = new PoseStampedMsg();
        msg.header = new RosMessageTypes.Std.HeaderMsg();
        msg.header.frame_id = "eye_left";

        //widget = WidgetFactory.Instance.CreateToastrWidget("", 35, "");
    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency) // && VRControllerInputSystem.IsUserActive())
        {
            var eyeMsg = GetLatestEyePose();
            ros.Publish(topicName, eyeMsg);
            eyeMsg.header.frame_id = "eye_right";
            ros.Publish(topicName, eyeMsg);
            timeElapsed = 0;
        }


    }

    PoseStampedMsg GetLatestEyePose()
    {
        targetX = LeftEye.transform.rotation.eulerAngles.y;
        targetX = (targetX > 90) ? targetX - 360 : targetX;
        targetX *= 2;
        msg.pose.position.x = targetX - prevX;

        targetY = LeftEye.transform.rotation.eulerAngles.x;
        targetY = (targetY > 90) ? targetY - 360 : targetY;
        msg.pose.position.y = targetY - prevY;

        prevX = targetX;
        prevY = targetY;

        //widget.SetMessage($"{targetX} \n {targetY}");
        //text.text = $"x: {targetX} \n y:{targetY}";
        Debug.Log($"x: {targetX} \n y:{targetY}");

        return msg;
    }
}
