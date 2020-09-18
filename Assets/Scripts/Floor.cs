using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Floor : MonoBehaviour
{
    [SerializeField]
    private Material[] _groundMaterials = null;
    [SerializeField]
    // If there are parts of the floor missing, we render the floor differently
    private bool _missingFloor = false;
    private Chaparone _chaparone;

    // Start is called before the first frame update
    void Start()
    {
        PopulateGround();
    }

    public Chaparone GetChaparone()
    {
        if(_chaparone == null) _chaparone = GetComponent<Chaparone>();
        return _chaparone;
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
            groundMeshFilter.mesh = MeshTools.CreateMeshFromVectors(GetChaparone().GetPoints(), range);
        }
        else
        {
            groundMeshFilter.mesh = MeshTools.CreateMeshFromVectors(GetChaparone().GetPoints(), range);

        }
        groundCollider.sharedMesh = groundMeshFilter.mesh;
    }
}
