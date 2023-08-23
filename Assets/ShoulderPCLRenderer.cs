using System.Linq;
using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class ShoulderPCLRenderer : MonoBehaviour
{
    public ShoulderPCLreceiver subscriber;

    Mesh mesh;
    MeshRenderer meshRenderer;
    MeshFilter meshFilter;
    [SerializeField]
    private Material meshMaterial;
    public float pointSize = 1f;

    private Vector3[] lastPositions;

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
        Vector3[] positions = subscriber.GetShouderPCL();

        if (positions == null || positions.Length == 0)
        {
            return;
        }

        Color color = subscriber.GetShoulderPCLColor();
        Color[] colors = new Color[positions.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            colors[i] = color;
        }

        mesh.Clear();
        mesh.vertices = positions;
        mesh.colors = colors;
        mesh.SetIndices(System.Linq.Enumerable.Range(0, positions.Length).ToArray(), MeshTopology.Points, 0);
        meshFilter.mesh = mesh;

        lastPositions = positions;
    }

    void Update()
    {
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}
