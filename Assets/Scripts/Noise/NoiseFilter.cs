using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

/// <summary>
/// Noise Filter, use to generate random terrain or random texture.
/// </summary>
[System.Serializable]
public class NoiseFilter
{
    // Noise
    FastNoise noise = new FastNoise();
    // Activate the noise
    public bool Activate = true;
    // Define the noise type with the FastNoise Library
    public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Value;
    // Roughness of the noise
    [Range(0, 10000)]
    public int Roughness = 100;
    // Center of the noise, used to change noise 
    public Vector2 Center = new Vector3(0, 0);
    // Max floor height (eg: used to make plateau)
    [Range(-1, 1)]
    public float MaxFloorHeight = 1;
    // Min floor height (eg: ocean plateau)
    [Range(-1, 1)]
    public float MinFloorHeight = -1;

    // Mask Noise
    FastNoise masknoise = new FastNoise();
    // Activate the mask
    public bool ActivateMask = false;
    // Define the noise for the mask
    public FastNoise.NoiseType maskNoiseType = FastNoise.NoiseType.Value;
    // Roughness of the mask noise
    [Range(0, 1000)]
    public int MaskRoughness = 100;
    // Center of the mask noise
    public Vector2 MaskCenter = new Vector3(0, 0);

    /// <summary>
    /// Evaluate Noise with Vector2 point
    /// </summary>
    public float Evaluate(Vector2 point)
    {
        // Set noise type
        noise.SetNoiseType(noiseType);
        masknoise.SetNoiseType(maskNoiseType);

        // Get value for the vector2 point
        float noiseValue = noise.GetNoise(point.x * Roughness + Center.x, point.y * Roughness + Center.y);
        // Default mask value (used when mask is not activate)
        float maskValue = 1.0f;
        // Check if the mask is activate
        if (ActivateMask)
        {
            // Get value for the vector2 point
            maskValue = masknoise.GetNoise(point.x * MaskRoughness + MaskCenter.x, point.y * MaskRoughness + MaskCenter.y);
        }

        // Manage max and min Height 
        if (noiseValue > MaxFloorHeight)
        {
            noiseValue = MaxFloorHeight;
        }
        else if (noiseValue < MinFloorHeight)
        {
            noiseValue = MinFloorHeight;
        }

        // return the noise value with the mask value
        return noiseValue * maskValue;
    }
}
