using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Terrain : MonoBehaviour
{
    // Mesh
    public Mesh terrainmesh;
    private Vector3[] vertices;
    private Vector3[] normals;
    private int[] triangles;
    private Vector2[] uvs;
    private double[] slopes;
    private List<double> slopes3rd;
    private double averageSlope = 0;
    private double averageSlope3rd = 0;
    public int Size = 10;
    private int seed;
    private int resolution;
    private int resolutionMultiplier = 16;

    [HideInInspector]
    public bool isGenerated = false;

    // Terrain Noise
    public TerrainNoise TerrainMeshNoise = new TerrainNoise();

    public enum ShowColorTerrainType
    {
        TerrainType,
        Temperature,
        Moisture,
        Biome
    }

    public ShowColorTerrainType showColorTerrainType = ShowColorTerrainType.TerrainType;

    // Height Terrain Types (Mountain, rocks, land ...)
    public List<TerrainHeightType> TerrainHeightTypes = new List<TerrainHeightType>();
    // Terrain Type Map with vertice nb as key and Terrain Type Name as value
    private Dictionary<int, string> terrainTypeMap = new Dictionary<int, string>();

    // Temperature Terrain Types (Hot, Temperate, Cold ...)
    public List<TerrainThresholdColor> TerrainTemperatureTypes = new List<TerrainThresholdColor>();
    // Terrain Temperature Map with vertice nb as key and Terrain Temperature Name as value
    private Dictionary<int, string> terrainTemperatureMap = new Dictionary<int, string>();

    // Moisture Terrain Types (Savana, Tundra, Desert ...)
    public List<TerrainThresholdColor> TerrainMoistureTypes = new List<TerrainThresholdColor>();
    // Moisture noise
    public NoiseFilter moistureNoise = new NoiseFilter();
    // Terrain Moisture Map with vertice nb as key and Terrain Moisture Name as value
    private Dictionary<int, string> terrainMoistureMap = new Dictionary<int, string>();

    // Biome Terrain Types (GrassLand, Savana, Tundra, Desert ...)
    public List<TerrainBiome> TerrainBiomeTypes = new List<TerrainBiome>();
    // Terrain Biome Map with vertice nb as key and Terrain Temperature Name as value
    private Dictionary<int, string> terrainBiomeMap = new Dictionary<int, string>();

    public void OnValidate()
    {
        Size = 10;
        seed = 656;
        applySeed();
        GenerateTerrainMesh();
        TerrainMeshNoise.GlobalStrength = resolutionMultiplier * 50;
        TerrainMeshNoise.GenerateNoise(GetComponent<MeshFilter>().sharedMesh, Size);
        GenerateSlopes();
        GenerateTerrainType();
        GenerateTerrainTemperature();
        GenerateTerrainMoisture();
        GenerateTerrainBiomesColor();
    }

    // Start
    void Start()
    {
        StartCoroutine(GenerateTerrain());
    }

    private IEnumerator GenerateTerrain()
    {
        GameObject.Find("MenuCamera").GetComponent<Camera>().enabled = true;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = true;
        Text loadingNumberText = GameObject.Find("LoadingPercentage").GetComponent<Text>();
        Slider loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();

        Size = GameSettings.Size;
        seed = GameSettings.Seed;
        applySeed();
        loadingNumberText.text = 10 + " %";
        loadingSlider.value = 0.1f;
        yield return null;
        GenerateTerrainMesh();
        loadingNumberText.text = 30 + " %";
        loadingSlider.value = 0.3f;
        yield return null;
        TerrainMeshNoise.GlobalStrength = resolutionMultiplier * 50;
        TerrainMeshNoise.GenerateNoise(GetComponent<MeshFilter>().sharedMesh, Size);
        loadingNumberText.text = 60 + " %";
        loadingSlider.value = 0.6f;
        yield return null;
        GenerateSlopes();
        loadingNumberText.text = 70 + " %";
        loadingSlider.value = 0.7f;
        yield return null;
        GenerateTerrainType();
        loadingNumberText.text = 75 + " %";
        loadingSlider.value = 0.75f;
        yield return null;
        GenerateTerrainTemperature();
        loadingNumberText.text = 80 + " %";
        loadingSlider.value = 0.80f;
        yield return null;
        GenerateTerrainMoisture();
        loadingNumberText.text = 95 + " %";
        loadingSlider.value = 0.95f;
        yield return null;
        GenerateTerrainBiomesColor();
        loadingNumberText.text = 100 + " %";
        loadingSlider.value = 1f;
        yield return new WaitForSeconds(1);

        GameObject.Find("MenuCamera").GetComponent<Camera>().enabled = false;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = false;
    }

    // Apply seed
    private void applySeed()
    {
        foreach(NoiseFilter noiseFilter in TerrainMeshNoise.NoiseFilters)
        {
            noiseFilter.Center = new Vector2(seed, 0);
            if(noiseFilter.ActivateMask)
            {
                noiseFilter.MaskCenter = new Vector2(seed, 0);
            }
        }

        moistureNoise.Center = new Vector2(seed, 0);
    }

    // Generate Terrain Mesh
    public void GenerateTerrainMesh()
    {
        if (terrainmesh == null)
        {
            terrainmesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        }

        // For increasing number of vertices and triangles available
        terrainmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        resolution = Size * resolutionMultiplier;

        transform.localScale = new Vector3(resolution * 10, 1, resolution * 10);

        vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        triangles = new int[resolution * resolution * 6];
        uvs = new Vector2[vertices.Length];

        // Generate Vertices and Triangles meshes
        for (int vi = 0, ti = 0, z = 0; z <= resolution; z++)
        {
            for (int x = 0; x <= resolution; x++, vi++)
            {
                vertices[vi] = new Vector3((float)x / resolution, 0.0f, (float)z / resolution);
                uvs[vi] = new Vector2(vertices[vi].x, vertices[vi].z);

                if (x < resolution - 1 && z < resolution - 1)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + resolution + 1;
                    triangles[ti + 5] = vi + resolution + 2;
                    ti += 6;
                }
            }
        }

        terrainmesh.Clear();

        terrainmesh.vertices = vertices;

        terrainmesh.triangles = triangles;

        terrainmesh.uv = uvs;

        terrainmesh.RecalculateBounds();
        terrainmesh.Optimize();
        terrainmesh.RecalculateNormals();
    }

    // Slope calcul
    public void GenerateSlopes()
    {
        vertices = terrainmesh.vertices;

        normals = terrainmesh.normals;

        slopes = new double[vertices.Length];

        double xA = Vector3.up.x;
        double yA = Vector3.up.y;
        double zA = Vector3.up.z;

        for (int i = 0; i < normals.Length; i++)
        {
            double xB = normals[i].x;
            double yB = normals[i].y;
            double zB = normals[i].z;
            slopes[i] = Math.Acos(Vector3.Dot(Vector3.up, normals[i]) / (Math.Sqrt((xA * xA) + (yA * yA) + (zA * zA)) * Math.Sqrt((xB * xB) + (yB * yB) + (zB * zB)))) * 180 / Math.PI;

            averageSlope += slopes[i];
        }
        averageSlope /= slopes.Length;

        slopes3rd = new List<double>();

        foreach (float slope in slopes)
        {
            if (slope > averageSlope)
            {
                slopes3rd.Add(slope);
                averageSlope3rd += slope;
            }
        }

        averageSlope3rd /= slopes3rd.Count;

        // Change slope depending on Size : 
        foreach (TerrainHeightType terrainHeightType in TerrainHeightTypes)
        {
            terrainHeightType.Slope = 0.0045f;
            if (terrainHeightType.DependOnSlope)
            {
                terrainHeightType.Slope = (float)averageSlope3rd;
            }
        }
    }

    // Generate Terrain Type
    public void GenerateTerrainType()
    {
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;

        vertices = terrainmesh.vertices;

        // Generate colors for each vertices
        for (int i = 0; i < vertices.Length; i++)
        {

            foreach (TerrainHeightType terrainHeightType in TerrainHeightTypes)
            {
                if (TerrainMeshNoise.HeightsWithoutGlobalStrength[i] < terrainHeightType.MaxHeight * TerrainMeshNoise.MaxHeight
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] > terrainHeightType.MinHeight * TerrainMeshNoise.MaxHeight
                    || (terrainHeightType.DependOnSlope
                    && slopes[i] < terrainHeightType.Slope
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] < terrainHeightType.MaxHeightDependOnSlope * TerrainMeshNoise.MaxHeight
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] > terrainHeightType.MinHeightDependOnSlope * TerrainMeshNoise.MaxHeight))
                {
                    terrainTypeMap[i] = terrainHeightType.name;
                }
            }
        }
    }

    // Generate Terrain Temperature
    public void GenerateTerrainTemperature()
    {
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;

        vertices = terrainmesh.vertices;

        // Generate colors for each vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            foreach (TerrainThresholdColor terrainTemperatureColor in TerrainTemperatureTypes)
            {
                if (!(vertices[i].z >= 0.5 + (terrainTemperatureColor.Threshold / 2.0f) - TerrainMeshNoise.HeightsWithoutGlobalStrength[i] || vertices[i].z <= 0.5 - (terrainTemperatureColor.Threshold / 2.0f) + TerrainMeshNoise.HeightsWithoutGlobalStrength[i]))
                {
                    terrainTemperatureMap[i] = terrainTemperatureColor.name;
                }
            }
        }
    }

    // Generate Terrain Moisture
    public void GenerateTerrainMoisture()
    {
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;

        vertices = terrainmesh.vertices;

        // Generate colors for each vertices
        for (int i = 0; i < vertices.Length; i++)
        {

            foreach (TerrainThresholdColor terrainMoistureColor in TerrainMoistureTypes)
            {
                float noiseValue = 0;

                if (moistureNoise.Activate)
                {
                    noiseValue = moistureNoise.Evaluate(new Vector2(vertices[i].x, vertices[i].z));
                }
                if (noiseValue > terrainMoistureColor.Threshold)
                {
                    terrainMoistureMap[i] = terrainMoistureColor.name;
                }
            }
        }
    }

    // Generate Terrain Biomes Color
    public void GenerateTerrainBiomesColor()
    {
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;

        vertices = terrainmesh.vertices;

        Color[] colors = new Color[vertices.Length];

        // Generate colors for each vertices
        for (int i = 0; i < vertices.Length; i++)
        {

            foreach (TerrainBiome terrainBiome in TerrainBiomeTypes)
            {
                if (
                    ((terrainTypeMap.ContainsKey(i) && terrainBiome.TerrainTypesStr.Contains(terrainTypeMap[i])) || terrainBiome.IgnoreTerrainType)
                    &&
                    ((terrainTemperatureMap.ContainsKey(i) && terrainBiome.TemperaturesStr.Contains(terrainTemperatureMap[i])) || terrainBiome.IgnoreTerrainTemperature)
                    &&
                    ((terrainMoistureMap.ContainsKey(i) && terrainBiome.MoisturesStr.Contains(terrainMoistureMap[i])) || terrainBiome.IgnoreTerrainMoisture)
                    &&
                    (terrainBiome.IgnoreHeight
                    ||
                    (TerrainMeshNoise.HeightsWithoutGlobalStrength[i] < terrainBiome.MaxHeight * TerrainMeshNoise.MaxHeight
                    &&
                    TerrainMeshNoise.HeightsWithoutGlobalStrength[i] > terrainBiome.MinHeight * TerrainMeshNoise.MaxHeight)))
                {

                    terrainBiomeMap[i] = terrainBiome.name;
                    if (terrainBiome.IgnoreSecondColor)
                    {
                        colors[i] = terrainBiome.Color;
                    }
                    else
                    {
                        float noiseValue = terrainBiome.TextureNoise.Evaluate(new Vector2(vertices[i].x, vertices[i].z));
                        noiseValue = (noiseValue + 1) / 2;
                        Color textureColor = (terrainBiome.Color * noiseValue + terrainBiome.SecondColor * (Math.Abs(noiseValue - 1))) / 2;
                        colors[i] = textureColor;
                    }
                }
            }
        }

        terrainmesh.colors = colors;
    }
}