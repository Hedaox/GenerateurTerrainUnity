using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Biome Terrain Type.
/// </summary>
[System.Serializable]
public class TerrainBiome : TerrainMaxMinHeight
{
    // Second color for the biome
    public Color SecondColor;

    // Noise filter for this type
    public NoiseFilter TextureNoise = new NoiseFilter();

    // Used for ignoring the second color (Biome with only one color)
    public bool IgnoreSecondColor;

    // Used for setting what Height Type the biome will be placed in
    public List<string> HeightTypesStr;
    // Used for ignoring the Height Types
    public bool IgnoreTerrainHeight;

    // Used for setting what Temperatures Type the biome will be placed in
    public List<string> TemperaturesStr;
    // Used for ignoring the Temperatures Types
    public bool IgnoreTerrainTemperature;

    // Used for setting what Moisture Type the biome will be placed in
    public List<string> MoisturesStr;
    // Used for ignoring the Moistures Types
    public bool IgnoreTerrainMoisture;
}