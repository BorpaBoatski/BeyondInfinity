using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

class SpawnedPlanet
{
    public SpawnedPlanet(PlanetSize size, float angle)
    {
        PlanetSize = size;
        Angle = angle;
    }

    public PlanetSize PlanetSize;
    public float Angle;
}

public class OrbitGenerator : MonoBehaviour
{
    public static OrbitGenerator Instance;

    [Header("References")]
    [SerializeField]
    Transform SmallPlanetsHolder;
    [SerializeField]
    Transform MediumPlanetsHolder;
    [SerializeField]
    Transform BigPlanetsHolder;
    [SerializeField]
    PlanetSpawningData PlanetSpawningData;
    //[SerializeField]
    //Sprite[] PlanetSprites;
    public RocketMovement Player;
    public OrbitUI UI;

    [Header("Spawning")]
    [SerializeField]
    GameObject BigPlanet;
    [SerializeField]
    GameObject MediumPlanet;
    [SerializeField]
    GameObject SmallPlanet;

    [Header("Developer")]
    [SerializeField]
    float _Radius;
    [SerializeField]
    string AddressableName;
    [SerializeField]
    int _PlanetsToSpawn;
    [SerializeField]
    float _MaxOrbitSpeed;
    [SerializeField]
    float _OrbitSpeedIncrement;
    [SerializeField]
    int _MinScoreForOrbit;
    [SerializeField]
    float _ScoreModifierPerOrbitPass;
    [SerializeField]
    float _PlanetsIncrement;
    [SerializeField]
    float _DistanceToNextOrbitSpawn;
    [SerializeField]
    int _MaxPlanets;

    #region Properties

    List<GameObject> _Planets;
    Color _GizmosColor = new Color();
    Planet BigPlanetData;
    Planet MediumPlanetData;
    Planet SmallPlanetData;
    [HideInInspector]
    public List<Planet> ActivePlanets = new List<Planet>();
    public delegate void PlanetChanging();
    public PlanetChanging OnPlanetsChange;
    Planet _PlanetToSave;
    Coroutine _SpawningRoutine;
    WaitForFixedUpdate _SpawnWait;
    List<PlanetDetails> _UndiscoveredPlanets = new List<PlanetDetails>();
    float _OrbitSpeed;
    Coroutine _OrbitMovementRoutine;
    Vector3 _PlayerInitialPosition;
    Coroutine ObservePlayerRoutine;
    public bool GenerationComplete { get; private set; } 

    #endregion 

