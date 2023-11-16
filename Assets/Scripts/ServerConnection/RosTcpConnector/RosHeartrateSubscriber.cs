using TMPro;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Sensor = RosMessageTypes.Std.Int32Msg;


public class RosHeartrateSubscriber : MonoBehaviour
{
    public GameObject RosHeartrateSubscribers;
    public string topic;
    private string unwantedString = "Int32Msg: " + "\ndata: ";
    private string messageData;
    public TextMeshProUGUI output;

    void Start()
    {
        Debug.Log(message:"Heartrate detection initialized");
        ROSConnection.GetOrCreateInstance().Subscribe<Sensor>("/roboy/pinky/sensing/vitals/heartrate", Rateupdate);
        
    }

    void Rateupdate(Sensor data)
    {
        //messageData = data.ToString().Replace(unwantedString, "Detected heartrate: \n");
        messageData = data.ToString().Replace(unwantedString, "");
        Debug.Log(message: "Detected heartrate: \n" + messageData); 
        output.text = " \n" + messageData;
    }
}