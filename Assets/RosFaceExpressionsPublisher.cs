using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using JointStateMsg = RosMessageTypes.Sensor.JointStateMsg;

public class RosFaceExpressionsPublisher : MonoBehaviour
{
    ROSConnection ros;
    public string topicName = "/operator/face/expressions";
    public float publishMessageFrequency = 0.1f;

    float timeElapsed;
    JointStateMsg msg;

    OVRFaceExpressions faceExpressions;
    List<float> faceExpressionsList;
    int length;

    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<JointStateMsg>(topicName);

        msg = new JointStateMsg();
        msg.header = new RosMessageTypes.Std.HeaderMsg();

        length = System.Enum.GetNames(typeof(OVRFaceExpressions.FaceExpression)).Length;
        Debug.Log($"length: {length}");
        faceExpressionsList = new List<float>(new float[length]);
        msg.name = new string[length];
        msg.position = new double[length];

        var fe = System.Enum.GetValues(typeof(OVRFaceExpressions.FaceExpression));
        Debug.Log($"values: {fe}");
        for (int i = 0; i < length; i++)
        {
            Debug.Log(fe.GetValue(i).ToString());
            msg.name[i] = fe.GetValue(i).ToString();
        }
        Debug.Log($"face names: {string.Join(", ", msg.name)}");
        //Debug.Log($"face names: {msg.name}");

        faceExpressions = GetComponent<OVRFaceExpressions>();

    }

    // Update is called once per frame
    void Update()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency) // && VRControllerInputSystem.IsUserActive())
        {
            ros.Publish(topicName, GetLatestFaceExpressions());
            timeElapsed = 0;
        }
    }

    JointStateMsg GetLatestFaceExpressions()
    {
        
        for (int i=0; i<length; i++)
        {
            float weight;
            faceExpressions.TryGetFaceExpressionWeight((OVRFaceExpressions.FaceExpression)i, out weight);
            msg.position[i] = weight;
        }
        Debug.Log($"face weights:  {string.Join(", ", msg.position)}");
        return msg;
    }
}
