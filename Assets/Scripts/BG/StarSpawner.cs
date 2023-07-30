using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StarSpawner : MonoBehaviour
{
    public Transform Player;
    public GameObject[] Stars;
    public GameObject[] BGStars;
    public Sprite[] StarSprites;
    public float HideStarInterval;
    public float BGStarInterval;
    public float SpawnDistance;
    public float SpawnHorizontalOffset;
    public float MoveSpeed = 5.0f;
    public float BGStarSpeedMultiplier = 0.3f;
    [SerializeField]
    GameObject TheComet;
    [SerializeField]
    SpriteRenderer TheCometRenderer;
        
    List<GameObject> HiddenStars = new List<GameObject>();
    List<GameObject> HiddenBGStars = new List<GameObject>();
    bool _IsRocketLaunching;
    WaitForSeconds _CometWait;

    private void Awake()
    {
        _CometWait = new WaitForSeconds(5);
    }

    private void Start()
    {
        for (int i = 0; i < BGStars.Length; i++)
        {
            BGStars[i].SetActive(false);
            HiddenBGStars.Add(BGStars[i]);
        }

        StartCoroutine(CheckForStarsOutOfRange());
        StartCoroutine(CheckForBGStarsOutOfRange());
        StartCoroutine(SpawnTheComet());
    }

    private void Update()
    {
        if (_IsRocketLaunching)
        {
            RotateStars();
            MoveStars();
        }
    }

    void RandomizeStars()
    {
        int SpawnLimit = Random.Range(1, 3);
        for (int i = 0; i < BGStars.Length; i++)
        {
            if (i < SpawnLimit)
            {
                BGStars[i].SetActive(true);
                InitialSpawnLocation(BGStars[i]);
            }
            else
            {
                BGStars[i].SetActive(false);
                HiddenBGStars.Add(BGStars[i]);
            }
        }
    }

    void RotateStars()
    {
        foreach (GameObject star in Stars)
        {
            if (HiddenStars.Contains(star)) continue;

            star.transform.rotation = Player.rotation;
        }
    }

    IEnumerator CheckForStarsOutOfRange()
    {
        while (enabled)
        {
            if (_IsRocketLaunching)
            {
                foreach (GameObject star in Stars)
                {
                    if (star.transform.position.y < Player.position.y - 10.0f)
                    {
                        if (!HiddenStars.Contains(star))
                        {
                            HiddenStars.Add(star);
                            star.SetActive(false);
                            continue;
                        }
                    }
                }

                if (HiddenStars.Count > 0)
                {
                    for (int i = 0; i < Random.Range(1, HiddenStars.Count); i++)
                    {
                        ChangeStarLocation(HiddenStars[i]);
                    }
                }
            }

            yield return new WaitForSeconds(HideStarInterval);
        }
    }

    IEnumerator CheckForBGStarsOutOfRange()
    {
        while (enabled)
        {
            foreach (GameObject star in BGStars)
            {
                if (star.transform.position.y < Player.position.y - 10.0f)
                {
                    if (!HiddenBGStars.Contains(star))
                    {
                        HiddenBGStars.Add(star);
                        star.SetActive(false);
                        continue;
                    }
                }
            }

            if (HiddenBGStars.Count > 0)
            {
                for (int i = 0; i < Random.Range(1, HiddenBGStars.Count); i++)
                {
                    ChangeStarLocation(HiddenBGStars[i], false);
                }
            }

            yield return new WaitForSeconds(BGStarInterval);
        }
    }

    IEnumerator SpawnTheComet()
    {
        while(true)
        {
            if (TheComet.gameObject.activeSelf)
            {
                if (!TheCometRenderer.isVisible)
                {
                    TheComet.gameObject.SetActive(false);
                }

                TheComet.transform.position += TheComet.transform.up * Time.deltaTime;
            }
            else
            {
                if (_IsRocketLaunching)
                {
                    yield return _CometWait;

                    int _spawnChance = UnityEngine.Random.Range(0, 100);

                    if (_spawnChance <= 10)
                    {
                        Vector3 _newLocation = Player.position + (Player.up * (SpawnDistance + SpawnDistance * Random.Range(0.5f, 2.0f))) + (Player.right * Random.Range(-SpawnHorizontalOffset, SpawnHorizontalOffset));
                        TheComet.gameObject.SetActive(true);
                        TheComet.transform.position = _newLocation;
                        Vector3 _direction = Player.position - TheComet.transform.position;
                        TheComet.transform.up = _direction;
                    }
                }
            }

            yield return null;
        }
    }

    void InitialSpawnLocation(GameObject star)
    {
        Vector3 _spawnPos = Player.position + (Player.up * Random.Range(5.0f, 10.0f) + (Player.right * Random.Range(-SpawnHorizontalOffset, SpawnHorizontalOffset)));
        star.transform.position = _spawnPos;
        star.GetComponent<SpriteRenderer>().sprite = StarSprites[Random.Range(0, StarSprites.Length - 1)];
    }

    void ChangeStarLocation(GameObject star, bool isFastStar = true)
    {
        Vector3 _newLocation = Player.position + (Player.up * (SpawnDistance + SpawnDistance * Random.Range(0.5f, 2.0f))) + (Player.right * Random.Range(-SpawnHorizontalOffset, SpawnHorizontalOffset));
        star.transform.position = _newLocation;
        if (isFastStar)
        {
            HiddenStars.Remove(star);
            int count = _IsRocketLaunching ? Random.Range(0, 2) : Random.Range(0, StarSprites.Length);
            star.GetComponent<SpriteRenderer>().sprite = StarSprites[count];
            if (_IsRocketLaunching)
            {
                star.transform.DOScale(new Vector2(0.5f, 3.0f), 1);
            }
        }
        else
        {
            HiddenBGStars.Remove(star);
            star.GetComponent<SpriteRenderer>().sprite = StarSprites[Random.Range(2, StarSprites.Length)];
        }
        
        star.SetActive(true);
    }

    public void ToggleStarStretch(bool state)
    {
        _IsRocketLaunching = state;

        foreach (GameObject star in Stars)
        {
            if (HiddenStars.Contains(star)) continue;

            if (state)
            {
                star.transform.DOScale(new Vector2(0.5f, 3.0f), 1);
            }
            else
            {
                star.transform.DOScale(new Vector2(1, 1), 1);
                star.transform.DORotate(Vector2.zero, 0.5f);
            }
        }
    }

    void MoveStars()
    {
        foreach (GameObject star in Stars)
        {
            if (HiddenStars.Contains(star)) continue;

            star.transform.position -= star.transform.up * Time.deltaTime * MoveSpeed;
        }

        foreach (GameObject bgStar in BGStars)
        {
            bgStar.transform.position -= Player.transform.up * Time.deltaTime * MoveSpeed * BGStarSpeedMultiplier;
        }
    }
}
