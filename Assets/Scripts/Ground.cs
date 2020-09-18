using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Ground : MonoBehaviour
{
    [SerializeField]
    private Material[] _groundMaterials = null;
    [SerializeField]
    // If there are parts of the floor missing, we render the floor differently
    private bool _missingFloor = false;
    private LineRange xRange;
    private LineRange zRange;

    // Chaparone
    private float _chaperoneAngleOffset = 90f;
    private HmdQuad_t[] _chaperoneQuads = null;
    private List<Vector3> _chaparoneVector3s;

    public void AddGround()
    {
        if (CreateChaporoneQuads())
        {
            CreateVector3();
        }
        else
        {
            Debug.LogError("No Chaparone");
        }
        PopulateGround(_chaparoneVector3s);
    }

    /*
     * Convert the Head Mounted Device Quad to a Vector3
     */
    private Vector3 GetPoint(int index)
    {
        Vector3 temp = transform.localScale;
        temp.x = _chaperoneQuads[index].vCorners0.v2;
        temp.y = _chaperoneQuads[index].vCorners0.v1;
        temp.z = _chaperoneQuads[index].vCorners0.v0;
        Quaternion q = Quaternion.AngleAxis(_chaperoneAngleOffset, new Vector3(0, 1, 0));

        temp = q * temp;

        return temp;
    }

    /*
     * Create the chaparone quads.
     * 
     * @return Success
     */
    private bool CreateChaporoneQuads()
    {
        if (_chaperoneQuads == null)
        {
            CVRChaperoneSetup chaperone = OpenVR.ChaperoneSetup;
            bool success = (chaperone != null) && chaperone.GetLiveCollisionBoundsInfo(out _chaperoneQuads);
            if (!success)
            {
                Debug.LogError("Failed to get Calibrated Chaperone bounds!  Make sure you have tracking first, and that your space is calibrated.");
                return false;
            }
        }
        return true;
    }

    /*
     * Create the chaparone Vector3 points.
     * 
     * @return Success
     */
    private bool CreateVector3()
    {
        _chaparoneVector3s = new List<Vector3>();
        if (_chaperoneQuads.Length > 0)
        {
            for (int i = 0; i < _chaperoneQuads.Length; i++)
            {
                Vector3 point = GetPoint(i);
                _chaparoneVector3s.Add(point);
                SetRanges(point.x, point.z);
            }
        }
        else
        {
            Debug.LogError("No Chaporone points");
            return false;
        }

        // We need to turn the plane upside down so it shows the correct side to the player with the texture on
        _chaparoneVector3s.Reverse();
        return true;
    }

    // Create the ground
    private void PopulateGround(List<Vector3> points)
    {
        GameObject ground = new GameObject();
        ground.transform.parent = transform;
        int groundMaterialIndex = Random.Range(0, _groundMaterials.Length);
        MeshRenderer groundRendered = ground.AddComponent<MeshRenderer>();
        groundRendered.material = _groundMaterials[groundMaterialIndex];
        ground.transform.name = "Ground" + groundRendered.material.name;
        ground.tag = "Ground";
        MeshCollider groundCollider = ground.AddComponent<MeshCollider>();
        MeshFilter groundMeshFilter = ground.AddComponent<MeshFilter>();
        PlaneRange range = new PlaneRange(0, 1f, 0, 0, 0, 1f);

        if (_missingFloor)
        {
            // TODO: Change to only render floor that exists
            groundMeshFilter.mesh = MeshTools.CreateMeshFromVectors(points, range);
        }
        else
        {
            groundMeshFilter.mesh = MeshTools.CreateMeshFromVectors(points, range);

        }
        groundCollider.sharedMesh = groundMeshFilter.mesh;
    }

    public void SetRanges(float x, float z)
    {
        if (xRange == null) xRange = new LineRange(x, x);
        if (zRange == null) zRange = new LineRange(z, z);
        xRange.SetRange(x);
        zRange.SetRange(z);
    }

    public LineRange GetXRange()
    {
        return xRange;
    }

    public LineRange GetZRange()
    {
        return zRange;
    }
}
