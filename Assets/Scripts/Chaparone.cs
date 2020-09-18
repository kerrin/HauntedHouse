using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class Chaparone : MonoBehaviour
{
    private float _chaperoneAngleOffset = 90f;
    private HmdQuad_t[] _chaperoneQuads = null;
    List<Vector3> _chaparoneVector3s = new List<Vector3>();
    private PlaneRange _groundRange = null;

    private void Start()
    {
        if (CreateChaporoneQuads())
        {
            CreateVector3();
        }
    }
    /*
     * Get all the Vector3 points that outline the chaparone area
     */
    public List<Vector3> GetPoints()
    {
        return _chaparoneVector3s;
    }

    /*
     * Get all the Vector3 points that outline the chaparone area
     */
    public PlaneRange GetGroundRange()
    {
        return _groundRange;
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
        if (_chaperoneQuads.Length > 0)
        {
            Vector3 point = GetPoint(0);
            _groundRange = new PlaneRange(point.x, 0, point.z);
            _chaparoneVector3s.Add(point);
            for (int i = 1; i < _chaperoneQuads.Length; i++)
            {
                point = GetPoint(i);
                _chaparoneVector3s.Add(point);
                SetRanges(point.x, ref _groundRange.minX, ref _groundRange.maxX, point.z, ref _groundRange.minZ, ref _groundRange.maxZ);
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
}
