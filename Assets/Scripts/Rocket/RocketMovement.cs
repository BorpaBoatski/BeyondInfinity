using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class RocketMovement : MonoBehaviour
{
    public float ShakeIntensity = 1.0f;
    public float ShakeFrequency = 1.0f;

    public float MaxFuelGauge = 60f;
    public float TotalSlowDuration = 3.0f;
    public float LaunchSpeed = 100f;
    public float RocketAngleRange = 30f;
    public float RocketSteerRange = 45f;
    public float RotationOffset = 90f;
    public float MinLaunchDistance = 1f;
    public float MaxLaunchDistance = 3f;
    public float SteeringRotationSpeed = 0.1f;
    public Transform LaunchIndicator;
    public Sprite IdleRocketSprite;
    [SerializeField]
    float _LandingYOffset;

    [Header("References")]
    public Transform ModelRoot;
    public Gradient ColorGradient;
    public Image FuelBar;
    public SpriteRenderer MyRenderer;
    public GameObject DragInstruction;
    public SteeringUI SteeringUI;
    public StarSpawner StarSpawner;
    public GameObject ReleaseInstruction;

    [HideInInspector]
    public float _CurrentFuelGauge;
    Vector2 _CurrentTouchPos;

    #region Properties

    bool _IsHoldingDown, _IsLaunching;
    Vector2 _InitialTouchPos;
    float _LaunchStrength;

    BoxCollider2D _MyCollider;
    public bool CanReceiveInput = false;
    int _LaunchSFXStage = 0;
    bool _IsFuelDepleted;
    Animator _Animator;
    WaitForSeconds _ColliderDelay;

    #endregion

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawIcon(transform.position + (Vector3.up * _LandingYOffset), "Landing");
    }

    private void Awake()
    {
        _MyCollider = GetComponent<BoxCollider2D>();
        _Animator = ModelRoot.GetComponentInChildren<Animator>();
        _ColliderDelay = new WaitForSeconds(.1f);
    }

    private void Start()
    {
        _CurrentFuelGauge = MaxFuelGauge;
    }

    void Update()
    {
        if (_IsLaunching)
        {
            //Debug.Log(_LaunchStrength);
            if(_LaunchStrength <= 0)
            {
                _IsLaunching = false;
                SteeringUI.ToggleSteeringUI(false);
                StarSpawner.ToggleStarStretch(false);
            }
            else
            {
                _LaunchSFXStage = 0;
                transform.position += transform.up * Time.deltaTime * _LaunchStrength;
                ConsumeFuel();
                ResetTouchPositions();
                SteeringRotation();
            }
        }
        else
        {
            if(!CanReceiveInput)
            {
                ResetTouchPositions();
                return;
            }

            if (Input.GetKey(KeyCode.R))
            {
                _IsLaunching = false;
            }
#if UNITY_EDITOR
            if (!_IsHoldingDown)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    GameManager.Instance.PlanetDetailsCanvas.Close();

                    if (DragInstruction.activeSelf) 
                    {
                        DragInstruction.SetActive(false);
                        ReleaseInstruction.SetActive(true);
                    }

                    _InitialTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    _CurrentTouchPos = _InitialTouchPos;
                    _IsHoldingDown = true;
                }
            }
            else
            {
                Vector2 directionVector = _InitialTouchPos - _CurrentTouchPos;

                if (Input.GetMouseButtonUp(0))
                {
                    _IsHoldingDown = false;
                    LaunchIndicator.gameObject.SetActive(false);

                    if (directionVector.y <= 0) return;

                    float _distance = Mathf.Min(Vector2.Distance(_InitialTouchPos, _CurrentTouchPos), MaxLaunchDistance);
                    if (_distance >= MinLaunchDistance)
                    {
                        BeginLaunch(_distance);
                    }
                }
                else
                {
                    _CurrentTouchPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

                    if (_CurrentTouchPos == _InitialTouchPos || directionVector.y <= 0) return;

                        float _angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
                        float _allowedAngle = Mathf.Clamp(_angle - RotationOffset, -RocketAngleRange, RocketAngleRange);
                        transform.rotation = Quaternion.Euler(0, 0, _allowedAngle);
                        Debug.DrawLine(_InitialTouchPos, _CurrentTouchPos, Color.red);
                        DrawLaunchIndicator(_allowedAngle);
                    }
                }
