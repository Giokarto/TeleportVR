using ServerConnection.RosTcpConnector;
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
        meshRenderer.material.SetFloat("_PointSize", pointSize);
        UpdateMesh();
    }
}