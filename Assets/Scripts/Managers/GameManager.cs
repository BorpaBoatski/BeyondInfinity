using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField]
    GameManagerUI UI;
    public PlanetDetailsCanvas PlanetDetailsCanvas;

    #region

    public int Score { get; private set; }
    public float ScoreModifier { get; private set; } = 1;
    public delegate void ScoreChange(int value);
    public ScoreChange OnScoreChange;
    public delegate void ScoreModifierChange(float value);
    public ScoreModifierChange OnScoreModifierChange;

    #endregion

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(Instance.gameObject);
            return;
        }

        Instance = this;
    }

    public void IncrementScore(Planet landedPlanet)
    {
        Score += Mathf.FloorToInt(OrbitGenerator.Instance.GetPlanetScore(landedPlanet.Size) * ScoreModifier);
        ScoreModifier = 1;
        OnScoreModifierChange?.Invoke(ScoreModifier);
        OnScoreChange?.Invoke(Score);
    }

    public void IncrementScoreModifier(float value)
    {
        ScoreModifier += value;
        OnScoreModifierChange?.Invoke(ScoreModifier);
    }

    public void GameOver()
    {
        OrbitGenerator.Instance.ClearOldPlanets(null);
        UI.DisplayGameOver();
    }

    public void StartGame()
    {
        OrbitGenerator.Instance.Player.CanReceiveInput = true;
    }
}
