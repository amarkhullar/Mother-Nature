using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class HexTileMetrics
{

    public const float outerRadius = 10f; // ie: radius to corner
    public const float innerRadius = outerRadius * 0.866025404f;
    public const float distBetween = innerRadius + 0.1f;

    public static Vector3[] corners = {
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius) // Stops an out of bounds exception, acts as a wraparound.
    };
}
