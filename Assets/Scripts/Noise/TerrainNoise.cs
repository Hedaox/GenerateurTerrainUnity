using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TerrainNoise
{
    // Noise Settings
    public float Persistence = 0.5f;
    [Range(0, 100000)]
    public int GlobalStrength = 10;
    public float MaxHeight = 0;
    public float MinHeight = 0;

    public float Diviser = 0;
    public List<NoiseFilter> NoiseFilters = new List<NoiseFilter>();

    [HideInInspector]
    public float[] HeightsWithoutGlobalStrength;

    // Generate noise for Terrain
    public void GenerateNoise(Mesh sharedMesh, int size)
    {
        Mesh mesh = sharedMesh;

        Vector3[] vertices = mesh.vertices;

        HeightsWithoutGlobalStrength = new float[vertices.Length];

        MaxHeight = 0;
        MinHeight = 0;

        // For each vertices generate noise
        for (int i = 0; i < vertices.Length; i++)
        {
            float amplitude = 1;
            float terrainNoiseValue = 0;

            // Add up every noise filters
            for(int j = 0; j < NoiseFilters.Count; j++)
            {
                if (NoiseFilters[j].Activate)
                {
                    terrainNoiseValue += (float)NoiseFilters[j].Evaluate(new Vector2(vertices[i].x * size / 10, vertices[i].z * size / 10));

                    if(j > 0)
                    {
                        amplitude *= Persistence;
                    }
                }
            }

            terrainNoiseValue *= amplitude;

            if (terrainNoiseValue > MaxHeight)
            {
                MaxHeight = terrainNoiseValue;
            }
            else if(terrainNoiseValue < MinHeight)
            {
                MinHeight = terrainNoiseValue;
            }

            HeightsWithoutGlobalStrength[i] = terrainNoiseValue;

            vertices[i].y = terrainNoiseValue * GlobalStrength;
        }

        mesh.vertices = vertices;

        mesh.RecalculateBounds();
        mesh.Optimize();
        mesh.RecalculateNormals();
    }
}
