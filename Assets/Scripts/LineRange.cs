using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*
 * The min and max values in a single dimension
 */
public class LineRange
{
    public float min;
    public float max;

    public LineRange(float inMin, float inMax)
    {
        this.min = inMin;
        this.max = inMax;
    }

    public float Range()
    {
        return max - min;
    }

    public void SetRange(float value)
    {
        if (min > value) min = value;
        if (max < value) max = value;
    }

    override public string ToString()
    {
        return "LineRange:" + min + " to " + max;
    }
}
