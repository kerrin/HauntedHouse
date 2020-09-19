using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ceiling : MonoBehaviour
{
    [SerializeField]
    private Material[] _ceilingMaterials = null;
    [SerializeField]
    private float _ceilingHeight = 2.5f;
    // All the sub classes
    private Ground _ground;
    private Wall _wall;

    // Start is called before the first frame update
    void Start()
    {
        _ground = GameObject.Find("Ground").GetComponent<Ground>();
        if (!_ground) Debug.LogError("No Ground in Ceiling");
        _wall = GameObject.Find("Walls").GetComponent<Wall>();
        if (!_wall) Debug.LogError("No Wall in Ceiling");
    }

    /*
     * Get how far from the floor the ceiling is
     */
    public float GetHeight()
    {
        return _ceilingHeight;
    }

    /*
     * Add a ceiling to the room
     */
    public void AddCeiling()
    {
        GameObject ceiling = new GameObject();
        ceiling.transform.parent = transform;
        int ceilingMaterialIndex = Random.Range(0, _ceilingMaterials.Length);
        MeshRenderer ceilingRendered = ceiling.AddComponent<MeshRenderer>();
        ceilingRendered.material = _ceilingMaterials[ceilingMaterialIndex];
        ceiling.transform.name = "Ceiling" + ceilingRendered.material.name;
        MeshCollider ceilingCollider = ceiling.AddComponent<MeshCollider>();
        MeshFilter ceilingMeshFilter = ceiling.AddComponent<MeshFilter>();
        List<Vector3> ceilingPoints = new List<Vector3>();
        LineRange groundRangeX = _ground.GetXRange();
        LineRange groundRangeZ = _ground.GetZRange();
        ceilingPoints.Add(new Vector3(groundRangeX.max * _wall.GetWallShrink(), _ceilingHeight, groundRangeZ.min * _wall.GetWallShrink()));
        ceilingPoints.Add(new Vector3(groundRangeX.min * _wall.GetWallShrink(), _ceilingHeight, groundRangeZ.min * _wall.GetWallShrink()));
        ceilingPoints.Add(new Vector3(groundRangeX.min * _wall.GetWallShrink(), _ceilingHeight, groundRangeZ.max * _wall.GetWallShrink()));
        ceilingPoints.Add(new Vector3(groundRangeX.max * _wall.GetWallShrink(), _ceilingHeight, groundRangeZ.max * _wall.GetWallShrink()));
        
        PlaneRange ceilingRange = new PlaneRange(
                0, 1,
                0, 0,
                0, 1
            );
        ceilingMeshFilter.mesh = MeshTools.CreateMeshFromVectors(ceilingPoints, ceilingRange);

        ceilingCollider.sharedMesh = ceilingMeshFilter.mesh;
    }
}
