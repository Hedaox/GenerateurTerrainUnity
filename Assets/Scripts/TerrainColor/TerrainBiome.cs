using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TerrainBiome : TerrainHeight
{
    public Color SecondColor;

    public NoiseFilter TextureNoise = new NoiseFilter();

    public bool IgnoreSecondColor;

    public List<string> TerrainTypesStr;
    public bool IgnoreTerrainType;

    public List<string> TemperaturesStr;
    public bool IgnoreTerrainTemperature;

    public List<string> MoisturesStr;
    public bool IgnoreTerrainMoisture;
}