using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGeneration
{
    private static Wave[] ws = null;
    public static Wave[] generateRandomWavesSingleton(int n)
    {
        if (ws != null) return ws;
        Wave[] waves = new Wave[n];

        for(int i = 0; i < n; i++)
        {
            waves[i] = new Wave();
            waves[i].amplitude = (i + 1) * 2;
            waves[i].frequency = 1 / (2f*(i+1f));
            waves[i].seedx = Random.value * 1000;
            waves[i].seedz = Random.value * 1000;
        }
        ws = waves;
        return waves;
    }

    // Like the function above, but we're repeatedly sampling different areas of the noise with differing weights, to make it more natural.
    public static float[,] generateNoiseMap(int depth, int width, float scale, float xoffset, float zoffset, Wave[] waves)
    {
        float[,] noiseMap = new float[depth, width];

        for (int z = 0; z < depth; z++)
        {
            for (int x = 0; x < depth; x++)
            {
                float sx = (x + xoffset) / scale;
                float sz = (z + zoffset) / scale;

                float noise = 0f;
                float normalization = 0f;
                foreach (Wave w in waves)
                {
                    noise += w.amplitude * Mathf.PerlinNoise(sx * w.frequency + w.seedx, sz * w.frequency + w.seedz);
                    normalization += w.amplitude;
                }
                noise /= normalization;

                noiseMap[z, x] = noise;
            }

        }
        return noiseMap;
    }
}

[System.Serializable]
public class Wave
{
    public float seedx, seedz;
    public float frequency; // The scale of the noise map.
    public float amplitude; // The "weight" of this wave
}