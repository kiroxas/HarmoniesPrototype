using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Face
{
    public List<Vector3> vertices { get; private set; }
    public List<int> triangles { get; private set; }
    public List<Vector2> uvs { get; private set; }

    public Face(List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
    {
        this.vertices = vertices;
        this.triangles = triangles;
        this.uvs = uvs;
    }
}

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class HexRenderer : MonoBehaviour
{
    private Mesh mesh;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    public HexCoordinate coordinate { get; private set; }

    public Material material;

    private List<Face> faces;

    public HexOptions options;

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        mesh = new Mesh();
        
        meshFilter.mesh = mesh;
        meshRenderer.material = material;

        drawMesh();
    }

    public void setCoordinate(HexCoordinate coord)
    {
        coordinate = coord;
    }

    private void OnEnable()
    {
        drawMesh();
    }

    public void OnValidate()
    {
        drawMesh();
    }

    public void drawMesh()
    {
        if(mesh)
        {
            drawFaces();
            combineFaces();

            MeshCollider collider = GetComponent<MeshCollider>();
            collider.sharedMesh = mesh;
        }
    }

    private Vector3 getPoint(float size, float height, int index)
    {
        float angleDeg = 60 * index;
        if(options.isFlatTopped == false)
        {
            angleDeg -= 30;
        }
        float angleRad = Mathf.PI / 180f * angleDeg;
        return new Vector3((size * Mathf.Cos(angleRad)), height, size * Mathf.Sin(angleRad));
    }

    private Face createFace(float innerRad, float outerRad, float heightA, float heightB, int point, bool reverse = false)
    {
        Vector3 pointA = getPoint(innerRad, heightB, point);
        Vector3 pointB = getPoint(innerRad, heightB, (point < 5) ? (point + 1) : 0);
        Vector3 pointC = getPoint(outerRad, heightA, (point < 5) ? (point + 1) : 0);
        Vector3 pointD = getPoint(outerRad, heightA, point);

        List<Vector3> vertices = new List<Vector3>(){ pointA, pointB, pointC, pointD};
        List<int> triangles = new List<int>() {0,1,2,2,3,0};
        List<Vector2> uvs = new List<Vector2>() {new Vector2(0,0), new Vector2(1,0), new Vector2(1,1), new Vector2(0,1)};
        if(reverse)
        {
            vertices.Reverse();
        }
        return new Face(vertices, triangles, uvs);
    }

    public void drawFaces()
    {
        faces = new List<Face>();

        float innerSize = options.innerSize;
        float outerSize = options.outerSize;
        float height = options.height;

        float halfHeight = height / 2.0f;
        // @Note top
        for(int point = 0; point < 6; point++)
        {
            faces.Add(createFace(innerSize, outerSize, halfHeight, halfHeight, point));
        }

        // @Note bottom
        for(int point = 0; point < 6; point++)
        {
            faces.Add(createFace(innerSize, outerSize, -halfHeight, -halfHeight, point, true));
        }

        // @Note inner
        for(int point = 0; point < 6; point++)
        {
            faces.Add(createFace(innerSize, innerSize, halfHeight, -halfHeight, point, true));
        }

        // @Note outer
        for(int point = 0; point < 6; point++)
        {
            faces.Add(createFace(outerSize, outerSize, halfHeight, -halfHeight, point, true));
        }
    }

    public void combineFaces()
    {
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>(); 
        List<Vector2> uvs = new List<Vector2>(); 

        for(int i = 0; i < faces.Count; ++i)
        {
            vertices.AddRange(faces[i].vertices);
            uvs.AddRange(faces[i].uvs);

            int offset = 4 * i;
            foreach(int triangle in faces[i].triangles)
            {
                tris.Add(triangle + offset);
            }
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
