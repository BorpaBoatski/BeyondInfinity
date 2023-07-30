using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitUI : MonoBehaviour
{
    [Header("Reference")]
    [SerializeField]
    List<OrbitArrowUI> Arrows = new List<OrbitArrowUI>();
    public RectTransform LineRect;

    #region Properties

    Coroutine CompassRoutine;
    public Canvas MyCanvas { get; private set; }

    #endregion

    private void Awake()
    {
        MyCanvas = GetComponent<Canvas>();
    }

    public void ToggleVisualization(bool state)
    {
        if (CompassRoutine != null) StopCoroutine(CompassRoutine);
        ToggleAllArrows(false);

        if (state) CompassRoutine = StartCoroutine(VisualizePlanets());
    }

    IEnumerator VisualizePlanets()
    {
        yield return new WaitUntil(() => OrbitGenerator.Instance.GenerationComplete);

        int _arrowDifference = OrbitGenerator.Instance.ActivePlanets.Count - Arrows.Count;

        for (int j = 0; j < _arrowDifference; j++)
        {
            CreateNewArrow();
        }

        for (int i = 0; i < OrbitGenerator.Instance.ActivePlanets.Count; i++)
        {
            Arrows[i].UpdateUI(OrbitGenerator.Instance.ActivePlanets[i]);
        }
    }

    void ToggleAllArrows(bool state)
    {
        for (int i = 0; i < Arrows.Count; i++)
        {
            if (!state) Arrows[i].StopUpdate();
            Arrows[i].gameObject.SetActive(state);
        }
    }

    void CreateNewArrow()
    {
        OrbitArrowUI _newArrow = Instantiate(Arrows[0].gameObject, Arrows[0].transform.parent).GetComponent<OrbitArrowUI>();
        Arrows.Add(_newArrow);
    }
}
