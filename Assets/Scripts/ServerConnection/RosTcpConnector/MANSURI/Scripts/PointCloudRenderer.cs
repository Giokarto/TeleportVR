using System.Linq;
using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{
    // Reference to the subscriber object that receives the point cloud data.
    public PointCloudReceiver subscriber;
    // Defines the radius for a mask circle. Range slider in Unity Inspector will allow adjusting between 0 and 300.
    [SerializeField][Range(0f, 300f)] private float maskCircleRadius;

    // Mesh components to represent the point cloud in the Unity scene.
    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    // Material to render the point cloud.
    [SerializeField]
    private Material meshMaterial;
    // Size of the points in the point cloud.
    public float pointSize = 1f;
    // Stores the last rendered point cloud positions to avoid unnecessary updates.
    private Vector3[] lastPositions;

    void Start()
    {
        // Setting up mesh renderer and filter components for the point cloud.
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        // Applying the specified material to the mesh renderer.
        meshRenderer.material = meshMaterial;
        // Initializing the mesh with a format to support larger index counts.
        mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };


    }

    void UpdateMesh()
    {

        // Fetching the latest point cloud positions from the subscriber.
        Vector3[] positions = subscriber.GetPCL();

        // If no positions are received or the array is empty, return without updating.
        if (positions == null || positions.Length == 0)
        {
            return;
        }

        // Check for changes before updating
        if (positions != lastPositions)
        {
            Color[] colors = subscriber.GetPCLColor();
            // Reset the mesh and apply new vertices, colors, and indices.
            mesh.Clear();
            mesh.vertices = positions;
            mesh.colors = colors;
            mesh.SetIndices(System.Linq.Enumerable.Range(0, positions.Length).ToArray(), MeshTopology.Points, 0);
            meshFilter.mesh = mesh;

            // Store the positions as the latest rendered points.
            lastPositions = positions;
        }
    }

    void Update()
    {
        //control the size of the points in the point cloud visualization. Then update the mesh to reflect on the changes.
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}