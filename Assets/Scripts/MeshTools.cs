using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTools : MonoBehaviour
{
    /*
     * Creates a Mesh plane from the points
     * The plane range allows the texture mapping to be aplied.
     */
    public static Mesh CreateMeshFromVectors(List<Vector3> points, PlaneRange planeRange)
    {
        List<int> tris = new List<int>(); // Every 3 ints represents a triangle in the ploygon
        List<Vector2> uvs = new List<Vector2>(); // Vertex position in 0-1 UV space (The material)

        int half = points.Count / 2;
        float realHalf = points.Count / 2f;

        for (int i = 1; i < half; i++)
        {
            tris.Add(i);
            tris.Add(points.Count - i);
            tris.Add(i + 1);

            tris.Add(i);
            int value = i == 1 ? 0 : (points.Count - i) + 1;
            tris.Add(value);
            tris.Add(points.Count - i);

        }
        if (Mathf.Floor(realHalf) != Mathf.Ceil(realHalf))
        {
            Debug.Log("Odd number of points " + points.Count + "(" + realHalf + ")");
            tris.Add(half);
            tris.Add(half + 1);
            tris.Add(half - 1);
        }
        for (int i = 0; i < points.Count; i++)
        {
            float[] uvRange = new float[2];
            int index = 0;
            if (planeRange.XRange() > 0) uvRange[index++] = (points[i].x - planeRange.minX) / planeRange.XRange();
            if (index < 1 || (planeRange.ZRange() > 0)) uvRange[index++] = (points[i].z - planeRange.minX) / planeRange.ZRange();
            if (index < 2) uvRange[index++] = (points[i].y - planeRange.minY) / planeRange.YRange();
            uvs.Add(new Vector2(uvRange[0], uvRange[1]));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}