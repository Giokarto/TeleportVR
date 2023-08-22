using System.Linq;
using ServerConnection.RosTcpConnector;
using ServerConnection.RosTcpConnector.MANSURI;
using UnityEngine;

public class PointCloudRenderer : MonoBehaviour
{
    public PointCloudReceiver subscriber;

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
        Vector3[] positions = subscriber.GetPCL();

        if (positions == null || positions.Length == 0)
        {
            return;
        }

        // Check for changes before updating
        if (positions != lastPositions)
        {
            Color[] colors = subscriber.GetPCLColor();

            mesh.Clear();
            mesh.vertices = positions;
            mesh.colors = colors;
            mesh.SetIndices(System.Linq.Enumerable.Range(0, positions.Length).ToArray(), MeshTopology.Points, 0);
            meshFilter.mesh = mesh;

            lastPositions = positions;
        }
    }

    void Update()
    {
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}