using ServerConnection.RosTcpConnector;
using UnityEngine;

public class PointCloudRenderer: MonoBehaviour
{
    public PointCloudReceiver subscriber;

    // Point cloud mesh
    Mesh mesh;
    // MeshRenderer for the point cloud
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    [SerializeField]
    private Material meshMaterial;
    public float pointSize = 1f;

    void Start()
    {
        // Add MeshRenderer and MeshFilter components to the game object
        // and set the material of the MeshRenderer
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer.material = meshMaterial;

        mesh = new Mesh
        {
            indexFormat = UnityEngine.Rendering.IndexFormat.UInt32
        };
    }

    void UpdateMesh()
    {
        // Get the point cloud positions and colors from the PointCloudReceiver
        Vector3[] positions = subscriber.GetPCL();
        Color[] colors = subscriber.GetPCLColor();

        if (positions == null || positions.Length == 0)
        {
            return;
        }

        int[] indices = new int[positions.Length];

        for (int i = 0; i < positions.Length; i++)
        {
            indices[i] = i;
        }

        mesh.Clear();
        mesh.vertices = positions;
        mesh.colors = colors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);

        meshFilter.mesh = mesh;
    }

    void Update()
    {
        // Set the _PointSize property of the material and update the mesh
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}