using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;
using UnityEngine.XR;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Room : MonoBehaviour
{
    [SerializeField]
    private GameObject _ground = null;
    [SerializeField]
    private Material _material = null;
    [SerializeField]
    private float _chaperoneAngleOffset = 90f;
    private HmdQuad_t[] _chaperoneQuads = null;
    //private GameObject _floor = null;
    //private LineRenderer _lineRenderer;
    // Start is called before the first frame update
    void Start()
    {
        Color color = Color.red;
        /*
        _lineRenderer = new LineRenderer();
        _lineRenderer = transform.gameObject.AddComponent<LineRenderer>();
        _lineRenderer.startColor = color;
        _lineRenderer.endColor = color;
        _lineRenderer.startWidth = 0.2f;
        _lineRenderer.endWidth = 0.2f;
        _lineRenderer.material = _material;
        */
        CreateRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 GetPoint(int index, HmdQuad_t quad)
    {
        Vector3 temp = transform.localScale;
        temp.x = quad.vCorners0.v2;
        temp.y = quad.vCorners0.v1;
        temp.z = quad.vCorners0.v0;
        Quaternion q = Quaternion.AngleAxis(_chaperoneAngleOffset, new Vector3(0, 1, 0));

        temp = q * temp;
        //_lineRenderer.SetPosition(index, temp);

        return temp;
    }

    private bool CreateRoom()
    {
        CVRChaperoneSetup chaperone = OpenVR.ChaperoneSetup;
        bool success = (chaperone != null) && chaperone.GetLiveCollisionBoundsInfo(out _chaperoneQuads);
        if (!success)
        {
            Debug.LogError("Failed to get Calibrated Chaperone bounds!  Make sure you have tracking first, and that your space is calibrated.");
            return false;
        }
        List<Vector3> points = new List<Vector3>();
        //_lineRenderer.positionCount = _chaperoneQuads.Length + 1;
        for (int i = 0; i < _chaperoneQuads.Length; i++)
        {
            Vector3 point = GetPoint(i, _chaperoneQuads[i]);
            points.Add(point);
        }
        if (_chaperoneQuads.Length > 0)
        {
            Vector3 point = GetPoint(_chaperoneQuads.Length, _chaperoneQuads[0]);
            MeshFilter groundMeshFilter = _ground.AddComponent<MeshFilter>();
            groundMeshFilter.mesh = CreateMeshFromVectors(points);
            MeshCollider collider = _ground.GetComponent<MeshCollider>();
            collider.sharedMesh = groundMeshFilter.mesh;
        }
        else
        {
            Debug.LogError("No Bounding points");
        }
        return success;
    }

    private Mesh CreateMeshFromVectors(List<Vector3> points)
    {
        List<int> tris = new List<int>(); // Every 3 ints represents a triangle in the ploygon
        List<Vector2> uvs = new List<Vector2>(); // Vertex position in 0-1 UV space (The material)
        
        int half = points.Count / 2;
        float realHalf = points.Count / 2f;
        float minX = points[0].x;
        float maxX = points[0].x;
        float minZ = points[0].z;
        float maxZ = points[0].z;
        for (int i = 1; i < half; i++) {
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(points.Count - i);

            tris.Add(i);
            tris.Add(points.Count - i);
            int value = i==1?0:(points.Count - i) + 1;
            tris.Add(value);
            if (minX > points[i].x) minX = points[i].x;
            if (maxX < points[i].x) maxX = points[i].x;
            if (minX > points[points.Count - i].x) minX = points[points.Count - i].x;
            if (maxX < points[points.Count - i].x) maxX = points[points.Count - i].x;
            if (minZ > points[i].z) minZ = points[i].z;
            if (maxZ < points[i].z) maxZ = points[i].z;
            if (minZ > points[points.Count - i].z) minZ = points[points.Count - i].z;
            if (maxZ < points[points.Count - i].z) maxZ = points[points.Count - i].z;
        }
        if (Math.Floor(realHalf) != Math.Ceiling(realHalf))
        {
            Debug.Log("Odd number of points " + points.Count + "(" + realHalf + ")");
            tris.Add(half);
            tris.Add(half + 1);
            tris.Add(half - 1);
            if (minX > points[half].x) minX = points[half].x;
            if (maxX < points[half].x) maxX = points[half].x;
            if (minZ > points[half].z) minZ = points[half].z;
            if (maxZ < points[half].z) maxZ = points[half].z;
        }
        float xRange = maxX - minX;
        float zRange = maxZ - minZ;
        for (int i = 0; i < points.Count; i++)
        {
            float xUv = (points[i].x - minX) / xRange;
            float zUv = (points[i].z - minX) / zRange;
            uvs.Add(new Vector2(xUv, zUv));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }
}
