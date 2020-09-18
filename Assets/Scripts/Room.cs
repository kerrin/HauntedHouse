using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class Room : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _windowPrefabs = null;
    [SerializeField]
    private RoomType _roomType;
    [SerializeField]
    // Every other room is displayed reversed, due to switchback on changing rooms
    private bool _reversed = false;
    [SerializeField]
    private float[] _windowChance = { 0.9f, 0.9f, 0.9f, 0.9f };
    private int _windowCount = 0;

    private Ground _ground;
    private Wall _wall;
    private Ceiling _ceiling;
    
    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Room");
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground in Room");
        _ceiling = GameObject.Find("Ceiling").GetComponent<Ceiling>();
        if (!_ceiling) Debug.LogError("No Ceiling in Room");
        _wall = GameObject.Find("Walls").GetComponent<Wall>();
        if (!_wall) Debug.LogError("No Wall in Room");
        _ground.AddGround();
        _wall.AddWalls();
        _ceiling.AddCeiling(); ;
        if (!CreateRoom())
        {
            Debug.LogError("Cannot initialise room");
            return;
        }

        PopulateRoom();
    }

    /*
     * Create a room and everything in it.
     */
    private bool CreateRoom()
    {
        AddDoors();
        FillOutsideChaparone();
        AddWindows();
        return true;
    }

    // Create the Doors to get in and out
    private void AddDoors()
    {
        foreach (bool isOutsideWall in _wall.GetIsOutsideWalls())
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
        LineRange groundRangeX = _ground.GetXRange();
        LineRange groundRangeZ = _ground.GetZRange();
        foreach (bool isOutsideWall in _wall.GetIsOutsideWalls())
        {
            if (isOutsideWall && Random.Range(0f, 1f) < _windowChance[wallIndex])
            {
                // Add Window(s)
                List<LineRange> freeWallSections = _wall.GetEmptySections(wallIndex);
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
                                windowAtZ = (groundRangeZ.min * _wall.GetWallShrink()) + 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            case 1:
                                windowAtX = (groundRangeX.max * _wall.GetWallShrink()) - 0f;
                                windowAtZ = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                usedWall = new LineRange(windowAtZ, windowAtZ + windowWidth);
                                Debug.Log("WindowZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtZ);
                                break;
                            case 2:
                                windowAtX = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                windowAtZ = (groundRangeZ.max * _wall.GetWallShrink()) - 0.05f;
                                usedWall = new LineRange(windowAtX, windowAtX + windowWidth);
                                Debug.Log("WindowX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtX);
                                break;
                            default:
                                windowAtX = (groundRangeX.min * _wall.GetWallShrink()) + 0f;
                                windowAtZ = Random.Range(freeWallSection.min, freeWallSection.max - windowWidth);
                                usedWall = new LineRange(windowAtZ, windowAtZ + windowWidth);
                                Debug.Log("WindowZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + windowAtZ);
                                break;
                        }
                        GameObject window = Instantiate<GameObject>(windowPrefab);
                        window.transform.Rotate(_wall.GetWallRotate(wallIndex) * Vector3.up);
                        window.transform.position = new Vector3(windowAtX, (_ceiling.GetHeight() / 3) * 2, windowAtZ);
                        Debug.Log("Window "+ wallIndex + "("+ windowAtX + ","+ windowAtZ + "): " + window.transform.position);
                        window.transform.parent = transform;
                        window.transform.name = "Window" + wallIndex + windowPrefab.name;
                        window.tag = "Decoration";
                        AddWindowLight(wallIndex, window);
                        _wall.AddWallUsage(wallIndex, usedWall);
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
            Vector3 lightPos = new Vector3(0, _ceiling.GetHeight() - 0.5f, 0);
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
}

public enum RoomType
{
    Foyer,
    Basement,
    Ground,
    Upper
}