#endif
            if (Input.touchCount > 0)
            {
                Touch _touchInput = Input.GetTouch(0);
                GameManager.Instance.PlanetDetailsCanvas.Close();

                if (!_IsHoldingDown)
                {
                    if (_touchInput.phase == TouchPhase.Began)
                    {
                        if (DragInstruction.activeSelf)
                        {
                            DragInstruction.SetActive(false);
                            ReleaseInstruction.SetActive(true);
                        }

                        _InitialTouchPos = Camera.main.ScreenToWorldPoint(_touchInput.position);
                        _CurrentTouchPos = _InitialTouchPos;
                        _IsHoldingDown = true;
                    } 
                }
                else
                {
                    
                    Vector2 directionVector = _InitialTouchPos - _CurrentTouchPos;

                    if (_touchInput.phase == TouchPhase.Ended)
                    {
                        _IsHoldingDown = false;
                        LaunchIndicator.gameObject.SetActive(false);

                        if (directionVector.y <= 0) return;

                        float _distance = Mathf.Min(Vector2.Distance(_InitialTouchPos, _CurrentTouchPos), MaxLaunchDistance);
                        if (_distance >= MinLaunchDistance)
                        {
                            BeginLaunch(_distance);
                        }
                    }
                    else
                    {
                        _CurrentTouchPos = Camera.main.ScreenToWorldPoint(_touchInput.position);

                        if (_CurrentTouchPos == _InitialTouchPos || directionVector.y <= 0) return;

                            float _angle = Mathf.Atan2(directionVector.y, directionVector.x) * Mathf.Rad2Deg;
                            float _allowedAngle = Mathf.Clamp(_angle - RotationOffset, -RocketAngleRange, RocketAngleRange);
                            transform.rotation = Quaternion.Euler(0, 0, _allowedAngle);
                            Debug.DrawLine(_InitialTouchPos, _CurrentTouchPos, Color.red);
                            DrawLaunchIndicator(_allowedAngle);
                        }
                    }
                }
            }
        }

    void DrawLaunchIndicator(float angle)
    {
        float _distance = Mathf.Min(Vector2.Distance(_InitialTouchPos, _CurrentTouchPos), MaxLaunchDistance);

        if (_distance < MinLaunchDistance)
        {
            _LaunchSFXStage = 0;
            LaunchIndicator.gameObject.SetActive(false);
            return;
        }

        PlayLaunchMeterSound(_distance / MaxLaunchDistance);
        LaunchIndicator.localScale = new Vector2(0.08f, 0.05f * (_distance + 1));
        LaunchIndicator.GetComponentInChildren<SpriteRenderer>().color = ColorGradient.Evaluate(_distance / MaxLaunchDistance);


        LaunchIndicator.gameObject.SetActive(true);
    }
    
    void PlayLaunchMeterSound(float level)
    {
        if (_LaunchSFXStage != 1 && level < 0.5f)
        {
            _LaunchSFXStage = 1;
            AudioManager.Instance.PlaySFX("Launch-Low");
        }
        else if (_LaunchSFXStage != 2 && (level >= 0.5f && level != 1))
        {
            _LaunchSFXStage = 2;
            AudioManager.Instance.PlaySFX("Launch-Mid");
        }
        else if (_LaunchSFXStage != 3 && level >= 1)
        {
            _LaunchSFXStage = 3;
            AudioManager.Instance.PlaySFX("Launch-High");
        }
    }

    void SteeringRotation()
    {
        float _currentAngle = transform.eulerAngles.z;
        if (_currentAngle > 180) _currentAngle -= 360;

        bool steerLeft = false;
        bool steerRight = false;
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            if (Input.mousePosition.x < Screen.width / 2.0f && _currentAngle < RocketSteerRange)
            {
                steerLeft = true;
            }
            else if (Input.mousePosition.x > Screen.width / 2.0f && _currentAngle > -RocketSteerRange)
            {
                steerRight = true;
            }
        }
