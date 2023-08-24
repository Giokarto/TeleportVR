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
        public GameObject BackSphere;
        public float distanceThreshold = 220f;
        //Initialize to a value greater than distanceThreshold
        private float distance = 280f;
        // ROS topic from which the distance Data will be subscribed.
        [SerializeField] public string rosDistanceTopic = "/obstacle_distance";

        private Color originalColor;

        void Start()
        {
            // Subscribe to the ROS topic
            ROSConnection.instance.Subscribe<Float64Msg>(rosDistanceTopic, HandleDistanceMessage);
            // Debug.Log("Subscribed to distance topic: " + rosDistanceTopic);
            // Debug.Log(rosDistanceTopic);

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
            // Debug.Log("Received data type: " + data.GetType().ToString());
            // Debug.Log("Received distance from /obstacle_distance: " + distance);
        }

        void Update()
        {
            //Debug.Log("Current distance value: " + distance);

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
                // Debug.Log("Reverting to original color.");

                canvas.SetActive(false);
                RawImage rawImageComponent = BackSphere.GetComponent<RawImage>();
                if (rawImageComponent != null)
                {
                    Material mat = rawImageComponent.material;

                    // Convert hex color to Unity's Color format
                    Color originalColor = new Color(0x5C / 255f, 0x66 / 255f, 0x6E / 255f, 1f);

                    mat.SetColor("_ColorFrom", originalColor);
                    rawImageComponent.material = mat;
                }
            }

        }
    }
}
