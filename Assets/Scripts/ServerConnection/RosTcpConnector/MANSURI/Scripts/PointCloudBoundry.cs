// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class PointCloudBoundry : MonoBehaviour
// {
//     public GameObject circleGameObject; // Reference to the circle/sphere GameObject
//     public PointCloudRenderer pointCloudRenderer; // Reference to the PointCloudRenderer script
//
//     private void Start()
//     {
//         FilterPointCloud();
//     }
//
//     private void FilterPointCloud()
//     {
//         if (pointCloudRenderer == null)
//         {
//             Debug.LogError("PointCloudRenderer reference is missing!");
//             return;
//         }
//
//         if (pointCloudRenderer.subscriber == null)
//         {
//             Debug.LogError("Subscriber in PointCloudRenderer is not assigned!");
//             return;
//         }
//
//         Vector3[] positions = pointCloudRenderer.subscriber.GetPCL();
//         if (positions == null)
//         {
//             Debug.LogError("Received null positions from PointCloudRenderer!");
//             return;
//         }
//
//         // Get the center and radius of the circle/sphere
//         Vector3 circleCenter = circleGameObject.transform.position;
//         float circleRadius = circleGameObject.transform.localScale.x / 2; // Assuming uniform scaling
//
//         // Fetch the current point cloud positions and colors
//         Color[] colors = pointCloudRenderer.subscriber.GetPCLColor();
//
//         for (int i = 0; i < positions.Length; i++)
//         {
//             float distance = Vector3.Distance(positions[i], circleCenter);
//             if (distance > circleRadius)
//             {
//                 // Set the color of the point outside the boundary to transparent
//                 colors[i] = new Color(0, 0, 0, 0);
//             }
//         }
//
//         // Update the colors in the PointCloudRenderer script
//         pointCloudRenderer.subscriber.SetPCLColor(colors); // Assuming you have a method to set colors in PointCloudReceiver
//     }
// }
