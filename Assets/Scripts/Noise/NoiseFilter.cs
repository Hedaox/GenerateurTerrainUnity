using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[System.Serializable]
public class NoiseFilter
{
    public bool Activate = true;
    public FastNoise.NoiseType noiseType = FastNoise.NoiseType.Value;
    [Range(0, 10000)]
    public int Roughness = 100;
    public Vector2 Center = new Vector3(0, 0);
    [Range(-1, 1)]
    public float MaxFloorHeight = 1;
    [Range(-1, 1)]
    public float MinFloorHeight = -1;
    public bool ActivateMask = false;
    public FastNoise.NoiseType maskNoiseType = FastNoise.NoiseType.Value;
    [Range(0, 1000)]
    public int MaskRoughness = 100;
    public Vector2 MaskCenter = new Vector3(0, 0);

    FastNoise noise = new FastNoise();
    FastNoise masknoise = new FastNoise();

    // Evaluate Noise with Vector2 point
    public float Evaluate(Vector2 point)
    {
        noise.SetNoiseType(noiseType);
        masknoise.SetNoiseType(maskNoiseType);

        float noiseValue = noise.GetNoise(point.x * Roughness + Center.x, point.y * Roughness + Center.y);
        float maskValue = 1.0f;
        if (ActivateMask)
        {
            maskValue = masknoise.GetNoise(point.x * MaskRoughness + MaskCenter.x, point.y * MaskRoughness + MaskCenter.y);
        }

        if (noiseValue > MaxFloorHeight)
        {
            noiseValue = MaxFloorHeight;
        }
        else if (noiseValue < MinFloorHeight)
        {
            noiseValue = MinFloorHeight;
        }

        return noiseValue * maskValue;
    }
}
