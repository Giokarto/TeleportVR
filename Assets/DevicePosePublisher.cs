using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using PoseMsg = RosMessageTypes.Geometry.PoseMsg;
using Vector3Msg = RosMessageTypes.Geometry.Vector3Msg;

public class DevicePosePublisher : MonoBehaviour
{

    ROSConnection ros;
    public string topicRoot = "/operator/device/";
    string topicName;
    private List<string> features = new List<string> { "pose", "velocity", "acceleration" };
    private List<string> deviceNames = new List<string> { "controller/left/", "controller/right/", "headset/" };
    public float publishMessageFrequency = 0.01f;
    private Dictionary<string, UnityEngine.XR.InputDevice> deviceMap = new Dictionary<string, UnityEngine.XR.InputDevice>();

    private float timeElapsed;
    private PoseMsg poseMsg;
    private Vector3Msg velMsg;
    private Vector3Msg accMsg;

    // Start is called before the first frame update
    void Start()
    {
        //while (InputManager.Instance.controllerLeft.Count == 0)
        //{
        //    Debug.Log("waiting for the input manager");
        //}

        try
        {
            deviceMap = new Dictionary<string, UnityEngine.XR.InputDevice>()
            {
                { "controller/left/", InputManager.Instance.controllerLeft[0]},
                { "controller/right/", InputManager.Instance.controllerRight[0]},
                { "headset/", InputManager.Instance.headset[0]}
            };
        } 
        catch
        {

        }

        //Debug.Log(InputManager.Instance.controllerLeft.Count);
        //Debug.Log(InputManager.Instance.controllerRight[0]);
        //Debug.Log(InputManager.Instance.headset[0]);

        ros = ROSConnection.GetOrCreateInstance();

        foreach(var device in deviceNames)
        {
            
            topicName = topicRoot + device ;
            ros.RegisterPublisher<PoseMsg>(topicName + "pose");
            ros.RegisterPublisher<Vector3Msg>(topicName + "velocity");
            ros.RegisterPublisher<Vector3Msg>(topicName + "acceleration");

        }
        
        poseMsg = new PoseMsg();
        velMsg = new Vector3Msg();
        accMsg = new Vector3Msg();

    }

    // Update is called once per frame
    void Update()
    {

        deviceMap = new Dictionary<string, UnityEngine.XR.InputDevice>()
        {
            { "controller/left/", InputManager.Instance.controllerLeft[0]},
            { "controller/right/", InputManager.Instance.controllerRight[0]},
            { "headset/", InputManager.Instance.headset[0]}
        };
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            foreach (var device in deviceMap.Keys)
            {
                topicName = topicRoot + device;
                deviceMap.TryGetValue(device, out var inputDevice);
                ros.Publish(topicName + "pose", GetLatestDevicePose(inputDevice));
                ros.Publish(topicName + "velocity", GetLatestDeviceVelocity(inputDevice));
                ros.Publish(topicName + "acceleration", GetLatestDeviceAcceleration(inputDevice));
            }
               
            timeElapsed = 0;
        }
    }

    PoseMsg GetLatestDevicePose(UnityEngine.XR.InputDevice device)
    {
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.devicePosition, out Vector3 devicePosition);
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceRotation, out Quaternion deviceRotation);
        var rosPosition = RosUtils.Vector2Ros(devicePosition);
        var rosOrientation = RosUtils.Quaternion2Ros(deviceRotation);

        //Debug.Log($"ros position: {rosPosition}");
        //Debug.Log($"ros orn: {rosOrientation}");
       
        
        poseMsg.position = new RosMessageTypes.Geometry.PointMsg(rosPosition.x, rosPosition.y, rosPosition.z);
        //poseMsg.position.x = rosPosition.x;
        //poseMsg.position.y = rosPosition.y;
        //poseMsg.position.z = rosPosition.z;

        poseMsg.orientation = new RosMessageTypes.Geometry.QuaternionMsg(rosOrientation.x, rosOrientation.y, rosOrientation.z, rosOrientation.w);
        //poseMsg.orientation.x = rosOrientation.x;
        //poseMsg.orientation.y = rosOrientation.y;
        //poseMsg.orientation.z = rosOrientation.z;
        //poseMsg.orientation.w = rosOrientation.w;

        return poseMsg;

    }

    Vector3Msg GetLatestDeviceVelocity(UnityEngine.XR.InputDevice device)
    {
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceVelocity, out Vector3 deviceVelocity);
        var rosVelocity = RosUtils.Vector2Ros(deviceVelocity);

        velMsg.x = rosVelocity.x;
        velMsg.y = rosVelocity.y;
        velMsg.z = rosVelocity.z;

        return velMsg;
    }

    Vector3Msg GetLatestDeviceAcceleration(UnityEngine.XR.InputDevice device)
    {
        device.TryGetFeatureValue(UnityEngine.XR.CommonUsages.deviceAcceleration, out Vector3 deviceAcceleration);
        var rosAccelearation = RosUtils.Vector2Ros(deviceAcceleration);

        accMsg.x = rosAccelearation.x;
        accMsg.y = rosAccelearation.y;
        accMsg.z = rosAccelearation.z;

        return accMsg;
    }


}
