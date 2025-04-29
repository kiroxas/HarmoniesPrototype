using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "HexOption", menuName = "ScriptableObjects/HexOption")]
public class HexOptions : ScriptableObject
{
    public float outerSize = 1.0f;
    public float innerSize = 0.0f;
    public float height = 0.0f;
    public bool isFlatTopped;
    public float margin = 0.05f;
};