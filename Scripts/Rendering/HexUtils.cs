using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public static class HexUtils
{
    public static Vector3 CubeToWorld(HexCoordinate hex, float hexSize)
    {
        float worldX = hexSize * (Mathf.Sqrt(3f) * hex.q + Mathf.Sqrt(3f)/2f * hex.r);
        float worldZ = hexSize * (3f/2f * hex.r);
        return new Vector3(worldX, 0, worldZ); 
    }

    public static Vector3 CubeToWorld_FlatTop(HexCoordinate hex, float hexSize)
    {
        float x = hexSize * (3f / 2f * hex.q);
        float z = hexSize * (Mathf.Sqrt(3f) * (hex.r + hex.q / 2f));
        return new Vector3(x, 0f, z);
    }
}

