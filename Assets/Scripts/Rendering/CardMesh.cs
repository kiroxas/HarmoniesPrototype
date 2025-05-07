using UnityEngine;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CardMesh : MonoBehaviour
{
    public float width = 1f;
    public float height = 1.4f;
    public float cornerRadius = 0.1f;
    public int cornerSegments = 8; // how smooth the corners are
    public float spriteHeight = 0.5f;

    public Sprite cardSprite;  // The sprite to display on the card
    public Vector3 spriteOffset = new Vector3(0, 0, 0.1f);  // Offset for the sprite in world space

    private void Awake()
    {
       
    }

    public void createCard(CardData data)
    {
        GenerateMesh();
        DisplaySprite(data.icon);
    }

    private void GenerateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.name = "SolidRoundedCard";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        float hw = width / 2f;
        float hh = height / 2f;
        float cr = Mathf.Min(cornerRadius, hw, hh);

        List<Vector3> outline = new List<Vector3>();

        outline.Add(new Vector3(-hw + cr, hh, 0));
        outline.Add(new Vector3(hw - cr, hh, 0));
        AddCorner(outline, new Vector2(hw - cr, hh - cr), 0f);

        outline.Add(new Vector3(hw, hh - cr, 0));
        outline.Add(new Vector3(hw, -hh + cr, 0));
        AddCorner(outline, new Vector2(hw - cr, -hh + cr), 270f);

        outline.Add(new Vector3(hw - cr, -hh, 0));
        outline.Add(new Vector3(-hw + cr, -hh, 0));
        AddCorner(outline, new Vector2(-hw + cr, -hh + cr), 180f);

        outline.Add(new Vector3(-hw, -hh + cr, 0));
        outline.Add(new Vector3(-hw, hh - cr, 0));
        AddCorner(outline, new Vector2(-hw + cr, hh - cr), 90f);
        vertices.Add(Vector3.zero);
        uvs.Add(new Vector2(0.5f, 0.5f));

        foreach (var point in outline)
        {
            vertices.Add(point);
            uvs.Add(new Vector2(point.x / width + 0.5f, point.y / height + 0.5f));
        }

        int centerIndex = 0;

        for (int i = 1; i <= outline.Count; i++)
        {
            int current = i;
            int next = (i % outline.Count) + 1;

            triangles.Add(centerIndex);
            triangles.Add(current);
            triangles.Add(next);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }

    void AddCorner(List<Vector3> outline, Vector2 center, float startAngle)
    {
        float step = 90f / cornerSegments;
        for (int i = 1; i < cornerSegments; i++)
        {
            float angle = startAngle + i * step;
            float rad = Mathf.Deg2Rad * angle;
            outline.Add(new Vector3(center.x + Mathf.Cos(rad) * cornerRadius,
                                    center.y + Mathf.Sin(rad) * cornerRadius,
                                    0));
        }
    }

    void DisplaySprite(Sprite cardSprite)
    {
        if (cardSprite != null)
        {
            GameObject spriteObject = new GameObject("CardSprite");
            spriteObject.transform.SetParent(transform, false);

            SpriteRenderer spriteRenderer = spriteObject.AddComponent<SpriteRenderer>();
            spriteRenderer.sprite = cardSprite;
            spriteObject.transform.localPosition = spriteOffset;
            Sprite sprite = spriteRenderer.sprite;
            float spritePPU = spriteRenderer.sprite.pixelsPerUnit;

            float meshWidth = width;
            float meshHeight = height;

            float spriteWidth = sprite.rect.width / spritePPU;
            float spriteHeight = sprite.rect.height / spritePPU;
            
            float aspectRatio = spriteWidth / spriteHeight;

            float scaleX = meshWidth / spriteWidth;
            float scaleY = meshHeight / spriteHeight;

            if(scaleX > scaleY)
            {
                scaleX = scaleY * aspectRatio;
            }
            else
            {
                scaleY = scaleX / aspectRatio;
            }

            
            spriteObject.transform.localScale = new Vector3(scaleX, scaleY, spriteObject.transform.localScale.z);;
            spriteRenderer.sortingOrder = 1; 
        }
    }
}
