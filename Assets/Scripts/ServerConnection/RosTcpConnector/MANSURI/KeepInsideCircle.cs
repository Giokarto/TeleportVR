using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepInsideCircle : MonoBehaviour
{
    public Transform pointCloud; // Reference to the point cloud object
    public float radius = 100.0f; // The radius of the circle

    void Update()
    {
        Vector3 centerPosition = transform.position; // The center of the circle

        Vector3 directionFromCenter = new Vector3(
            pointCloud.position.x - centerPosition.x,
            0,
            pointCloud.position.z - centerPosition.z
        );

        float distanceFromCenter = directionFromCenter.magnitude;

        if (distanceFromCenter > radius)
        {
            // The point cloud is outside the circle, so move it back to the edge of the circle
            Vector3 positionOnCircleEdge = centerPosition + directionFromCenter.normalized * radius;
            pointCloud.position = new Vector3(positionOnCircleEdge.x, pointCloud.position.y, positionOnCircleEdge.z);
        }
    }
}