#endif
        foreach (Touch touchInput in Input.touches)
        {
            if (touchInput.position.x < Screen.width / 2.0f && _currentAngle < RocketSteerRange)
            {
                steerLeft = true;
            }
            else if (touchInput.position.x > Screen.width / 2.0f && _currentAngle > -RocketSteerRange)
            {
                steerRight = true;
            }
        }

        float direction = (steerLeft ? 1 : 0) - (steerRight ? 1 : 0);

        if (direction != 0)
        {
            ConsumeFuel();
            //AudioManager.Instance.PlaySFX("Steering");
            transform.Rotate(Vector3.forward * direction * SteeringRotationSpeed);
        }
    }

    public void Land(Planet collidedPlanet)
    {
        AudioManager.Instance.StopSFX("Moving");
        _IsLaunching = false;
        ToggleLaunchingAnimation(false);
        _MyCollider.enabled = false;
        RefillFuel();
        LandingSequence(collidedPlanet);
        collidedPlanet.ToggleRings(false);
        OrbitGenerator.Instance.ClearOldPlanets(collidedPlanet);
        GameManager.Instance.IncrementScore(collidedPlanet);
        SteeringUI.ToggleSteeringUI(false);
        StarSpawner.ToggleStarStretch(false);
    }

    void LandingSequence(Planet collidedPlanet)
    {
        float _takeOffOffset = collidedPlanet.MyRenderer.bounds.extents.y + MyRenderer.bounds.extents.y + _LandingYOffset;
        CanReceiveInput = false;
        Vector2 _direction = collidedPlanet.transform.position - transform.position;
        Sequence _landingSequence = DOTween.Sequence();
        _landingSequence.Append(transform.DOMove(collidedPlanet.transform.position, .5f));
        _landingSequence.Join(transform.DORotate(_direction, .5f));
        _landingSequence.Join(ModelRoot.DOScale(Vector3.zero, .5f));
        _landingSequence.AppendCallback(() =>
        {
            transform.rotation = Quaternion.identity;
            GameManager.Instance.PlanetDetailsCanvas.InitializeUI(collidedPlanet);

        });
        _landingSequence.Insert(.6f, ModelRoot.DOScale(Vector3.one, .5f));
        _landingSequence.Join(transform.DOMove(collidedPlanet.transform.position + Vector3.up * _takeOffOffset, .5f));
        _landingSequence.OnComplete(() =>
        {
            CanReceiveInput = true;
            OrbitGenerator.Instance.UpdateUndiscoveredPlanets(collidedPlanet);
            OrbitGenerator.Instance.StartGeneratingOrbit(collidedPlanet);
        });
    }

    void ConsumeFuel()
    {
        if (_IsFuelDepleted) return;

        if (_CurrentFuelGauge <= 0)
        {
            _IsFuelDepleted = true;
            _CurrentFuelGauge = 0;
            ToggleLaunchingAnimation(false);
            AudioManager.Instance.StopSFX("Moving");
            DOVirtual.Float(_LaunchStrength, 0, TotalSlowDuration, value => _LaunchStrength = value).OnComplete(() => RocketFullStop());
        }
        else
        {
            _CurrentFuelGauge = Mathf.Max(_CurrentFuelGauge - Time.deltaTime, 0);
            FuelBar.fillAmount = GetFuelPercentage();
        }
    }

    void RocketFullStop()
    {
        CanReceiveInput = false;
        GameManager.Instance.GameOver();
    }

    void RefillFuel()
    {
        _CurrentFuelGauge = MaxFuelGauge;
        _IsFuelDepleted = false;
        FuelBar.fillAmount = GetFuelPercentage();
    }

    public float GetFuelPercentage()
    {
        //Debug.Log(_CurrentFuelGauge / MaxFuelGauge);
        return _CurrentFuelGauge / MaxFuelGauge;
    }

    void ResetTouchPositions()
    {
        _CurrentTouchPos = Vector2.zero;
        _InitialTouchPos = Vector2.zero;
    }

    IEnumerator RocketShakeInterval()
    {
        while (_IsLaunching)
        {
            ModelRoot.DOShakePosition(ShakeFrequency, new Vector3(ShakeIntensity, ShakeIntensity, 0), 10, 90);
            yield return new WaitForSeconds(ShakeFrequency);
        }
    }

    void ToggleLaunchingAnimation(bool state)
    {
        if (state)
        {
            _Animator.enabled = true;
        }
        else
        {
            _Animator.enabled = false;
            ModelRoot.GetComponentInChildren<SpriteRenderer>().sprite = IdleRocketSprite;
        }
    }

    void BeginLaunch(float _pullDistance)
    {
        if (ReleaseInstruction.activeSelf) ReleaseInstruction.SetActive(false);

        _LaunchStrength = LaunchSpeed * (_pullDistance / MaxLaunchDistance);
        _IsLaunching = true;
        ToggleLaunchingAnimation(true);
        StartCoroutine(RocketShakeInterval());
        AudioManager.Instance.PlaySFX("Launch");
        AudioManager.Instance.PlaySFX("Moving");
        SteeringUI.ToggleSteeringUI(true);
        StarSpawner.ToggleStarStretch(true);
        StartCoroutine(ReenableCollider());
    }

    IEnumerator ReenableCollider()
    {
        yield return _ColliderDelay;
        _MyCollider.enabled = true;
    }
}
