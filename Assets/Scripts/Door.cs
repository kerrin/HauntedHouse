using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _doorPrefabs = null;
    [SerializeField]
    private Vector3 _doorScale = new Vector3(.4f, .4f, .6f);
    // All the sub classes
    private Ground _ground;
    private Wall _wall;
    // Start is called before the first frame update
    void Start()
    {
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground in Room");
        _wall = GameObject.Find("Walls").GetComponent<Wall>();
        if (!_wall) Debug.LogError("No Wall in Room");
    }

    // Create the Doors to get in and out
    public void AddDoors()
    {
        int doorIndex = Random.Range(0, _doorPrefabs.Length);
        GameObject doorPrefab = _doorPrefabs[doorIndex];
        Bounds bounds = doorPrefab.GetComponent<MeshRenderer>().bounds;
        float doorWidth = (bounds.max.x - bounds.min.x) * _doorScale.x;
        float doorHeight = 1.05f; // (bounds.max.y - bounds.min.y)  *_doorScale.y;
        int wallIndex = 0;
        foreach (bool isOutsideWall in _wall.GetIsOutsideWalls())
        {
            if (!isOutsideWall)
            {
                // Add a Door if we want
                Debug.Log("Try Door on wall " + wallIndex);
                // Find location player can get to door handle on wall
                List<LineRange> accessibleWall = _wall.GetAccessibleSections(wallIndex);
                Debug.Log(accessibleWall.Count);
                // No need to check if the wall is free (it should be as we add doors first)
                // Find a section big enough for a door
                foreach (LineRange wallSection in accessibleWall)
                {
                    Debug.Log(wallSection);
                    if (wallSection.Range() > doorWidth)
                    {
                        Debug.Log("Door on wall " + wallIndex);
                        Vector3 doorAt = _wall.AddToWall(wallIndex, wallSection, doorWidth, doorHeight);

                        GameObject door = Instantiate<GameObject>(doorPrefab);
                        door.transform.Rotate(_wall.GetWallRotate(wallIndex) * Vector3.forward);
                        door.transform.position = doorAt;
                        door.transform.localScale = _doorScale;
                        Debug.Log("door " + wallIndex + "(" + doorAt.x + "," + doorAt.z + "): " + door.transform.position);
                        door.transform.parent = transform;
                        door.transform.name = "Door" + wallIndex + doorPrefab.name;
                        door.tag = "Door";
                    }
                }
            }
            wallIndex++;
        }
    }
}
