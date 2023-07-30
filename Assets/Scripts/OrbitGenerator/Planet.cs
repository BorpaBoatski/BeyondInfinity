using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour
{
    [Header("Developer")]
    public PlanetSize Size;

    [Header("References")]
    public SpriteRenderer MyRenderer;
    public GameObject OuterRings;

    #region Properties

    public PlanetDetails MyDetails { get; private set; }

    #endregion


    public Sprite GetSprite() { return MyRenderer.sprite; }

    public void AssignPlanetSprite(PlanetDetails details)
    {
        MyDetails = details;
        MyRenderer.sprite = details.PlanetSprite;
    }

    public void ToggleRings(bool state)
    {
        OuterRings.SetActive(state);
    }
}
