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
    private RoomType _roomType;
    [SerializeField]
    // Every other room is displayed reversed, due to switchback on changing rooms
    private bool _reversed = false;
    [SerializeField]
    // If there are parts of the floor missing, we render the floor differently
    private bool _missingFloor = false;
    [SerializeField]
    private float _chaperoneAngleOffset = 90f;
    private HmdQuad_t[] _chaperoneQuads = null;
    private RoomRange _roomRange = null;
    List<Vector3> _chaparonePoints = new List<Vector3>();
    // Start is called before the first frame update
    void Start()
    {
        if(!CreateRoom())
        {
            Debug.LogError("Cannot initialise room");
            return;
        }
        PopulateRoom();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private Vector3 GetPoint(HmdQuad_t quad)
    {
        Vector3 temp = transform.localScale;
        temp.x = quad.vCorners0.v2;
        temp.y = quad.vCorners0.v1;
        temp.z = quad.vCorners0.v0;
        Quaternion q = Quaternion.AngleAxis(_chaperoneAngleOffset, new Vector3(0, 1, 0));

        temp = q * temp;

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
        
        
        if (_chaperoneQuads.Length > 0)
        {
            Vector3 point = GetPoint(_chaperoneQuads[0]);
            _roomRange = new RoomRange(point.x, point.z);
            _chaparonePoints.Add(point);
            for (int i = 1; i < _chaperoneQuads.Length; i++)
            {
                point = GetPoint(_chaperoneQuads[i]);
                _chaparonePoints.Add(point);
                SetRanges(point.x, ref _roomRange.minX, ref _roomRange.maxX, point.z, ref _roomRange.minZ, ref _roomRange.maxZ);
            }

            PopulateGround();
            CreateWalls();
            FillOutsideChaparone();
        }
        else
        {
            Debug.LogError("No Bounding points");
            return false;
        }
        return success;
    }

    // Create the ground
    private void PopulateGround()
    {
        MeshFilter groundMeshFilter = _ground.AddComponent<MeshFilter>();
        MeshCollider collider = _ground.GetComponent<MeshCollider>();
        if (_missingFloor)
        {
            // TODO: Change to only render floor that exists
            groundMeshFilter.mesh = CreateMeshFromVectors(_chaparonePoints, _roomRange);
        }
        else {
            groundMeshFilter.mesh = CreateMeshFromVectors(_chaparonePoints, _roomRange);
            
        }
        collider.sharedMesh = groundMeshFilter.mesh;
    }

    /*
     * Create the walls
     * Walls are slightly smaller than a bounding box of the chaparone area
     * We will in the area outside the chaparone area, but inside walls
     * with debris, furniture, ect
     */
    private void CreateWalls()
    {

    }

    // Create the Objects that fill in the space outside the chaparone
    private void FillOutsideChaparone()
    {

    }

    private void PopulateRoom()
    {
        AddInteractiveObjects();
        AddPeople();
        AddGhosts();
        AddItems();
        AddEvents();
    }

    // Add the interactive objects
    private void AddInteractiveObjects()
    {

    }

    // Add the People
    private void AddPeople()
    {

    }

    // Add the Ghosts
    private void AddGhosts()
    {

    }

    // Add the Items
    private void AddItems()
    {

    }

    // Add the Events
    private void AddEvents()
    {

    }

    private Mesh CreateMeshFromVectors(List<Vector3> points, RoomRange roomRange)
    {
        List<int> tris = new List<int>(); // Every 3 ints represents a triangle in the ploygon
        List<Vector2> uvs = new List<Vector2>(); // Vertex position in 0-1 UV space (The material)
        
        int half = points.Count / 2;
        float realHalf = points.Count / 2f;
        
        for (int i = 1; i < half; i++) {
            tris.Add(i);
            tris.Add(i + 1);
            tris.Add(points.Count - i);

            tris.Add(i);
            tris.Add(points.Count - i);
            int value = i==1?0:(points.Count - i) + 1;
            tris.Add(value);
           
        }
        if (Math.Floor(realHalf) != Math.Ceiling(realHalf))
        {
            Debug.Log("Odd number of points " + points.Count + "(" + realHalf + ")");
            tris.Add(half);
            tris.Add(half + 1);
            tris.Add(half - 1);
        }
        for (int i = 0; i < points.Count; i++)
        {
            float xUv = (points[i].x - roomRange.minX) / roomRange.XRange();
            float zUv = (points[i].z - roomRange.minX) / roomRange.ZRange();
            uvs.Add(new Vector2(xUv, zUv));
        }

        Mesh mesh = new Mesh();
        mesh.vertices = points.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        return mesh;
    }

    private void SetRanges(float x, ref float minX, ref float maxX, float z, ref float minZ, ref float maxZ)
    {
        if (minX > x) minX = x;
        if (maxX < x) maxX = x;
        if (minZ > z) minZ = z;
        if (maxZ < z) maxZ = z;
    }
}

public enum RoomType
{
    Foyer,
    Basement,
    Ground,
    Upper
}

/*
 * The Room Range Details
 */
public class RoomRange
{
    public float minX;
    public float maxX;
    public float minZ;
    public float maxZ;

    public RoomRange(float x, float z)
    {
        minX = x;
        maxX = x;
        minZ = z;
        maxZ = z;
    }

    public float XRange()
    {
        return maxX - minX;
    }

    public float ZRange()
    {
        return maxZ - minZ;
    }
}