using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class Room : MonoBehaviour
{
    [SerializeField]
    private Material[] _groundMaterials = null;
    [SerializeField]
    private Material[] _wallMaterials = null;
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
    private PlaneRange _groundRange = null;
    List<Vector3> _chaparonePoints = new List<Vector3>();
    private float _ceilingHeight = 2.5f;
    private float _shrinkWall = 0.9f; // 90%
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
            _groundRange = new PlaneRange(point.x, 0, point.z);
            _chaparonePoints.Add(point);
            for (int i = 1; i < _chaperoneQuads.Length; i++)
            {
                point = GetPoint(_chaperoneQuads[i]);
                _chaparonePoints.Add(point);
                SetRanges(point.x, ref _groundRange.minX, ref _groundRange.maxX, point.z, ref _groundRange.minZ, ref _groundRange.maxZ);
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
        GameObject ground = new GameObject();
        ground.transform.parent = transform;
        int groundMaterialIndex = Random.Range(0, _groundMaterials.Length);
        MeshRenderer groundRendered = ground.AddComponent<MeshRenderer>();
        groundRendered.material = _groundMaterials[groundMaterialIndex];
        ground.transform.name = "Ground" + groundRendered.material.name;
        ground.tag = "Floor";
        MeshCollider groundCollider = ground.AddComponent<MeshCollider>();
        MeshFilter groundMeshFilter = ground.AddComponent<MeshFilter>();
        PlaneRange range = new PlaneRange(0, 1f, 0, 0, 0, 1f);
        if (_missingFloor)
        {
            // TODO: Change to only render floor that exists
            groundMeshFilter.mesh = CreateMeshFromVectors(_chaparonePoints, range);
        }
        else {
            _chaparonePoints.Reverse();
            groundMeshFilter.mesh = CreateMeshFromVectors(_chaparonePoints, range);
            
        }
        groundCollider.sharedMesh = groundMeshFilter.mesh;
    }

    /*
     * Create the walls
     * Walls are slightly smaller than a bounding box of the chaparone area
     * We will in the area outside the chaparone area, but inside walls
     * with debris, furniture, ect
     */
    private void CreateWalls()
    {
        GameObject[] walls = new GameObject[4];
        List<Vector3>[] wallPoints = new List<Vector3>[4];
        PlaneRange[] wallRanges = new PlaneRange[4];
        wallPoints[0] = new List<Vector3>();
        wallPoints[0].Add(new Vector3(_groundRange.minX * _shrinkWall, 0, _groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(_groundRange.maxX * _shrinkWall, 0, _groundRange.minZ * _shrinkWall));
        wallRanges[0] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );
        wallPoints[1] = new List<Vector3>();
        wallPoints[1].Add(new Vector3(_groundRange.maxX * _shrinkWall, 0, _groundRange.minZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(_groundRange.maxX * _shrinkWall, 0, _groundRange.maxZ * _shrinkWall));
        wallRanges[1] = new PlaneRange(
                0, 0,
                0, 1,
                0, 1
            );
        wallPoints[2] = new List<Vector3>();
        wallPoints[2].Add(new Vector3(_groundRange.maxX * _shrinkWall, 0, _groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(_groundRange.minX * _shrinkWall, 0, _groundRange.maxZ * _shrinkWall));
        wallRanges[2] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );
        wallPoints[3] = new List<Vector3>();
        wallPoints[3].Add(new Vector3(_groundRange.minX * _shrinkWall, 0, _groundRange.maxZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(_groundRange.minX * _shrinkWall, 0, _groundRange.minZ * _shrinkWall));
        int wallMaterialIndex = Random.Range(0, _wallMaterials.Length);
        wallRanges[3] = new PlaneRange(
                0, 0,
                0, 1,
                0, 1
            );
        for (int i = 0; i < 4; i++)
        {
            GameObject wall = new GameObject();
            wall.transform.parent = transform;
            MeshRenderer wallRendered = wall.AddComponent<MeshRenderer>();
            wallRendered.material = _wallMaterials[wallMaterialIndex];
            wall.transform.name = "Wall" + i + wallRendered.material.name;
            wall.tag = "Wall";
            MeshCollider wallCollider = wall.AddComponent<MeshCollider>();
            MeshFilter wallMeshFilter = wall.AddComponent<MeshFilter>();
            wallMeshFilter.mesh = CreateMeshFromVectors(wallPoints[i], wallRanges[i]);
            wallCollider.sharedMesh = wallMeshFilter.mesh;
        }
    }

    // Create the Doors to get in and out
    private void CreateDoors()
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

    private Mesh CreateMeshFromVectors(List<Vector3> points, PlaneRange planeRange)
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
            if(planeRange.XRange() > 0) uvRange[index++] = (points[i].x - planeRange.minX) / planeRange.XRange();
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
public class PlaneRange
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;

    public PlaneRange(float x, float y, float z)
    {
        minX = x;
        maxX = x;
        minY = y;
        maxY = y;
        minZ = z;
        maxZ = z;
    }

    public PlaneRange(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        this.minX = xMin;
        this.maxX = xMax;
        this.minY = yMin;
        this.maxY = yMax;
        this.minZ = zMin;
        this.maxZ = zMax;
    }

    public float XRange()
    {
        return maxX - minX;
    }

    public float YRange()
    {
        return maxY - minY;
    }

    public float ZRange()
    {
        return maxZ - minZ;
    }
}