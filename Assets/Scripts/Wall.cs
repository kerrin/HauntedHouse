using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Wall : MonoBehaviour
{
    private float _adjustment = 0.6f; // Wall x and z are off by this when creating compared to free wall space list!
    [SerializeField]
    private Material[] _wallMaterials = null;
    [SerializeField]
    private bool[] _isOutsideWalls = { false, false, false, false };
    private float _shrinkWall = 0.9f; // 90%
    private List<GameObject> _walls = new List<GameObject>();
    // Track free sections of wall
    private List<LineRange>[] _wallEmpty = new List<LineRange>[4];
    // Track areas of the wall that are inside the chaparone area
    private List<LineRange>[] _wallInChaparoneArea = new List<LineRange>[4];
    // All the sub classes
    private Ground _ground;
    private Ceiling _ceiling;

    // Start is called before the first frame update
    void Start()
    {
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground Script in Wall");
        _ceiling = GameObject.Find("Ceiling").GetComponent<Ceiling>();
        if (!_ceiling) Debug.LogError("No Ceiling in Wall");
    }

    public void SetOutsideWall(int wallIndex, bool isOutsideWall)
    {
        _isOutsideWalls[wallIndex] = isOutsideWall;
    }

    /*
     * Get how much was pull in the walls from the full chaparone size
     */
    public float GetWallShrink()
    {
        return _shrinkWall;
    }

    /*
     * Get the list of isOutsideWall flags
     */
    public bool[] GetIsOutsideWalls()
    {
        return _isOutsideWalls;
    }

    /*
     * Get the list of empty sections of the wall
     */
    public List<LineRange> GetEmptySections(int wallIndex)
    {
        return _wallEmpty[wallIndex];
    }

    /*
     * Get the list of empty sections of the wall
     */
    public List<LineRange> GetAccessibleSections(int wallIndex)
    {
        return _wallInChaparoneArea[wallIndex];
    }

    /*
     * Create the walls
     * Walls are slightly smaller than a bounding box of the chaparone area
     * We will in the area outside the chaparone area, but inside walls
     * with debris, furniture, ect
     */
    public void AddWalls()
    {
        GameObject[] walls = new GameObject[4];
        List<Vector3>[] wallPoints = new List<Vector3>[4];
        PlaneRange[] wallRanges = new PlaneRange[4];
        LineRange groundRangeX = _ground.GetXRange();
        LineRange groundRangeZ = _ground.GetZRange();
        _wallEmpty[0] = new List<LineRange>();
        _wallEmpty[0].Add(new LineRange(_adjustment + groundRangeX.min * _shrinkWall, _adjustment + groundRangeX.max * _shrinkWall));
        wallPoints[0] = new List<Vector3>();
        wallPoints[0].Add(new Vector3(groundRangeX.min * _shrinkWall, 0, groundRangeZ.min * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRangeX.min * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.min * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRangeX.max * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.min * _shrinkWall));
        wallPoints[0].Add(new Vector3(groundRangeX.max * _shrinkWall, 0, groundRangeZ.min * _shrinkWall));
        wallRanges[0] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );

        _wallEmpty[1] = new List<LineRange>();
        _wallEmpty[1].Add(new LineRange(_adjustment + groundRangeZ.min * _shrinkWall, _adjustment + groundRangeZ.max * _shrinkWall));
        _wallEmpty[1] = new List<LineRange>();
        _wallEmpty[1].Add(new LineRange(_adjustment + groundRangeZ.min * _shrinkWall, _adjustment + groundRangeZ.max * _shrinkWall));
        wallPoints[1] = new List<Vector3>();
        wallPoints[1].Add(new Vector3(groundRangeX.max * _shrinkWall, 0, groundRangeZ.min * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRangeX.max * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.min * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRangeX.max * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.max * _shrinkWall));
        wallPoints[1].Add(new Vector3(groundRangeX.max * _shrinkWall, 0, groundRangeZ.max * _shrinkWall));
        wallRanges[1] = new PlaneRange(
                0, 0,
                0, 1,
                0, 1
            );
        _wallEmpty[2] = new List<LineRange>();
        _wallEmpty[2].Add(new LineRange(_adjustment + groundRangeX.min * _shrinkWall, _adjustment + groundRangeX.max * _shrinkWall));
        wallPoints[2] = new List<Vector3>();
        wallPoints[2].Add(new Vector3(groundRangeX.max * _shrinkWall, 0, groundRangeZ.max * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRangeX.max * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.max * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRangeX.min * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.max * _shrinkWall));
        wallPoints[2].Add(new Vector3(groundRangeX.min * _shrinkWall, 0, groundRangeZ.max * _shrinkWall));
        wallRanges[2] = new PlaneRange(
                0, 1,
                0, 1,
                0, 0
            );
        _wallEmpty[3] = new List<LineRange>();
        _wallEmpty[3].Add(new LineRange(_adjustment + groundRangeZ.min * _shrinkWall, _adjustment + groundRangeZ.max * _shrinkWall));
        wallPoints[3] = new List<Vector3>();
        wallPoints[3].Add(new Vector3(groundRangeX.min * _shrinkWall, 0, groundRangeZ.max * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRangeX.min * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.max * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRangeX.min * _shrinkWall, _ceiling.GetHeight(), groundRangeZ.min * _shrinkWall));
        wallPoints[3].Add(new Vector3(groundRangeX.min * _shrinkWall, 0, groundRangeZ.min * _shrinkWall));
        int wallMaterialIndex = Random.Range(0, _wallMaterials.Length);
        wallRanges[3] = new PlaneRange(
                0, 0,
                0, 1,
                0, 1
            );
        for (int wallIndex = 0; wallIndex < 4; wallIndex++)
        {
            GameObject wall = new GameObject();
            wall.transform.parent = transform;
            MeshRenderer wallRendered = wall.AddComponent<MeshRenderer>();
            wallRendered.material = _wallMaterials[wallMaterialIndex];
            wall.transform.name = "Wall" + wallIndex + wallRendered.material.name;
            wall.tag = "Wall";
            MeshCollider wallCollider = wall.AddComponent<MeshCollider>();
            MeshFilter wallMeshFilter = wall.AddComponent<MeshFilter>();
            wallMeshFilter.mesh = MeshTools.CreateMeshFromVectors(wallPoints[wallIndex], wallRanges[wallIndex]);
            wallCollider.sharedMesh = wallMeshFilter.mesh;
            _walls.Add(wall);
            CalculateAccessibleWall(wallIndex);
        }
    }

    /*
     * What degree rotation to apply to the wall based on it's index
     */
    public float GetWallRotate(int wallIndex)
    {
        switch (wallIndex)
        {
            case 0: return 0f;
            case 1: return 270f;
            case 2: return 180f;
            case 3: return 90f;
            default: return 0f;
        }
    }

    /*
     * Block a section of the wall, so it doesn't have multiple objects in the same place
     */
    public void AddWallUsage(int wallIndex, LineRange usedWall)
    {
        int sectionIndex = 0;
        while (sectionIndex < _wallEmpty[wallIndex].Count)
        {
            LineRange freeWallSection = _wallEmpty[wallIndex][sectionIndex];
            if ((freeWallSection.min < usedWall.min && freeWallSection.max > usedWall.min) ||
                (freeWallSection.min < usedWall.max && freeWallSection.max > usedWall.max))
            {
                LineRange newFreeSection = new LineRange(usedWall.max, freeWallSection.max);
                if (sectionIndex + 1 == _wallEmpty[wallIndex].Count)
                {
                    // Was last item, so add at end of list
                    _wallEmpty[wallIndex].Add(newFreeSection);
                }
                else
                {
                    // In middle of list so insert in correct place
                    _wallEmpty[wallIndex].Insert(sectionIndex + 1, newFreeSection);
                }

                newFreeSection = new LineRange(freeWallSection.min, usedWall.min);
                _wallEmpty[wallIndex][sectionIndex] = newFreeSection;
                break;
            }
            sectionIndex++;
        }
    }

    public Vector3 AddToWall(int wallIndex, LineRange freeWallSection, float thingWidth, float y)
    {
        LineRange groundRangeX = _ground.GetXRange();
        LineRange groundRangeZ = _ground.GetZRange();
        float thingAtX, thingAtZ;
        LineRange usedWall;
        switch (wallIndex)
        {
            case 0:
                thingAtX = Random.Range(freeWallSection.min, freeWallSection.max - thingWidth);
                thingAtZ = (groundRangeZ.min * GetWallShrink()) + 0f;
                usedWall = new LineRange(thingAtX, thingAtX + thingWidth);
                Debug.Log("ThingX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + thingAtX);
                break;
            case 1:
                thingAtX = (groundRangeX.max * GetWallShrink()) - 0f;
                thingAtZ = Random.Range(freeWallSection.min, freeWallSection.max - thingWidth);
                usedWall = new LineRange(thingAtZ, thingAtZ + thingWidth);
                Debug.Log("ThingZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + thingAtZ);
                break;
            case 2:
                thingAtX = Random.Range(freeWallSection.min, freeWallSection.max - thingWidth);
                thingAtZ = (groundRangeZ.max * GetWallShrink()) - 0f;
                usedWall = new LineRange(thingAtX, thingAtX + thingWidth);
                Debug.Log("ThingX " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + thingAtX);
                break;
            default:
                thingAtX = (groundRangeX.min * GetWallShrink()) + 0f;
                thingAtZ = Random.Range(freeWallSection.min, freeWallSection.max - thingWidth);
                usedWall = new LineRange(thingAtZ, thingAtZ + thingWidth);
                Debug.Log("ThingZ " + wallIndex + "(" + freeWallSection.min + " to " + freeWallSection.max + "): " + thingAtZ);
                break;
        }
        AddWallUsage(wallIndex, usedWall);
        return new Vector3(thingAtX, y, thingAtZ);
    }

    /*
    * Calculate what pars fo the wall can be accessed, as they are inside the chaparone area
    * This is called when _wallEmpty[wallIndex] is the entire wall
    */
    private void CalculateAccessibleWall(int wallIndex)
    {
        LineRange wallRange = _wallEmpty[wallIndex][0];
        _wallInChaparoneArea[wallIndex] = new List<LineRange>();
        // TODO: Check for intersections with the floor?
        _wallInChaparoneArea[wallIndex].Add(wallRange); // For now just allow anywhere
    }
}
