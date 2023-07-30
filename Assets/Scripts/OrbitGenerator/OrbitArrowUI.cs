using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class OrbitArrowUI : MonoBehaviour
{
    [Header("Developer")]
    [SerializeField]
    Sprite SmallIcon;
    [SerializeField]
    Sprite MediumIcon;
    [SerializeField]
    Sprite BigIcon;
    [SerializeField]
    float SmallScale;
    [SerializeField]
    float MediumScale;
    [SerializeField]
    float BigScale;

    [Header("Reference")]
    public Image PlanetIcon;

    #region Properties

    public RectTransform MyRect { get; private set; }
    CanvasGroup _MyCanvasGroup;
    Planet _AssignedPlanet;
    Coroutine MoveRoutine;
    Canvas _MyCanvas;

    #endregion

    private void Awake()
    {
        MyRect = GetComponent<RectTransform>();
        _MyCanvasGroup = GetComponent<CanvasGroup>();
        _MyCanvas = GetComponent<Canvas>();
    }

    public void UpdateUI(Planet assignedPlanet)
    {
        gameObject.SetActive(true);
        _AssignedPlanet = assignedPlanet;

        if (MoveRoutine != null) StopCoroutine(MoveRoutine);
        MoveRoutine = StartCoroutine(MoveUI());

        UpdateIcon();
    }

    IEnumerator MoveUI()
    {
        while(true)
        {
            Vector2 _direction = _AssignedPlanet.transform.position.XY() - OrbitGenerator.Instance.Player.transform.position.XY();
            float _angle = Vector2.Angle(OrbitGenerator.Instance.Player.transform.up, _direction);
            _MyCanvasGroup.BlocksAndVisible(_angle < 90);

            if (_angle < 90) AlignOnCompassLine(_angle);
            
            yield return null;
        }
        
    }

    void AlignOnCompassLine(float angle)
    {
        float _compassPositionX = 0;

        Vector2 _forwardDirection = OrbitGenerator.Instance.Player.transform.InverseTransformPoint(_AssignedPlanet.transform.position);

        if (_forwardDirection.x < 0)
        {
            _compassPositionX = Mathf.Lerp(0, (-OrbitGenerator.Instance.UI.LineRect.rect.width + 40) / 2f , angle / 90f);
        }
        else if (_forwardDirection.x >= 0)
        {
            _compassPositionX = Mathf.Lerp(0, (OrbitGenerator.Instance.UI.LineRect.rect.width - 40) / 2f , angle / 90f);
        }

        MyRect.anchoredPosition = Vector2.right * _compassPositionX;
    }

    void UpdateIcon()
    {
        Vector3 _newScale = Vector3.one;
        int _newSort = 1;

        switch (_AssignedPlanet.Size)
        {
            case PlanetSize.SMALL:
                PlanetIcon.sprite = SmallIcon;
                _newScale = Vector3.one * SmallScale;
                _newSort = 3;
                break;
            case PlanetSize.MEDIUM:
                PlanetIcon.sprite = MediumIcon;
                _newScale = Vector3.one * MediumScale;
                _newSort = 2;
                break;
            case PlanetSize.BIG:
                PlanetIcon.sprite = BigIcon;
                _newScale = Vector3.one * BigScale;
                _newSort = 1;
                break;
        }

        PlanetIcon.transform.localScale = _newScale;
        _MyCanvas.sortingOrder = _newSort;
        _MyCanvasGroup.alpha = 1;
    }

    public void StopUpdate()
    {
        _AssignedPlanet = null;
        if(MoveRoutine != null) StopCoroutine(MoveRoutine);
        _MyCanvasGroup.alpha = 0;
        gameObject.SetActive(false);
    }
}
