using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{
    public PointCloudReceiver subscriber1;
    public ShoulderPCLreceiver subscriber2;

    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    [SerializeField]
    private Material meshMaterial;
    public float pointSize = 1f;

    private Vector3[] lastPositions1;
    private Vector3[] lastPositions2;

    void Start()
    {
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
        Vector3[] positions1 = subscriber1.GetPCL();
        Vector3[] positions2 = subscriber2.GetShouderPCL();

        if (positions1 == null || positions1.Length == 0 || positions2 == null || positions2.Length == 0)
        {
            return;
        }

        // Check for changes before updating
        if (positions1 != lastPositions1 || positions2 != lastPositions2)
        {
            int totalPoints = positions1.Length + positions2.Length;
            Vector3[] combinedPositions = new Vector3[totalPoints];
            Color[] combinedColors = new Color[totalPoints];

            int[] indices = new int[totalPoints];
            for (int i = 0; i < totalPoints; i++)
            {
                indices[i] = i;
            }

            positions1.CopyTo(combinedPositions, 0);
            positions2.CopyTo(combinedPositions, positions1.Length);

            Color[] colors1 = subscriber1.GetPCLColor();
            Color color2 = subscriber2.GetShoulderPCLColor();
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

            lastPositions1 = positions1;
            lastPositions2 = positions2;
        }
    }

    void Update()
    {
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}

