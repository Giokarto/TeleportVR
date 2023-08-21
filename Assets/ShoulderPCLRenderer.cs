using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class ShoulderPCLRenderer : MonoBehaviour
{
    public ShoulderPCLreceiver subscriber1;

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
        // Get the point cloud positions and color from the ShoulderPCLreceiver
        Vector3[] positions1 = subscriber1.GetShouderPCL();
        Color color1 = subscriber1.GetShoulderPCLColor();
        Debug.Log("Color: " + color1);

        if (positions1 == null || positions1.Length == 0)
        {
            return;
        }

        int totalPoints = positions1.Length;
        Vector3[] combinedPositions = new Vector3[totalPoints];
        Color[] combinedColors = new Color[totalPoints];

        int[] indices = new int[totalPoints];
        for (int i = 0; i < totalPoints; i++)
        {
            indices[i] = i;
        }

        // Assign positions from the point cloud
        positions1.CopyTo(combinedPositions, 0);

        // Fill the combinedColors array with the single color value
        for (int i = 0; i < totalPoints; i++)
        {
            combinedColors[i] = color1;
        }

        mesh.Clear();
        mesh.vertices = combinedPositions;
        mesh.colors = combinedColors;
        mesh.SetIndices(indices, MeshTopology.Points, 0);
        //mesh.Optimize();
        meshFilter.mesh = mesh;
    }

    void Update()
    {
        // Set the _PointSize property of the material and update the mesh
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}
