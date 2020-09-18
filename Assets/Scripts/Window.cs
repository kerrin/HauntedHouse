using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Window : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _windowPrefabs = null;
    [SerializeField]
    private float[] _windowChance = { 0.9f, 0.9f, 0.9f, 0.9f };
    private int _windowCount = 0;

    // All the sub classes
    private Ground _ground;
    private Ceiling _ceiling;
    private Wall _wall;
    // Start is called before the first frame update
    void Start()
    {
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground in Room");
        _ceiling = GameObject.Find("Ceiling").GetComponent<Ceiling>();
        if (!_ceiling) Debug.LogError("No Ceiling in Room");
        _wall = GameObject.Find("Walls").GetComponent<Wall>();
        if (!_wall) Debug.LogError("No Wall in Room");
    }

    public int GetWindowCount()
    {
        return _windowCount;
    }

    /*
     * Add windows to the outside walls
     */
    public void AddWindows()
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
                foreach (LineRange freeWallSection in freeWallSections)
                {
                    if (freeWallSection.max - freeWallSection.min > windowWidth)
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
                        Debug.Log("Window " + wallIndex + "(" + windowAtX + "," + windowAtZ + "): " + window.transform.position);
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
        Vector3 rotationVector = new Vector3(45f, 0, 0);
        switch (windowIndex)
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
}
