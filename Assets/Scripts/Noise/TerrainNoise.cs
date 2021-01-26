using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class is used to generate noise for the Terrain.
/// </summary>
[System.Serializable]
public class TerrainNoise
{
    // Persistence of the noise when going trough multiple noise filters
    public float Persistence = 0.5f;
    // Global Strength (will make higher peaks)
    [Range(0, 100000)]
    public int GlobalStrength = 10;
    // Max and Min height for plateau
    public float MaxHeight = 0;
    public float MinHeight = 0;

    // All noise filters for the terrain
    public List<NoiseFilter> NoiseFilters = new List<NoiseFilter>();

    // The height without applying the global strength
    [HideInInspector]
    public float[] HeightsWithoutGlobalStrength;

    /// <summary>
    /// Generate noise for Terrain
    /// </summary>
    public void GenerateNoise(Mesh sharedMesh, int size)
    {
        // Get Mesh
        Mesh mesh = sharedMesh;
        // Get Vertices 
        Vector3[] vertices = mesh.vertices;

        HeightsWithoutGlobalStrength = new float[vertices.Length];

        MaxHeight = 0;
        MinHeight = 0;

        // For each vertices generate noise
        for (int i = 0; i < vertices.Length; i++)
        {
            // Amplitude is use to manage strength of noise with filters
            float amplitude = 1;
            // Value of the noise at this vertice
            float terrainNoiseValue = 0;

            // Add up every noise filters
            for(int j = 0; j < NoiseFilters.Count; j++)
            {
                // If this filter is activated
                if (NoiseFilters[j].Activate)
                {
                    // Evaluate noise on this vertice
                    terrainNoiseValue += (float)NoiseFilters[j].Evaluate(new Vector2(vertices[i].x * size / 10, vertices[i].z * size / 10));

                    // Apply persistence on the amplitude except for first filter
                    if(j > 0)
                    {
                        amplitude *= Persistence;
                    }
                }
            }
            // Apply amplitude on noise value
            terrainNoiseValue *= amplitude;

            // For managing plateau
            if (terrainNoiseValue > MaxHeight)
            {
                MaxHeight = terrainNoiseValue;
            }
            else if(terrainNoiseValue < MinHeight)
            {
                MinHeight = terrainNoiseValue;
            }

            // Assign Heights without the Global Strength
            HeightsWithoutGlobalStrength[i] = terrainNoiseValue;

            // Assign real Heights
            vertices[i].y = terrainNoiseValue * GlobalStrength;
        }

        // Change vertices 
        mesh.vertices = vertices;

        // Calculate normals and optimisation
        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
