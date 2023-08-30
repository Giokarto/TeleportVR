using UnityEngine;

public class StayInsideTheCircle : MonoBehaviour
{
    // public Transform pointCloud; // Reference to the point cloud object
    // public GameObject cubeGameObject; // Reference to the cube's GameObject
    //
    // // void Start()
    // // {
    // //     FilterPointCloud();
    // // }
    // //
    // // void FilterPointCloud()
    // // {
    // //     // Get the bounds of the cube
    // //     Bounds cubeBounds = cubeGameObject.GetComponent<Renderer>().bounds;
    // //
    // //     // Assuming pointCloud is a collection of Vector3 points
    // //     foreach (Vector3 point in pointCloud)
    // //     {
    // //         if (!cubeBounds.Contains(point))
    // //         {
    // //             // Point is outside the cube. Take necessary action.
    // //             // For example, if you want to remove the point:
    // //             // pointCloud.Remove(point);
    // //         }
    // //     }
    // // }
    //
    // public Material pointCloudMaterial;
    //
    // void LateUpdate()
    // {
    //     // Get the center position and half-extents of the cube
    //     Vector3 centerPosition = cubeGameObject.transform.position;
    //     Vector3 halfExtents = cubeGameObject.transform.localScale * 0.5f;
    //
    //     // If you need to pass the cube's center and half-extents to the shader, uncomment the following lines:
    //     pointCloudMaterial.SetVector("_CubeCenter", centerPosition);
    //     pointCloudMaterial.SetVector("_CubeHalfExtents", halfExtents);
    // }

}
