using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Token : MonoBehaviour
{
    public int segments = 36;
    public float radius = 0.6f;
    public uint boardIndex;
    
    private float height = VisualValues.tokenHeight;

    private void Start()
    {
        GenerateCylinder();
    }

    public void setColor(Color color)
    {
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        renderer.material = new Material(renderer.material);
        renderer.material.color = color;
    }

    private void GenerateCylinder()
    {
        Mesh mesh = new Mesh();

        int vertexCount = (segments + 1) * 4 + 2;
        Vector3[] vertices = new Vector3[vertexCount];
        Vector3[] normals = new Vector3[vertexCount];
        int[] triangles = new int[segments * 12];

        float angleStep = Mathf.PI * 2f / (segments - 1);
        int vert = 0;
        int tri = 0;

        float halfHeight =  height / 2f;
        normals[vert] = Vector3.up;
        vertices[vert++] = new Vector3(0, halfHeight, 0);
        normals[vert] = Vector3.down;
        vertices[vert++] = new Vector3(0, -halfHeight, 0);

        // @Note Create circle vertices
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;

            vertices[vert] = new Vector3(x, halfHeight, z);
            normals[vert++] = Vector3.up;

            vertices[vert] = new Vector3(x, -halfHeight, z);
            normals[vert++] = Vector3.down;
        }

        // @Note Create side vertices
        for (int i = 0; i <= segments; i++)
        {
            float angle = i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float z = Mathf.Sin(angle) * radius;
            Vector3 normal = new Vector3(x, 0, z).normalized;

            vertices[vert] = new Vector3(x, halfHeight, z);
            normals[vert++] = normal;

            vertices[vert] = new Vector3(x, -halfHeight, z);
            normals[vert++] = normal;
        }

        // @Note Create top and bottom triangles
        for (int i = 0; i < segments; i++)
        {
            int topCenter = 0;
            int bottomCenter = 1;
            int topVertex = 2 + i * 2;
            int bottomVertex = 3 + i * 2;

            //@Note Top triangle
            triangles[tri++] = topCenter;
            triangles[tri++] = topVertex + 2;
            triangles[tri++] = topVertex;

            //@Note Bottom triangle
            triangles[tri++] = bottomCenter;
            triangles[tri++] = bottomVertex;
            triangles[tri++] = bottomVertex + 2;
        }

        //@Note Create side triangles
        int sideOffset = (segments + 1) * 2;
        for (int i = 0; i < segments; i++)
        {
            int current = sideOffset + i * 2;
            int next = current + 2;

            triangles[tri++] = current;
            triangles[tri++] = next;
            triangles[tri++] = current + 1;

            triangles[tri++] = current + 1;
            triangles[tri++] = next;
            triangles[tri++] = next + 1;
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.triangles = triangles;

        GetComponent<MeshFilter>().mesh = mesh;
        GetComponent<MeshCollider>().sharedMesh = mesh;
    }
}