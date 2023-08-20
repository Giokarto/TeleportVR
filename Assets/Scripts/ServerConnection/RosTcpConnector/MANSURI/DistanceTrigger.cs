using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

namespace ServerConnection.RosTcpConnector.MANSURI
{
    public class DistanceTrigger : MonoBehaviour
    {
        public GameObject canvas;
        public float distanceThreshold = 0.10f; // 40 cm converted to meters
        public GameObject BackSphere;

        // The distance that will be set from the STREAMINROS class
        private float distance;

        [SerializeField] public string rosDistanceTopic = "/obstacle_distance";

        // Start is called before the first frame update
        void Start()
        {
            // Subscribe to the ROS topic
            ROSConnection.instance.Subscribe<Float64Msg>(rosDistanceTopic, HandleDistanceMessage);
            Debug.Log("Subscribed to distance topic: " + rosDistanceTopic);
        }

        // Callback function to handle the received obstacle distance data
        private void HandleDistanceMessage(Float64Msg data)
        {
            // Update the distance value with the received data
            distance = (float)data.data;
        }
        
        void Update()
        {
            // If the distance is greater than the threshold, show the canvas
            if (distance > distanceThreshold)
            {
                canvas.SetActive(true);
                Image imageComponent = BackSphere.GetComponent<Image>();
                if (imageComponent != null)
                {
                    Material mat = imageComponent.material;
                    mat.SetColor("_ColorFrom", Color.red);
                    imageComponent.material = mat; // Re-assign the material back to the Image component
                }
            }
            // If the distance is less than the threshold, hide the canvas
            else
            {
                canvas.SetActive(false);
            }
        }
    }
}