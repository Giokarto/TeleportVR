using TMPro;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using Sensor = RosMessageTypes.Std.Int32Msg;


public class RosRespiratoryrateSubscriber : MonoBehaviour
{
    public GameObject RosRespiratoryrateSubscribers;
    public string topic;
    private string unwantedString = "Int32Msg: " + "\ndata: ";
    private string messageData;
    public TextMeshProUGUI output;

    void Start()
    {
        Debug.Log(message:"Respiratoryrate detection initialized");
        ROSConnection.GetOrCreateInstance().Subscribe<Sensor>("/roboy/pinky/sensing/vitals/respiratoryrate", Rateupdate);
    }

    void Rateupdate(Sensor data)
    {
        //messageData = data.ToString().Replace(unwantedString, "Detected respiratoryrate: \n");
        messageData = data.ToString().Replace(unwantedString, "");                    //check Int32 Script (remove output)
        Debug.Log(message: "Detected respiratoryrate: \n" + messageData);                           //Logger output in the terminal
        output.text = " \n" + messageData;                                                                  //Text output for VR
        
    }
}