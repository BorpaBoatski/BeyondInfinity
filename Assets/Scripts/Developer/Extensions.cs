using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static Vector2 XY(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static void BlocksAndVisible(this CanvasGroup group, bool state)
    {
        group.alpha = state ? 1 : 0;
        group.blocksRaycasts = state;
    }
}
