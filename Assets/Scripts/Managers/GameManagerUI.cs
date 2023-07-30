using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using UnityEngine.SceneManagement;

public class GameManagerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    TextMeshProUGUI _ScoreText;
    [SerializeField]
    RectTransform _PowRect;
    [SerializeField]
    TextMeshProUGUI _ScoreModifierText;
    [SerializeField]
    Image BlackCover;
    [SerializeField]
    TextMeshProUGUI LostText;
    [SerializeField]
    CameraManager Camera;
    [SerializeField]
    TextMeshProUGUI RestartText;
    [SerializeField]
    TextMeshProUGUI GameOverScoreText;
    [SerializeField]
    TextMeshProUGUI GameOverScoreLabel;
    [SerializeField]
    Button RestartButton;
    [SerializeField]
    CanvasGroup ScoreGroup;
    [SerializeField]
    string[] LoseQuotes;

    [Header("Developer")]
    [SerializeField]
    int _MaxPowScale;
    [SerializeField]
    int _ScoreToStartPow = 20;

    #region Properties

    Sequence _ScoreGainSequence;
    float _DefaultFontSize;
    const string c_DoubleDigitFormat = "<rotate={0}>{1}</rotate><rotate=-{0}>{2}</rotate>";
    const string c_ScoreModifier = "x{0}";
    const string c_ScribbleFormat = "Scribble_{0}";

    #endregion

    private void Awake()
    {
        _DefaultFontSize = _ScoreText.fontSize;
    }

    private void Start()
    {
        GameManager.Instance.OnScoreChange += UpdateScoreText;
        GameManager.Instance.OnScoreModifierChange += UpdateScoreModifierText;
    }

    void UpdateScoreText(int scoreValue)
    {
        _ScoreText.text = scoreValue.ToString();
        ScoreGainAnimation();
    }

    void ScoreGainAnimation()
    {
        if (_ScoreGainSequence != null) _ScoreGainSequence.Complete();
        _ScoreGainSequence = DOTween.Sequence();

        if(_ScoreText.text.Length < 2)
        {
            SingleDigitAnimation();
        }
        else
        {
            DoubleDigitAnimation();
        }
    }

    void SingleDigitAnimation()
    {
        _ScoreGainSequence.Append(DOVirtual.Float(_DefaultFontSize, _DefaultFontSize * 2, 1, (x) => UpdateFontSize(x)));
        _ScoreGainSequence.Join(DOTween.Shake(() => _ScoreText.transform.eulerAngles, (x) => _ScoreText.transform.eulerAngles = x, 1, 
            100, ignoreZAxis: false));
        _ScoreGainSequence.Append(DOVirtual.Float(_DefaultFontSize * 2, _DefaultFontSize, 1, (x) => UpdateFontSize(x)));
    }

    void DoubleDigitAnimation()
    {
        bool _toPow = GameManager.Instance.Score >= 20;
        float _tweenTimes = .5f;

        if(_toPow) _PowRect.localScale = Vector3.one;

        _ScoreGainSequence.Append(DOVirtual.Float(_DefaultFontSize, _DefaultFontSize * 2, _tweenTimes, (x) => UpdateFontSize(x)));
        _ScoreGainSequence.Join(DOVirtual.Float(0, 30, _tweenTimes, (x) => UpdateCharacterSpacing(x)));
        _ScoreGainSequence.Join(DOVirtual.Float(0, 30, _tweenTimes, (x) => RotateCharacters(x)));
        _ScoreGainSequence.Join(_PowRect.DOScale(3, _tweenTimes));
        _ScoreGainSequence.Append(DOVirtual.Float(_DefaultFontSize * 2, _DefaultFontSize, _tweenTimes, (x) => UpdateFontSize(x)));
        _ScoreGainSequence.Join(DOVirtual.Float(30, 0, _tweenTimes, (x) => UpdateCharacterSpacing(x)));
        _ScoreGainSequence.Join(DOVirtual.Float(30, 0, _tweenTimes, (x) => RotateCharacters(x)));
        _ScoreGainSequence.Join(_PowRect.DOScale(Vector3.zero, _tweenTimes));
    }

    private void UpdateFontSize(float fontSize)
    {
        _ScoreText.fontSize = fontSize;
    }

    void UpdateCharacterSpacing(float spacing)
    {
        _ScoreText.characterSpacing = spacing;
    }

    void RotateCharacters(float rotation)
    {
        string _cachedScore = GameManager.Instance.Score.ToString();
        _ScoreText.text = string.Format(c_DoubleDigitFormat, rotation, _cachedScore[0], _cachedScore[1]);
    }

    void UpdateScoreModifierText(float value)
    {
        if (value == 1)
        {
            _ScoreModifierText.text = string.Empty;
            return;
        }

        _ScoreModifierText.DOText(string.Format(c_ScoreModifier, value), 1);
    }

    public void DisplayGameOver()
    {
        ScoreGroup.alpha = 0;
        OrbitGenerator.Instance.UI.MyCanvas.enabled = false;
        OrbitGenerator.Instance.Player.FuelBar.GetComponentInParent<Canvas>().enabled = false;
        GameOverScoreText.text = GameManager.Instance.Score.ToString();
        BlackCover.enabled = true;

        Sequence GameOverSequence = DOTween.Sequence();

        GameOverSequence.Append(BlackCover.DOFade(1, 1));
        GameOverSequence.AppendCallback(() => StartCoroutine(BeginPlayerRotation()));
        ///1 sec
        GameOverSequence.Insert(2, BlackCover.DOFade(0, 1));
        ///3 sec
        //GameOverSequence.AppendCallback(() => PlayCharacterSounds());
        GameOverSequence.Append(LostText.DOText(LoseQuotes[Random.Range(0, LoseQuotes.Length)], 3)).SetEase(Ease.OutQuad);

        ///6 sec
        GameOverSequence.AppendCallback(() => GameOverScoreLabel.enabled = true);
        GameOverSequence.InsertCallback(7, () => GameOverScoreText.enabled = true);
        ///7 sec
        GameOverSequence.Append(GameOverScoreText.DOCounter(0, GameManager.Instance.Score, 1));
        GameOverSequence.AppendCallback(() => ShowRestartText());

        StartCoroutine(PlayCharacterSounds(GameOverSequence));
    }

    IEnumerator PlayCharacterSounds(Sequence relatedSequence)
    {
        int _lastScribbleSFX = 0;
        int _lastCheckedVisibleCharacters = -1;
        DOTweenTMPAnimator animator = new DOTweenTMPAnimator(LostText);

        while(relatedSequence.IsPlaying())
        {
            for (int i = 0; i < animator.textInfo.characterCount; i++)
            {
                if (!animator.textInfo.characterInfo[i].isVisible) continue;

                if (_lastCheckedVisibleCharacters >= i) continue;

                _lastCheckedVisibleCharacters = i;
                _lastScribbleSFX = Random.Range(0, 3);
                AudioManager.Instance.PlaySFX(string.Format(c_ScribbleFormat, _lastScribbleSFX.ToString()));
            }

            yield return null;
        }
    }

    void ShowRestartText()
    {
        RestartButton.enabled = true;
        RestartText.DOFade(1, 1).SetLoops(-1, LoopType.Yoyo);
    }

    IEnumerator BeginPlayerRotation()
    {
        while(true)
        {
            OrbitGenerator.Instance.Player.transform.eulerAngles += Vector3.forward * Time.deltaTime * 3;
            yield return null;
        }
    }

    public void OnClickRestart()
    {
        SceneManager.LoadScene(1);
    }
}
