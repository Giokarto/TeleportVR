using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class PointCloudRenderer: MonoBehaviour
{
  public PointCloudReceiver subscriber1;
    public ShoulderPCLreceiver subscriber2;

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
        // Get the point cloud positions and colors from the PointCloudReceiver and ShoulderPCLreceiver
        Vector3[] positions1 = subscriber1.GetPCL();
        Color[] colors1 = subscriber1.GetPCLColor();
        Vector3[] positions2 = subscriber2.GetShouderPCL();
        Color color2 = subscriber2.GetShoulderPCLColor();

        if (positions1 == null || positions1.Length == 0 || positions2 == null || positions2.Length == 0)
        {
            return;
        }

        int totalPoints = positions1.Length + positions2.Length;
        Vector3[] combinedPositions = new Vector3[totalPoints];
        Color[] combinedColors = new Color[totalPoints];

        int[] indices = new int[totalPoints];
        for (int i = 0; i < totalPoints; i++)
        {
            indices[i] = i;
        }

        // Combine positions and colors from both point clouds
        positions1.CopyTo(combinedPositions, 0);
        positions2.CopyTo(combinedPositions, positions1.Length);

        colors1.CopyTo(combinedColors, 0);
        for (int i = 0; i < positions2.Length; i++)
        {
            combinedColors[i + positions1.Length] = color2;
        }

        mesh.Clear();
        mesh.vertices = combinedPositions;
        mesh.colors = combinedColors;
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