using System;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class ConnectorVisualization : MonoBehaviour
{
    [SerializeField]
    private int segments = 5;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Vector3 Origin => transform.position;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        baseColor = meshRenderer.material.color;
    }

    public void GenerateMesh (Vector3 startNodeWorldPos, Vector3 endNodeWorldPos) 
    {
        // set up some values
        Vector3 span = endNodeWorldPos - startNodeWorldPos;
        Vector3 direction = span.normalized;
        Vector3 rightOffset = Quaternion.FromToRotation(Vector3.forward, direction) * (Vector3.right * 0.1f);
        Vector3 leftOffset = Quaternion.FromToRotation(Vector3.forward, direction) * (Vector3.left * 0.1f);
        Vector3 start = startNodeWorldPos - Origin;
        
        // generate verts
        Vector3[] initialVertices = new Vector3[2 + segments * 2];
        for (int i = 0; i < segments + 1; i++)
        {
            int vert1 = i * 2;
            Vector3 pos1 = start + (span / segments * i) + rightOffset;
            initialVertices[vert1] = pos1;
            
            int vert2 = i * 2 + 1;
            Vector3 pos2 = start + (span /segments * i) + leftOffset;
            initialVertices[vert2] = pos2;
        }
        
        // generate normals
        Vector3[] initialNormals = new Vector3[initialVertices.Length];
        for (int i = 0; i < initialNormals.Length; i++)
        {
            initialNormals[i] = Vector3.up;
        }
        
        // generate triangles
        int[] initialTris = new int[segments * 6];
        for (int i = 0; i < segments; i++)
        {
            initialTris[i*6 + 0] = i*2 + 0;
            initialTris[i*6 + 1] = i*2 + 1;
            initialTris[i*6 + 2] = i*2 + 2;
            
            initialTris[i*6 + 3] = i*2 + 1;
            initialTris[i*6 + 4] = i*2 + 3;
            initialTris[i*6 + 5] = i*2 + 2;
        }
        
        // create mesh
        Mesh mesh = new()
        {
            name = "Procedural Mesh",
            vertices = initialVertices,
            normals = initialNormals,
            triangles = initialTris
        };
        
        meshFilter.mesh = mesh;
    }

    private Color baseColor;
    public Color HighlightColor;

    public void SetHighlight(HighlightState state)
    {
        switch (state)
        {
            case HighlightState.NotHighlighted:
                meshRenderer.material.color = baseColor;
                break;
            case HighlightState.LightlyHighlighted:
                meshRenderer.material.color = NegativeMultiplyBlend(baseColor, HighlightColor, 0.5f);
                break;
            case HighlightState.Highlighted:
                meshRenderer.material.color = HighlightColor;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    private Color NegativeMultiplyBlend(Color origColor, Color overlay, float percent = 1)
    {
        Color invertedA = new Color(1 - origColor.r, 1 - origColor.g, 1 - origColor.b);
        Color invertedB = new Color(1 - overlay.r, 1 - overlay.g, 1 - overlay.b);
        Color fullNegMult = new Color(1 - invertedA.r * invertedB.r,1 - invertedA.g * invertedB.g,1 - invertedA.b * invertedB.b);
        return Color.Lerp(origColor, fullNegMult, percent);
    }
}
