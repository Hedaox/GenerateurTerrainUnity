using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainHeightType : TerrainHeight
{
    public bool DependOnSlope;
    public float MaxHeightDependOnSlope;
    public float MinHeightDependOnSlope;
    public float Slope;
}