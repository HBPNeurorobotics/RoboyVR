using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AngularLimitStorage {

    public float yLowLimit, yHighLimit;
    public float zLowLimit, zHighLimit;

    public AngularLimitStorage(float yLowLimit, float yHighLimit, float zLowLimit, float zHighLimit)
    {
        this.yLowLimit = yLowLimit;
        this.zLowLimit = zLowLimit;
        this.yHighLimit = yHighLimit;
        this.zHighLimit = zHighLimit;
    }
}
