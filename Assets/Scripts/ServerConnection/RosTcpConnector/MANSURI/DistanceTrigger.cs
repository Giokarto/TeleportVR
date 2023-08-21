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
        public float distanceThreshold = 220f; // Set threshold to 149
        public GameObject BackSphere;

        // The distance that will be set from the STREAMINROS class
        private float distance = 150f; // Initialize to a value greater than distanceThreshold

        [SerializeField] public string rosDistanceTopic = "/obstacle_distance";

        private Color originalColor; // To store the original color

        // Start is called before the first frame update
        void Start()
        {
            // Subscribe to the ROS topic
            ROSConnection.instance.Subscribe<Float64Msg>(rosDistanceTopic, HandleDistanceMessage);
            Debug.Log("Subscribed to distance topic: " + rosDistanceTopic);
            Debug.Log(rosDistanceTopic);

            // Store the original color
            RawImage rawImageComponent = BackSphere.GetComponent<RawImage>();
            if (rawImageComponent != null)
            {
                originalColor = rawImageComponent.material.GetColor("_ColorFrom");
            }
        }

        // Callback function to handle the received obstacle distance data
        private void HandleDistanceMessage(Float64Msg data)
        {
            // Update the distance value with the received data
            distance = (float)data.data;
            Debug.Log("Received data type: " + data.GetType().ToString());
            Debug.Log("Received distance from /obstacle_distance: " + distance);
        }
        
        void Update()
        {
            Debug.Log("Current distance value: " + distance);

            // If the distance is less than or equal to the threshold, show the canvas and change color to red
            if (distance <= distanceThreshold)
            {
                Debug.Log("Distance is less than threshold. Changing color to red.");

                canvas.SetActive(true);
                RawImage rawImageComponent = BackSphere.GetComponent<RawImage>();
                if (rawImageComponent != null)
                {
                    Material mat = rawImageComponent.material;
                    mat.SetColor("_ColorFrom", Color.red);
                    rawImageComponent.material = mat; // Re-assign the material back to the RawImage component
                }
            }
            // If the distance is greater than the threshold, hide the canvas and revert to original color
            else
            {
                Debug.Log("Reverting to original color.");

                canvas.SetActive(false);
                RawImage rawImageComponent = BackSphere.GetComponent<RawImage>();
                if (rawImageComponent != null)
                {
                    Material mat = rawImageComponent.material;
                    mat.SetColor("_ColorFrom", originalColor);
                    rawImageComponent.material = mat;
                }
            }
        }
    }
}
