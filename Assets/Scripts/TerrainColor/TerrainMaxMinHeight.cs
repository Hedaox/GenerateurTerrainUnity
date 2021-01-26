using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Terrain type that has a min and max Height
/// </summary>
[System.Serializable]
public class TerrainMaxMinHeight : TerrainType
{
    // Max or min height where this terrain type should be apply
    public float MaxHeight;
    public float MinHeight;

    // True if we want to ignore the height max and min
    public bool IgnoreHeight;
}

