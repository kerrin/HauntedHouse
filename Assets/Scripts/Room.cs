﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valve.VR;

public class Room : MonoBehaviour
{
    
    [SerializeField]
    private RoomType _roomType;
    [SerializeField]
    // Every other room is displayed reversed, due to switchback on changing rooms
    private bool _reversed = false;
    [SerializeField]
    private float[] _outsideWallChance = { 0.2f, 0.5f, 0.5f, 0.9f };
    // All the sub classes
    private Ground _ground;
    private Wall _wall;
    private Ceiling _ceiling;
    private Window _window;
    private Door _door;

    // Start is called before the first frame update
    void Start()
    {
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground in Room");
        _ceiling = GameObject.Find("Ceiling").GetComponent<Ceiling>();
        if (!_ceiling) Debug.LogError("No Ceiling in Room");
        _wall = GameObject.Find("Walls").GetComponent<Wall>();
        if (!_wall) Debug.LogError("No Wall in Room");
        _window = GameObject.Find("Windows").GetComponent<Window>();
        if (!_window) Debug.LogError("No Window in Room");
        _door = GameObject.Find("Doors").GetComponent<Door>();
        if (!_door) Debug.LogError("No Door in Room");
        _ground.AddGround();
        for(int wallIndex = 0; wallIndex < _outsideWallChance.Length; wallIndex++) {
            if (Random.Range(0, 1f) < _outsideWallChance[wallIndex])
            {
                _wall.SetOutsideWall(wallIndex, true);
            } else
            {
                _wall.SetOutsideWall(wallIndex, false);
            }
        }
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
        // Doors most important, can't go outside chaparone area anyway
        _door.AddDoors();
        // Then the chaparone areas
        FillOutsideChaparone();
        // Then the windows
        _window.AddWindows();
        return true;
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
        if(_window.GetWindowCount() == 0 || Random.Range(0f, _window.GetWindowCount()) < 1)
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