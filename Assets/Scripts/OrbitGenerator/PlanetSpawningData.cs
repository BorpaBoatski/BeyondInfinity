using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct PlanetData
{
    public PlanetSize PlanetSize;
    public float RequiredDistance;
    public int Score;
}

[System.Serializable]
public struct PlanetDetails
{
    public string PlanetName;
    public Sprite PlanetSprite;
    [TextArea(2,3)]
    public string PlanetDescription;
}

[CreateAssetMenu(fileName = "PlanetSpawningData", menuName = "Database/PlanetSpawningData")]
public class PlanetSpawningData : ScriptableObject
{
    public PlanetData[] PlanetDatas;
    public PlanetDetails[] PlanetDetails;

    public PlanetData FindPlanetData(PlanetSize size)
    {
        for (int i = 0; i < PlanetDatas.Length; i++)
        {
            if (PlanetDatas[i].PlanetSize == size) return PlanetDatas[i];
        }


#if DEVELOPMENT_BUILD || UNITY_EDITOR
        Debug.LogError("Missing PlanetData");
#endif

        return new PlanetData();
    }
}
