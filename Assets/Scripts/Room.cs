using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class Room : MonoBehaviour
{
    private float _adjustment = 0.6f; // Wall x and z are off by this when creating compared to free wall space list!
    [SerializeField]
    private Material[] _ceilingMaterials = null;
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
    private bool[] _isOutsideWalls = { false, false, false, false };
    [SerializeField]
    private float[] _windowChance = { 0.9f, 0.9f, 0.9f, 0.9f };
    private int _windowCount = 0;
    private Floor _floor;
    private float _ceilingHeight = 2.5f;
    private float _shrinkWall = 0.9f; // 90%
    private List<GameObject> _walls = new List<GameObject>();
    private List<LineRange>[] _wallEmpty = new List<LineRange>[4];
    // Start is called before the first frame update
    void Start()
    {
        _floor = GetComponent<Floor>();
        if (!CreateRoom())
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
     * Create a room and everything in it.
     */
    private bool CreateRoom()
    {
        AddWalls();
        AddCeiling();
        AddDoors();
        FillOutsideChaparone();
        AddWindows();
        return true;
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
        Chaparone chaparone = _floor.GetChaparone();
        PlaneRange groundRange = chaparone.GetGroundRange();
        _wallEmpty[0] = new List<LineRange>();
        _wallEmpty[0].Add(new LineRange(_adjustment + groundRange.minX * _shrinkWall, _adjustment + groundRange.maxX * _shrinkWall));
        wallPoints[0] = new List<Vector3>();
        wallPoints[0].Add(new Vector3(groundRange.minX * _shrinkWall, 0, groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRange.maxX * _shrinkWall, 0, groundRange.minZ * _shrinkWall));
        wallRanges[0] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );
        _wallEmpty[1] = new List<LineRange>();
        _wallEmpty[1].Add(new LineRange(_adjustment + groundRange.minZ * _shrinkWall, _adjustment + groundRange.maxZ * _shrinkWall));
        wallPoints[1] = new List<Vector3>();
        wallPoints[1].Add(new Vector3(groundRange.maxX * _shrinkWall, 0, groundRange.minZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRange.maxX * _shrinkWall, 0, groundRange.maxZ * _shrinkWall));
        wallRanges[1] = new PlaneRange(
                0, 0,
                0, 1,
                0, 1
            );
        _wallEmpty[2] = new List<LineRange>();
        _wallEmpty[2].Add(new LineRange(_adjustment + groundRange.minX * _shrinkWall, _adjustment + groundRange.maxX * _shrinkWall));
        wallPoints[2] = new List<Vector3>();
        wallPoints[2].Add(new Vector3(groundRange.maxX * _shrinkWall, 0, groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRange.minX * _shrinkWall, 0, groundRange.maxZ * _shrinkWall));
        wallRanges[2] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );
        _wallEmpty[3] = new List<LineRange>();
        _wallEmpty[3].Add(new LineRange(_adjustment + groundRange.minZ * _shrinkWall, _adjustment + groundRange.maxZ * _shrinkWall));
        wallPoints[3] = new List<Vector3>();
        wallPoints[3].Add(new Vector3(groundRange.minX * _shrinkWall, 0, groundRange.maxZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRange.minX * _shrinkWall, 0, groundRange.minZ * _shrinkWall));
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
            wallMeshFilter.mesh = MeshTools.CreateMeshFromVectors(wallPoints[i], wallRanges[i]);
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
        int ceilingMaterialIndex = Random.Range(0, _ceilingMaterials.Length);
        MeshRenderer ceilingRendered = ceiling.AddComponent<MeshRenderer>();
        ceilingRendered.material = _ceilingMaterials[ceilingMaterialIndex];
        ceiling.transform.name = "Ceiling" + ceilingRendered.material.name;
        ceiling.tag = "Floor";
        MeshCollider ceilingCollider = ceiling.AddComponent<MeshCollider>();
        MeshFilter ceilingMeshFilter = ceiling.AddComponent<MeshFilter>();
        List<Vector3> ceilingPoints = new List<Vector3>();
        Chaparone chaparone = _floor.GetChaparone();
        PlaneRange groundRange = chaparone.GetGroundRange();
        ceilingPoints.Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.minZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(groundRange.minX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        ceilingPoints.Add(new Vector3(groundRange.maxX * _shrinkWall, _ceilingHeight, groundRange.maxZ * _shrinkWall));
        PlaneRange ceilingRange = new PlaneRange(
                0, 1,
                0, 0,
                0, 1
            );
        ceilingMeshFilter.mesh = MeshTools.CreateMeshFromVectors(ceilingPoints, ceilingRange);

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
        Chaparone chaparone = _floor.GetChaparone();
        PlaneRange groundRange = chaparone.GetGroundRange();
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
                                windowAtZ = (groundRange.minZ * _shrinkWall) + 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            case 1:
                                windowAtX = (groundRange.maxX * _shrinkWall) - 0f;
                                windowAtZ = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                usedWall = new LineRange(windowAtZ, windowAtZ + windowWidth);
                                Debug.Log("WindowZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtZ);
                                break;
                            case 2:
                                windowAtX = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                windowAtZ = (groundRange.maxZ * _shrinkWall) - 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            default:
                                windowAtX = (groundRange.minX * _shrinkWall) + 0f;
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