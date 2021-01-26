using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Terrain Height with slope management
/// </summary>
[System.Serializable]
public class TerrainHeightType : TerrainMaxMinHeight
{
    // Check if terrain depends on slope
    public bool DependOnSlope;
    // Max or Min Height where terrain type is affected by slope
    public float MaxHeightDependOnSlope;
    public float MinHeightDependOnSlope;
    // Average slope
    public float Slope;
}