    private void OnDrawGizmosSelected()
    {
        if (Player == null) return;

        Gizmos.DrawWireSphere(Player.transform.position, _Radius);
    }

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
            return;
        }

        Instance = this;
        SmallPlanetData = SmallPlanet.GetComponent<Planet>();
        MediumPlanetData = MediumPlanet.GetComponent<Planet>();
        BigPlanetData = BigPlanet.GetComponent<Planet>();
        _UndiscoveredPlanets = PlanetSpawningData.PlanetDetails.ToList();
        _SpawnWait = new WaitForFixedUpdate();
    }

    private void Start()
    {
        StartGeneratingOrbit();
        GameManager.Instance.OnScoreChange += IncreasePlanetSpawn;
    }

    public void StartGeneratingOrbit(Planet collidedPlanet = null)
    {
        if (_SpawningRoutine != null) StopCoroutine(_SpawningRoutine);
        _SpawningRoutine = StartCoroutine(GenerateOrbit(collidedPlanet));
    }

    public IEnumerator GenerateOrbit(Planet _planetToSave = null)
    {
        GenerationComplete = false;
        ClearOldPlanets(_planetToSave);
        _PlayerInitialPosition = Player.transform.position;
        int _repeatedCreationAttempts = 0;
        UI.ToggleVisualization(true);
        PlanetSize _randomPlanetSize;
        float _randomAngle;
        SpawnedPlanet _newPlanet;

        do
        {
            _randomPlanetSize = (PlanetSize)UnityEngine.Random.Range(0, (int)Enum.GetValues(typeof(PlanetSize)).Cast<PlanetSize>().Max());
            _randomAngle = UnityEngine.Random.Range(0, 359);

            /// There must be a planet the player can aim for
            if(ActivePlanets.Count < 3)
            {
                _randomAngle = UnityEngine.Random.Range(0, Player.RocketAngleRange * 2);
                _randomAngle -= Player.RocketAngleRange;
            }

            ///We will always have the 3 planet sizes available
            if (ActivePlanets.Count < 3)
            {
                switch (ActivePlanets.Count)
                {
                    case 0:
                        _randomPlanetSize = PlanetSize.SMALL;
                        break;
                    case 1:
                        _randomPlanetSize = PlanetSize.MEDIUM;
                        break;
                    case 2:
                        _randomPlanetSize = PlanetSize.BIG;
                        break;
                }
            }

            _newPlanet = new SpawnedPlanet(_randomPlanetSize, _randomAngle);

            //Debug.Break();

            if (PlanetHasEnoughSpace(_randomAngle, _randomPlanetSize, ActivePlanets))
            {
                _repeatedCreationAttempts = 0;
                ActivePlanets.Add(SpawnPlanet(_newPlanet).GetComponent<Planet>());
            }
            else _repeatedCreationAttempts++;

            if(_repeatedCreationAttempts >= 5)
            {
                //FillInGaps(ref Planets);
                GenerationComplete = true;
            }

            ///To adhere to planet limit
            if (_PlanetsToSpawn > 0 && ActivePlanets.Count >= _PlanetsToSpawn) GenerationComplete = true;
            yield return _SpawnWait;

        } while (!GenerationComplete);

        OnPlanetsChange?.Invoke();

        if (ObservePlayerRoutine != null) StopCoroutine(ObservePlayerRoutine);
        ObservePlayerRoutine = StartCoroutine(ObservePlayerPosition());

        if (GameManager.Instance.Score <= _MinScoreForOrbit) yield break;

        //_PlanetsToSpawn = -1;

        if (_OrbitMovementRoutine != null) StopCoroutine(_OrbitMovementRoutine);
        _OrbitMovementRoutine = StartCoroutine(OrbitMovement());
        //SpawnOrbit(Planets);
    }

    GameObject SpawnPlanet(SpawnedPlanet planet)
    {
        GameObject _newPlanet = ReturnUnusedPlanet(planet.PlanetSize);

        if(_newPlanet == null)
        {
            switch (planet.PlanetSize)
            {
                case PlanetSize.SMALL:
                    _newPlanet = Instantiate(SmallPlanet, SmallPlanetsHolder);
                    break;
                case PlanetSize.MEDIUM:
                    _newPlanet = Instantiate(MediumPlanet, MediumPlanetsHolder);
                    break;
                case PlanetSize.BIG:
                    _newPlanet = Instantiate(BigPlanet, BigPlanetsHolder);
                    break;
            }
        }

        _newPlanet.transform.position = GeneratePlanetLocation(planet.Angle);
        Planet _planet = _newPlanet.GetComponent<Planet>();
        _planet.AssignPlanetSprite(GetRandomPlanetDetail());
        _planet.ToggleRings(true);
        _newPlanet.SetActive(true);
        return _newPlanet;
    }

    GameObject ReturnUnusedPlanet(PlanetSize size)
    {
        Transform _holderToCheck = null;

        switch (size)
        {
            case PlanetSize.SMALL:
                _holderToCheck = SmallPlanetsHolder;
                break;
            case PlanetSize.MEDIUM:
                _holderToCheck = MediumPlanetsHolder;
                break;
            case PlanetSize.BIG:
                _holderToCheck = BigPlanetsHolder;
                break;
        }


        for (int i = 0; i < _holderToCheck.childCount; i++)
        {
            if (!_holderToCheck.GetChild(i).gameObject.activeSelf) return _holderToCheck.GetChild(i).gameObject;
        }

        return null;
    }

    //void SpawnOrbit(List<SpawnedPlanet> planets)
    //{
    //    for (int i = 0; i < planets.Count; i++)
    //    {
    //        GameObject _newPlanet = null;

    //        switch(planets[i].PlanetSize)
    //        {
    //            case PlanetSize.SMALL:
    //                _newPlanet = Instantiate(SmallPlanet);
    //                break;
    //            case PlanetSize.MEDIUM:
    //                _newPlanet = Instantiate(MediumPlanet);
    //                break;
    //            case PlanetSize.BIG:
    //                _newPlanet = Instantiate(BigPlanet);
    //                break;
    //        }

    //        _newPlanet.transform.position = GeneratePlanetLocation(planets[i].Angle);
    //        _newPlanet.transform.parent = PlanetHolder;
    //    }
    //}

    Vector2 GeneratePlanetLocation(float angle, PlanetSize size = PlanetSize.SMALL)
    {
        Vector2 _planetLocation = Vector2.zero;

        Vector2 _direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;

        //Color _debugColor = Color.red;

        //switch(size)
        //{
        //    case PlanetSize.SMALL:
        //        _debugColor = Color.yellow;
        //        break;
        //    case PlanetSize.MEDIUM:
        //        _debugColor = Color.green;
        //        break;
        //    case PlanetSize.BIG:
        //        _debugColor = Color.black;
        //        break;
        //}

        //Debug.DrawRay(Player.transform.position, _direction * 100, _debugColor, 2);

        _planetLocation = Player.transform.position.XY() + (_direction.normalized * _Radius);

        return _planetLocation;
    }

    bool PlanetHasEnoughSpace(float planetToSpawnAngle, PlanetSize size, List<Planet> confirmedPlanets)
    {
        Vector2 _planetLocation = GeneratePlanetLocation(planetToSpawnAngle, size);
        Planet _spawningPlanetData = null;

        switch (size)
        {
            case PlanetSize.SMALL:
                _spawningPlanetData = SmallPlanetData;
                break;
            case PlanetSize.MEDIUM:
                _spawningPlanetData = MediumPlanetData;
                break;
            case PlanetSize.BIG:
                _spawningPlanetData = BigPlanetData;
                break;
        }

        Bounds _testingBounds = _spawningPlanetData.MyRenderer.bounds;
        _testingBounds.center = _planetLocation;
        //DebugDrawBounds(_testingBounds, size);

        for (int i = 0; i < confirmedPlanets.Count; i++)
        {
            if (_testingBounds.Intersects(confirmedPlanets[i].MyRenderer.bounds)) return false;

            Vector2 _closestPointOnOtherPlanetBounds = confirmedPlanets[i].MyRenderer.bounds.ClosestPoint(_planetLocation);
            Vector2 _closestPointOnThisPlanetBounds = _testingBounds.ClosestPoint(confirmedPlanets[i].transform.position);


            bool WithinThisPlanetMinDistance = Vector2.Distance(_closestPointOnOtherPlanetBounds, _closestPointOnThisPlanetBounds) <= PlanetSpawningData.FindPlanetData(size).RequiredDistance;
            bool WithinOtherPlanetMinDistance = Vector2.Distance(_closestPointOnOtherPlanetBounds, _closestPointOnThisPlanetBounds) <= PlanetSpawningData.FindPlanetData(confirmedPlanets[i].Size).RequiredDistance;

            //Debug.DrawLine(_closestPointOnOtherPlanetBounds, _closestPointOnThisPlanetBounds,
            //    (WithinThisPlanetMinDistance || WithinOtherPlanetMinDistance ? Color.red : Color.magenta), 2);

            if (WithinOtherPlanetMinDistance || WithinThisPlanetMinDistance) return false;

            //Ray _collisionTestRay = new Ray(_planetLocation, Vector3.forward);
            //bool _collided = false;

            //if (_collided) return false;



            ///Check if new spawning angle falls within another planets safe zone
            //bool _withinMinRange = planetToSpawnAngle >= (confirmedPlanets[i].Angle - PlanetSpawningData.FindPlanetData(confirmedPlanets[i].PlanetSize).MinAngleToNextPlanet) % 360;
            //bool _withinMaxRange = planetToSpawnAngle <= (confirmedPlanets[i].Angle + PlanetSpawningData.FindPlanetData(confirmedPlanets[i].PlanetSize).MinAngleToNextPlanet) % 360;
            //if (_withinMaxRange && _withinMinRange) return false;
        }

        return true;
    }

    public void ClearOldPlanets(Planet planetToSave)
    {
        if (ActivePlanets.Count <= 0) return;

        if (_PlanetToSave != null)
        {
            _PlanetToSave.gameObject.SetActive(false);
            _PlanetToSave.transform.SetParent(FindRespectiveHolder(_PlanetToSave));
        }

        if(planetToSave != null)
        {
            _PlanetToSave = planetToSave;
            _PlanetToSave.transform.SetParent(null);
        }

        for (int i = ActivePlanets.Count - 1; i >= 0; i--)
        {
            ActivePlanets[i].gameObject.SetActive(ActivePlanets[i] == planetToSave);
            ActivePlanets.RemoveAt(i);
        }

        transform.localEulerAngles = Vector3.zero;
        UI.ToggleVisualization(false);
        if (ObservePlayerRoutine != null) StopCoroutine(ObservePlayerRoutine);

    }

    public void UpdateUndiscoveredPlanets(Planet discoveredPlanet)
    {
        if (_UndiscoveredPlanets.Count == 0) return;

        PlanetDetails _planetSprite = discoveredPlanet.MyDetails;
        if (_UndiscoveredPlanets.Contains(_planetSprite))
        {
            _UndiscoveredPlanets.Remove(_planetSprite);
        }
    }

    public bool IsUndiscoveredPlanet(Planet targetPlanet)
    {
        return _UndiscoveredPlanets.Contains(targetPlanet.MyDetails);
    }

    void FillInGaps(ref List<Planet> confirmedPlanets)
    {

    }

    void DebugDrawBounds(Bounds bounds, PlanetSize size)
    {
        Color _debugColor = Color.magenta;

        switch(size)
        {
            case PlanetSize.SMALL:
                _debugColor = Color.yellow;
                break;
            case PlanetSize.MEDIUM:
                _debugColor = Color.green;
                break;
            case PlanetSize.BIG:
                _debugColor = Color.black;
                break;
        }

        Vector2 TopLeft = bounds.center + new Vector3(-bounds.extents.x, bounds.extents.y);
        Vector2 TopRight = bounds.center + bounds.extents;
        Vector2 BottomLeft = bounds.center - bounds.extents;
        Vector2 BottomRight = bounds.center + new Vector3(bounds.extents.x, -bounds.extents.y);

        Debug.DrawLine(TopLeft, TopRight, _debugColor);
        Debug.DrawLine(TopLeft, BottomLeft, _debugColor);
        Debug.DrawLine(BottomRight, BottomLeft, _debugColor);
        Debug.DrawLine(BottomRight, TopRight, _debugColor);

    }

    Transform FindRespectiveHolder(Planet planet)
    {
        switch(planet.Size)
        {
            case PlanetSize.SMALL:
                return SmallPlanetsHolder;
            case PlanetSize.MEDIUM:
                return MediumPlanetsHolder;
            case PlanetSize.BIG:
                return BigPlanetsHolder;
        }

        return null;
    }

    IEnumerator OrbitMovement()
    {
        float _scoreDifference = 0;
        float _speedClamped = _OrbitSpeedIncrement;

        while (true)
        {
            _scoreDifference = GameManager.Instance.Score - _MinScoreForOrbit;
            _speedClamped = Mathf.Clamp(_scoreDifference * _OrbitSpeedIncrement, _OrbitSpeedIncrement, _MaxOrbitSpeed);
            transform.localEulerAngles += Vector3.forward * _speedClamped * Time.deltaTime;
            yield return null;
        }
    }

    PlanetDetails GetRandomPlanetDetail()
    {
        if(_UndiscoveredPlanets.Count > 0)
        {
            return _UndiscoveredPlanets[UnityEngine.Random.Range(0, _UndiscoveredPlanets.Count - 1)];
        }

        return PlanetSpawningData.PlanetDetails[UnityEngine.Random.Range(0, PlanetSpawningData.PlanetDetails.Length - 1)];
    }

    IEnumerator ObservePlayerPosition()
    {
        while(true)
        {
            float _distance = Vector2.Distance(_PlayerInitialPosition, Player.transform.position);

            ///Random offset
            if(_distance > _Radius + _DistanceToNextOrbitSpawn)
            {
                if (_SpawningRoutine != null) StopCoroutine(_SpawningRoutine);
                _SpawningRoutine = StartCoroutine(GenerateOrbit());
                GameManager.Instance.IncrementScoreModifier(_ScoreModifierPerOrbitPass);
            }

            yield return null;
        }
    }

    void IncreasePlanetSpawn(int value)
    {
        _PlanetsToSpawn = Mathf.Clamp(_PlanetsToSpawn + 1, 1, _MaxPlanets);
    }

    public int GetPlanetScore(PlanetSize size)
    {
        for (int i = 0; i < PlanetSpawningData.PlanetDatas.Length; i++)
        {
            if (PlanetSpawningData.PlanetDatas[i].PlanetSize != size) continue;

            return PlanetSpawningData.PlanetDatas[i].Score;
        }

        return 0;
    }

    #region Testing

    [ContextMenu("Generate Orbit")]
    public void TestOrbit()
    {
        if (_SpawningRoutine != null) StopCoroutine(_SpawningRoutine);
        _SpawningRoutine = StartCoroutine(GenerateOrbit());
    }

    #endregion
}
