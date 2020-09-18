using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * The Plane Range Details
 * The min and max values in all three dimensions
 */
public class PlaneRange
{
    public float minX;
    public float maxX;
    public float minY;
    public float maxY;
    public float minZ;
    public float maxZ;

    public PlaneRange(float x, float y, float z)
    {
        minX = x;
        maxX = x;
        minY = y;
        maxY = y;
        minZ = z;
        maxZ = z;
    }

    public PlaneRange(float xMin, float xMax, float yMin, float yMax, float zMin, float zMax)
    {
        this.minX = xMin;
        this.maxX = xMax;
        this.minY = yMin;
        this.maxY = yMax;
        this.minZ = zMin;
        this.maxZ = zMax;
    }

    public float XRange()
    {
        return maxX - minX;
    }

    public float YRange()
    {
        return maxY - minY;
    }

    public float ZRange()
    {
        return maxZ - minZ;
    }
}
