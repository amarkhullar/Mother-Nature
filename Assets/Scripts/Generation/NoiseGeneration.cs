using System;
﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGeneration
{

    public static Wave[] generateRandomWaves(int n)
    {
        Wave[] waves = new Wave[n];

        for (int i = 0; i < n; i++)
        {
            waves[i] = new Wave();
            waves[i].amplitude = (i + 1) * 2;
            waves[i].frequency = 1 / (2f * (i + 1f));
            waves[i].seedx = UnityEngine.Random.value * 1000;
            waves[i].seedz = UnityEngine.Random.value * 1000;
        }
        return waves;
    }

    // Provides a way to generate the same waves with the same inputs. It's probably not great, but hey it probably works.
    // All seed numbers essentially chosen at random, hoping that this can make use of perlin noise's property of looking mostly indistinguishable at any point
    public static Wave[] generateWaves(int n, int seed)
    {
        Wave[] waves = new Wave[n];

        float seedx = (seed * (7+seed%7) / (5 +seed%5) * (seed % 2 == 0 ? -1 : 1)) % 15485863;
        float seedz = (seed * (13+seed%13)/(11+seed%11)* (seed % 2 == 0 ? 1 : -1)) % 15485863;

        for (int i = 0; i < n; i++)
        {
            waves[i] = new Wave();
            waves[i].amplitude = (i + 1) * 2;
            waves[i].frequency = 1 / (2f * (i + 1f));
            waves[i].seedx = seedx;
            waves[i].seedz = seedz;

            seedx = (seedx * (7+seedz%5) / (5 +seedz%3) * (seedz % 2 == 0 ? -1 : 1)) % 15485863;
            seedz = (seedz * (13+seedx%17)/(11+seedx%7) * (seedx % 2 == 0 ? 1 : -1)) % 15485863;
        }
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

    public static float[] GetNoiseAtPoints(float[] xs, float[] zs, float scale, float xoffset, float zoffset, Wave[] waves)
    {
        float[] noiseMap = new float[xs.Length];

        for (int i = 0; i < xs.Length; i++)
        {
            float x = xs[i];
            float z = zs[i];
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
            noiseMap[i] = noise;
        }

        return noiseMap;
    }

    public static float GetNoiseAtPoint(float x, float z, float scale, float xoffset, float zoffset, Wave[] waves)
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
        return noise;
    }



}

[System.Serializable]
public class Wave
{
    public float seedx, seedz;
    public float frequency; // The scale of the noise map.
    public float amplitude; // The "weight" of this wave
}
