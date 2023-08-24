using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StayInsideTheCircle : MonoBehaviour
{
    public Transform pointCloud; // Reference to the point cloud object
    public GameObject circleGameObject; // Reference to the circle's GameObject

    void Update()
    {
        // Get the center position of the circle
        Vector3 centerPosition = circleGameObject.transform.position;

        // Calculate the radius of the circle based on its scale (assuming it's uniformly scaled)
        float radius = circleGameObject.transform.localScale.x * 0.5f;

        // Calculate the direction from the center of the circle to the point cloud
        Vector3 directionFromCenter = new Vector3(
            pointCloud.position.x - centerPosition.x,
            0,
            pointCloud.position.z - centerPosition.z
        );

        // Calculate the distance of the point cloud from the center of the circle
        float distanceFromCenter = directionFromCenter.magnitude;

        // Check if the point cloud is outside the circle
        if (distanceFromCenter > radius)
        {
            // The point cloud is outside the circle, so move it back to the edge of the circle
            Vector3 positionOnCircleEdge = centerPosition + directionFromCenter.normalized * radius;
            pointCloud.position = new Vector3(positionOnCircleEdge.x, pointCloud.position.y, positionOnCircleEdge.z);
        }
    }
}
