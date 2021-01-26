using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simple Terrain Type
/// </summary>
[System.Serializable]
public class TerrainType
{
    // Terrain name (used to compare type)
    public string name;
    // Terrain color (used to apply color on mesh)
    public Color Color;
}
