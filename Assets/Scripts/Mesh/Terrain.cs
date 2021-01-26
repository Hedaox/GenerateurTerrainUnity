using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


/// <summary>
/// Class for the Terrain Mesh.
/// Contains methods for generating Terrain, terrain types and his biomes.
/// </summary>
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Terrain : MonoBehaviour
{
    // Current Terrain Mesh 
    public Mesh terrainmesh;
    // Terrain Mesh Vertices
    private Vector3[] vertices;
    // Terrain Mesh Normals
    private Vector3[] normals;
    // Terrain Mesh Triangles
    private int[] triangles;
    // Terrain Mesh UVs
    private Vector2[] uvs;
    // Terrain Mesh Slopes
    private double[] slopes;
    // Terrain Mesh Slopes that are higher than the average slope value
    private List<double> slopesHigh;
    // Average value of slopes
    private double averageSlope = 0;
    // Average value of high slopes
    private double averageSlopeHigh = 0;
    // Terrain Size
    public int Size = 10;
    // Terain Seed
    private int seed;
    // Terrain Vertices Resolution
    private int resolution;
    // Resolution Multiplier
    private int resolutionMultiplier = 16;

    // Terrain Noise 
    public TerrainNoise TerrainMeshNoise = new TerrainNoise();

    // Used to show the colors of the different Terrain Display Type (used for debug)
    public enum ShowColorTerrainDisplayType
    {
        HeightType,
        Temperature,
        Moisture,
        Biome
    }
    public ShowColorTerrainDisplayType showColorTerrainDisplayType = ShowColorTerrainDisplayType.HeightType;

    // Height Terrain Types (Mountain, rocks, land ...)
    public List<TerrainHeightType> TerrainHeightTypes = new List<TerrainHeightType>();
    // Terrain Type Map with vertice nb as key and Terrain Type Name as value
    private Dictionary<int, string> terrainTypeMap = new Dictionary<int, string>();

    // Temperature Terrain Types (Hot, Temperate, Cold ...)
    public List<TerrainThreshold> TerrainTemperatureTypes = new List<TerrainThreshold>();
    // Terrain Temperature Map with vertice nb as key and Terrain Temperature Name as value
    private Dictionary<int, string> terrainTemperatureMap = new Dictionary<int, string>();

    // Moisture Terrain Types (Savana, Tundra, Desert ...)
    public List<TerrainThreshold> TerrainMoistureTypes = new List<TerrainThreshold>();
    // Moisture noise
    public NoiseFilter moistureNoise = new NoiseFilter();
    // Terrain Moisture Map with vertice nb as key and Terrain Moisture Name as value
    private Dictionary<int, string> terrainMoistureMap = new Dictionary<int, string>();

    // Biome Terrain Types (GrassLand, Savana, Tundra, Desert ...)
    public List<TerrainBiome> TerrainBiomeTypes = new List<TerrainBiome>();
    // Terrain Biome Map with vertice nb as key and Terrain Temperature Name as value
    private Dictionary<int, string> terrainBiomeMap = new Dictionary<int, string>();

    // Called when the script is loaded or a value is changed in the Inspector (runs in editor)
    public void OnValidate()
    {
        // Generate a Terrain of size 10 and seed 656
        Size = 10;
        seed = 656;
        applySeed();
        GenerateTerrainMesh();
        // Change strength of noise depending on resolution multiplier
        TerrainMeshNoise.GlobalStrength = resolutionMultiplier * 50;
        // Generate Noise and apply on the Terrain
        TerrainMeshNoise.GenerateNoise(GetComponent<MeshFilter>().sharedMesh, Size);

        GenerateSlopes();
        GenerateTerrainHeightType();
        GenerateTerrainTemperature();
        GenerateTerrainMoisture();

        GenerateTerrainBiomesColor();
    }

    // Called at the Start
    void Start()
    {
        // Generate Terrain on Start
        StartCoroutine(GenerateTerrain());
    }

    /// <summary>
    ///  Generate Terrain and display a Loading screen in the meantime
    /// </summary>
    private IEnumerator GenerateTerrain()
    {
        // Enable Loading menu
        GameObject.Find("MenuCamera").GetComponent<Camera>().enabled = true;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = true;
        // Get Text and Slider for the Loading
        Text loadingNumberText = GameObject.Find("LoadingPercentage").GetComponent<Text>();
        Slider loadingSlider = GameObject.Find("LoadingSlider").GetComponent<Slider>();

        // Use Game Settings for getting Size and Seed
        Size = GameSettings.Size;
        seed = GameSettings.Seed;

        // Generate a Terrain in game like in OnValidate() while advancing the loading bar
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
        GenerateTerrainHeightType();
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

        // Remove Loading Screen
        GameObject.Find("MenuCamera").GetComponent<Camera>().enabled = false;
        GameObject.Find("MenuLoading").GetComponent<Canvas>().enabled = false;
    }

    /// <summary>
    ///  Apply the seed using noise filters from the TerrainMeshNoise
    /// </summary>
    private void applySeed()
    {
        // For each filter in the TerrainMeshNoise
        foreach(NoiseFilter noiseFilter in TerrainMeshNoise.NoiseFilters)
        {
            // Seed is used to change the center of the noise
            noiseFilter.Center = new Vector2(seed, 0);
            // If mask is used
            if(noiseFilter.ActivateMask)
            {
                // Seed also changes the center of the mask
                noiseFilter.MaskCenter = new Vector2(seed, 0);
            }
        }
        // Seed changes the center of the Moisture Noise
        moistureNoise.Center = new Vector2(seed, 0);
    }

    /// <summary>
    /// Generate Simple Plane Mesh.
    /// </summary>
    public void GenerateTerrainMesh()
    {
        // Create a new mesh if not existing
        if (terrainmesh == null)
        {
            terrainmesh = GetComponent<MeshFilter>().sharedMesh = new Mesh();
        }

        // For increasing number of vertices and triangles available
        terrainmesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        // Resolution calculation 
        resolution = Size * resolutionMultiplier;
        // Scale Mesh depending on resolution
        transform.localScale = new Vector3(resolution * 10, 1, resolution * 10);

        // Declaring Vertices, Triangles and UVs
        vertices = new Vector3[(resolution + 1) * (resolution + 1)];
        triangles = new int[resolution * resolution * 6];
        uvs = new Vector2[vertices.Length];

        // Generate Vertices, Triangles meshes and UVs for making a simple plane
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

        // Clear previous mesh
        terrainmesh.Clear();

        // Apply the new mesh values
        terrainmesh.vertices = vertices;
        terrainmesh.triangles = triangles;
        terrainmesh.uv = uvs;

        // Calculate normals and optimisation
        terrainmesh.RecalculateBounds();
        terrainmesh.Optimize();
        terrainmesh.RecalculateNormals();
    }

    /// <summary>
    /// Slopes calculation.
    /// </summary>
    public void GenerateSlopes()
    {
        // Get current mesh values
        vertices = terrainmesh.vertices;
        normals = terrainmesh.normals;
        slopes = new double[vertices.Length];

        // Get up values
        double xA = Vector3.up.x;
        double yA = Vector3.up.y;
        double zA = Vector3.up.z;

        // For each normal
        for (int i = 0; i < normals.Length; i++)
        {
            // Get xyz values
            double xB = normals[i].x;
            double yB = normals[i].y;
            double zB = normals[i].z;
            // calculate slope on this normal with up values
            slopes[i] = Math.Acos(Vector3.Dot(Vector3.up, normals[i]) / (Math.Sqrt((xA * xA) + (yA * yA) + (zA * zA)) * Math.Sqrt((xB * xB) + (yB * yB) + (zB * zB)))) * 180 / Math.PI;
            
            // Add slopes in the average
            averageSlope += slopes[i];
        }
        // Average slope calculation
        averageSlope /= slopes.Length;

        // Get all slopes higher than the average
        slopesHigh = new List<double>();
        foreach (float slope in slopes)
        {
            if (slope > averageSlope)
            {
                slopesHigh.Add(slope);
                averageSlopeHigh += slope;
            }
        }

        // High slope average
        averageSlopeHigh /= slopesHigh.Count;

        // For each Height Type
        foreach (TerrainHeightType terrainHeightType in TerrainHeightTypes)
        {
            // If terrain height depends on slope (eg : Beach and Land)
            if (terrainHeightType.DependOnSlope)
            {
                // Average Slope used with Height Type
                terrainHeightType.Slope = (float)averageSlopeHigh;
            }
        }
    }

    /// <summary>
    /// Generate Height Type for each vertices.
    /// </summary>
    public void GenerateTerrainHeightType()
    {
        // Get Mesh and vertices
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = terrainmesh.vertices;

        // For each vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // For each Height Type
            foreach (TerrainHeightType terrainHeightType in TerrainHeightTypes)
            {
                // Check if Vertices is compatible with terrain Height type
                if (TerrainMeshNoise.HeightsWithoutGlobalStrength[i] < terrainHeightType.MaxHeight * TerrainMeshNoise.MaxHeight
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] > terrainHeightType.MinHeight * TerrainMeshNoise.MaxHeight
                    || (terrainHeightType.DependOnSlope
                    && slopes[i] < terrainHeightType.Slope
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] < terrainHeightType.MaxHeightDependOnSlope * TerrainMeshNoise.MaxHeight
                    && TerrainMeshNoise.HeightsWithoutGlobalStrength[i] > terrainHeightType.MinHeightDependOnSlope * TerrainMeshNoise.MaxHeight))
                {
                    // Put the terrain Height type on that vertice
                    terrainTypeMap[i] = terrainHeightType.name;
                }
            }
        }
    }

    /// <summary>
    /// Generate Temperature Type for each vertices.
    /// </summary>
    public void GenerateTerrainTemperature()
    {
        // Get Mesh and vertices
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = terrainmesh.vertices;

        // For each vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // For each Terrain Temperature Type
            foreach (TerrainThreshold terrainTemperature in TerrainTemperatureTypes)
            {
                // Check if Vertices is compatible with terrain Temperature type
                if (!(vertices[i].z >= 0.5 + (terrainTemperature.Threshold / 2.0f) - TerrainMeshNoise.HeightsWithoutGlobalStrength[i] || vertices[i].z <= 0.5 - (terrainTemperature.Threshold / 2.0f) + TerrainMeshNoise.HeightsWithoutGlobalStrength[i]))
                {
                    // Put the terrain Temperature type on that vertice
                    terrainTemperatureMap[i] = terrainTemperature.name;
                }
            }
        }
    }

    /// <summary>
    /// Generate Moisture Type for each vertices.
    /// </summary>
    public void GenerateTerrainMoisture()
    {
        // Get Mesh and vertices
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = terrainmesh.vertices;

        // For each vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // For each Terrain Moisture Type
            foreach (TerrainThreshold terrainMoisture in TerrainMoistureTypes)
            {
                float noiseValue = 0;
                
                // If we want to use Noise on Moisture
                if (moistureNoise.Activate)
                {
                    // Apply some noise on the moisture
                    noiseValue = moistureNoise.Evaluate(new Vector2(vertices[i].x, vertices[i].z));
                }
                // Check if Vertices is compatible with terrain Moisture type
                if (noiseValue > terrainMoisture.Threshold)
                {
                    // Put the terrain Moisture type on that vertice
                    terrainMoistureMap[i] = terrainMoisture.name;
                }
            }
        }
    }

    /// <summary>
    /// Generate Biome Type for each vertices.
    /// Depends on Height, Temperature and Moisture.
    /// </summary>
    public void GenerateTerrainBiomesColor()
    {
        // Get Mesh and vertices
        terrainmesh = GetComponent<MeshFilter>().sharedMesh;
        vertices = terrainmesh.vertices;
        // For coloring the terrain
        Color[] colors = new Color[vertices.Length];

        // For each vertices
        for (int i = 0; i < vertices.Length; i++)
        {
            // For each type of Biome
            foreach (TerrainBiome terrainBiome in TerrainBiomeTypes)
            {
                // Check if Vertices is compatible with terrain Biome type
                if (
                    ((terrainTypeMap.ContainsKey(i) && terrainBiome.HeightTypesStr.Contains(terrainTypeMap[i])) || terrainBiome.IgnoreTerrainHeight)
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
                    // Put the terrain Biome type on that vertice
                    terrainBiomeMap[i] = terrainBiome.name;

                    // If the biome have only one color
                    if (terrainBiome.IgnoreSecondColor)
                    {
                        // Apply color
                        colors[i] = terrainBiome.Color;
                    }
                    else
                    {
                        // Generate noise for apllying the two colors
                        float noiseValue = terrainBiome.TextureNoise.Evaluate(new Vector2(vertices[i].x, vertices[i].z));
                        noiseValue = (noiseValue + 1) / 2;
                        Color textureColor = (terrainBiome.Color * noiseValue + terrainBiome.SecondColor * (Math.Abs(noiseValue - 1))) / 2;
                        colors[i] = textureColor;
                    }
                }
            }
        }

        // Apply colors on all vertices
        terrainmesh.colors = colors;
    }
}