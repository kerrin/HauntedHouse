using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class Room : MonoBehaviour
{
    private float _adjustment = 0.6f; // Wall x and z are off by this when creating compared to free wall space list!
    [SerializeField]
    private Material[] _groundMaterials = null;
    [SerializeField]
    private Material[] _wallMaterials = null;
    [SerializeField]
    private GameObject[] _windowPrefabs = null;
    [SerializeField]
    private RoomType _roomType;
    [SerializeField]
    // Every other room is displayed reversed, due to switchback on changing rooms
    private bool _reversed = false;
    [SerializeField]
    // If there are parts of the floor missing, we render the floor differently
    private bool _missingFloor = false;
    [SerializeField]
    private bool[] _isOutsideWalls = { false, false, false, false };
    [SerializeField]
    private float[] _windowChance = { 0.9f, 0.9f, 0.9f, 0.9f };
    private int _windowCount = 0;
    [SerializeField]
    private float _chaperoneAngleOffset = 90f;
    private HmdQuad_t[] _chaperoneQuads = null;
    private PlaneRange _groundRange = null;
    List<Vector3> _chaparonePoints = new List<Vector3>();
    private float _ceilingHeight = 2.5f;
    private float _shrinkWall = 0.9f; // 90%
    private List<GameObject> _walls = new List<GameObject>();
    private List<LineRange>[] _wallEmpty = new List<LineRange>[4];
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

    /*
     * Convert the Head Mounted Device Quad to a Vector3
     */
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

    /*
     * Create a room and everything in it.
     */
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
            AddWalls();
            AddCeiling();
            AddDoors();
            FillOutsideChaparone();
            AddWindows();
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
    private void AddWalls()
    {
        GameObject[] walls = new GameObject[4];
        List<Vector3>[] wallPoints = new List<Vector3>[4];
        PlaneRange[] wallRanges = new PlaneRange[4];
        _wallEmpty[0] = new List<LineRange>();
        _wallEmpty[0].Add(new LineRange(_adjustment + _groundRange.minX * _shrinkWall, _adjustment + _groundRange.maxX * _shrinkWall));
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
        _wallEmpty[1] = new List<LineRange>();
        _wallEmpty[1].Add(new LineRange(_adjustment + _groundRange.minZ * _shrinkWall, _adjustment + _groundRange.maxZ * _shrinkWall));
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
        _wallEmpty[2] = new List<LineRange>();
        _wallEmpty[2].Add(new LineRange(_adjustment + _groundRange.minX * _shrinkWall, _adjustment + _groundRange.maxX * _shrinkWall));
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
        _wallEmpty[3] = new List<LineRange>();
        _wallEmpty[3].Add(new LineRange(_adjustment + _groundRange.minZ * _shrinkWall, _adjustment + _groundRange.maxZ * _shrinkWall));
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
            _walls.Add(wall);
        }
    }

    /*
     * Add a ceiling to the room
     */
    private void AddCeiling()
    {
        GameObject ceiling = new GameObject();
        ceiling.transform.parent = transform;
        int ceilingMaterialIndex = Random.Range(0, _groundMaterials.Length);
        MeshRenderer ceilingRendered = ceiling.AddComponent<MeshRenderer>();
        ceilingRendered.material = _groundMaterials[ceilingMaterialIndex];
        ceiling.transform.name = "Ceiling" + ceilingRendered.material.name;
        ceiling.tag = "Floor";
        MeshCollider ceilingCollider = ceiling.AddComponent<MeshCollider>();
        MeshFilter ceilingMeshFilter = ceiling.AddComponent<MeshFilter>();
        List<Vector3> ceilingPoints = new List<Vector3>();
        ceilingPoints.Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.minZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(_groundRange.minX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(_groundRange.maxX * _shrinkWall, _ceilingHeight, _groundRange.maxZ * _shrinkWall));
        PlaneRange ceilingRange = new PlaneRange(
                0, 1,
                0, 0,
                0, 1
            );
        ceilingMeshFilter.mesh = CreateMeshFromVectors(ceilingPoints, ceilingRange);

        ceilingCollider.sharedMesh = ceilingMeshFilter.mesh;
    }

    // Create the Doors to get in and out
    private void AddDoors()
    {
        foreach (bool isOutsideWall in _isOutsideWalls)
        {
            if (!isOutsideWall)
            {
                // Add a Door if we want
            }
        }
    }

    /*
     * Add windows to the outside walls
     */
    private void AddWindows()
    {
        int wallIndex = 0;
        int windowIndex = Random.Range(0, _windowPrefabs.Length);
        GameObject windowPrefab = _windowPrefabs[windowIndex];
        Bounds bounds = windowPrefab.GetComponent<MeshRenderer>().bounds;
        float windowWidth = bounds.max.x - bounds.min.x;
        Debug.Log("Window Size: " + windowWidth);
        foreach (bool isOutsideWall in _isOutsideWalls)
        {
            if (isOutsideWall && Random.Range(0f, 1f) < _windowChance[wallIndex])
            {
                // Add Window(s)
                List<LineRange> freeWallSections = _wallEmpty[wallIndex];
                foreach(LineRange freeWallSection in freeWallSections)
                {
                    if(freeWallSection.max - freeWallSection.min > windowWidth)
                    {
                        // Window fits
                        float windowAtX, windowAtZ;
                        LineRange usedWall;
                        switch (wallIndex)
                        {
                            case 0:
                                windowAtX = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                windowAtZ = (_groundRange.minZ * _shrinkWall) + 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            case 1:
                                windowAtX = (_groundRange.maxX * _shrinkWall) - 0f;
                                windowAtZ = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                usedWall = new LineRange(windowAtZ, windowAtZ + windowWidth);
                                Debug.Log("WindowZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtZ);
                                break;
                            case 2:
                                windowAtX = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                windowAtZ = (_groundRange.maxZ * _shrinkWall) - 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            default:
                                windowAtX = (_groundRange.minX * _shrinkWall) + 0f;
                                windowAtZ = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                usedWall = new LineRange(windowAtZ, windowAtZ + windowWidth);
                                Debug.Log("WindowZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtZ);
                                break;
                        }
                        GameObject window = Instantiate<GameObject>(windowPrefab);
                        window.transform.Rotate(GetWallRotate(wallIndex) * Vector3.up);
                        window.transform.position = new Vector3(windowAtX, (_ceilingHeight / 3) * 2, windowAtZ);
                        Debug.Log("Window "+ wallIndex + "("+ windowAtX + ","+ windowAtZ + "): " + window.transform.position);
                        window.transform.parent = transform;
                        window.transform.name = "Window" + wallIndex + windowPrefab.name;
                        window.tag = "Decoration";
                        AddWindowLight(wallIndex, window);
                        AddWallUsage(wallIndex, usedWall);
                        _windowCount++;
                        // Only one window per wall
                        break;
                    }
                }
            }
            wallIndex++;
        }
        
    }

    /*
     * Add a light to the window
     */
    private void AddWindowLight(int windowIndex, GameObject window)
    {
        GameObject lightSource = new GameObject();
        lightSource.transform.parent = window.transform;
        Vector3 lightPos = new Vector3(window.transform.position.x, window.transform.position.y - 0.5f, window.transform.position.z);
        Light light = lightSource.AddComponent<Light>();
        light.type = LightType.Point;
        Vector3 rotationVector = new Vector3(45f,0,0);
        switch(windowIndex)
        {
            case 0:
                lightPos.z += 0.2f;
                rotationVector.y = 0f;
                break;
            case 1:
                lightPos.x -= 0.2f;
                rotationVector.y = 270f;
                break;
            case 2:
                lightPos.z -= 0.2f;
                rotationVector.y = 180f;
                break;
            default:
                lightPos.x += 0.2f;
                rotationVector.y = 90f;
                break;
        }
        //light.transform.Rotate(rotationVector.x, rotationVector.y, rotationVector.z);
        lightSource.transform.position = lightPos;
        light.color = Color.cyan;
        light.range = 2f;
        light.intensity = 0.75f;
        light.shadows = LightShadows.Soft;
        light.name = "WindowLight";
    }

    /*
     * Create the Objects that fill in the space outside the chaparone, but inside the walls
     * This is to stop people exiting the chaparone area, but keep the room size as large as possible.
     */

    private void FillOutsideChaparone()
    {

    }

    /*
     * Add all the people, events, items, etc to a room
     */
    private void PopulateRoom()
    {
        AddInteractiveObjects();
        AddPeople();
        AddGhosts();
        AddItems();
        AddEvents();
        AddLights();
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

    /*
     * Add lights to the room.
     * If there are windows, we may choose not to turn the lights on
     */
    private void AddLights()
    {
        if(_windowCount == 0 || Random.Range(0f, _windowCount) < 1)
        {
            GameObject lightSource = new GameObject();
            lightSource.transform.parent = transform;
            Vector3 lightPos = new Vector3(0, _ceilingHeight - 0.5f, 0);
            Light light = lightSource.AddComponent<Light>();
            light.type = LightType.Point;
            
            lightSource.transform.position = lightPos;
            light.color = Color.yellow;
            light.range = 3f;
            light.intensity = 0.85f;
            light.shadows = LightShadows.Soft;
            light.name = "LightBulb";
        }
    }

    /*
     * Creates a Mesh plane from the points
     * The plane range allows the texture mapping to be aplied.
     */
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

    /*
     * Adjust the min and max values based on the new values
     */
    private void SetRanges(float x, ref float minX, ref float maxX, float z, ref float minZ, ref float maxZ)
    {
        if (minX > x) minX = x;
        if (maxX < x) maxX = x;
        if (minZ > z) minZ = z;
        if (maxZ < z) maxZ = z;
    }

    /*
     * What degree rotation to apply to the wall based on it's index
     */
    private float GetWallRotate(int wallIndex)
    {
        switch(wallIndex)
        {
            case 0: return 180f;
            case 1: return 270f;
            case 2: return 0f;
            case 3: return 90f;
            default: return 0f;
        }
    }

    /*
     * Block a section of the wall, so it doesn't have multiple objects in the same place
     */
    private void AddWallUsage(int wallIndex, LineRange usedWall)
    {
        int index = 0;
        while (index < _wallEmpty[wallIndex].Count) {
            LineRange freeWallSection = _wallEmpty[wallIndex][index];
            if ((freeWallSection.min < usedWall.min && freeWallSection.max > usedWall.min) ||
                (freeWallSection.min < usedWall.max && freeWallSection.max > usedWall.max))
            {
                LineRange newFreeSection = new LineRange(usedWall.max, freeWallSection.max);
                if (index + 1 == _wallEmpty[wallIndex].Count)
                {
                    // Was last item, so add at end of list
                    _wallEmpty[wallIndex].Add(newFreeSection);
                }
                else
                {
                    // In middle of list so insert in correct place
                    _wallEmpty[wallIndex].Insert(index + 1, newFreeSection);
                }
                
                newFreeSection = new LineRange(freeWallSection.min, usedWall.min);
                _wallEmpty[wallIndex][index] = newFreeSection;
                break;
            }
            index++;
        }
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
 * The Plane Range Details
 * The min and max values in all three dimensions
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

/*
 * The min and max values in a single dimension
 */
public class LineRange
{
    public float min;
    public float max;

    public LineRange(float inMin, float inMax)
    {
        this.min = inMin;
        this.max = inMax;
    }

    public float Range()
    {
        return max - min;
    }
    
    override public string ToString()
    {
        return "LineRange:" + min + " to " + max;
    }
}