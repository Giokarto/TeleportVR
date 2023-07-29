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
        public float distanceThreshold = 40;

        // The distance that will be set from the STREAMINROS class
        public float distance;

        public void SetDistance(float distance)
        {
            this.distance = distance;
        }

        void Update()
        {
            // If the distance is greater than the threshold, show the canvas
            if (distance > distanceThreshold)
            {
                canvas.SetActive(true);
            }
            // If the distance is less than the threshold, hide the canvas
            else if (distance < distanceThreshold)
            {
                canvas.SetActive(false);
            }
        }
    }
}
