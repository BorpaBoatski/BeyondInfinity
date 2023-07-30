using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class PlanetDetailsCanvas : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    Image PlanetImage;
    [SerializeField]
    TextMeshProUGUI PlanetNameText;
    [SerializeField]
    TextMeshProUGUI PlanetDetailText;
    [SerializeField]
    TextMeshProUGUI TapLaunchLabel;
    [SerializeField]
    RectTransform _WindowRect;

    #region Properties

    CanvasGroup _MyCanvasGroup;
    Vector3 _PlanetScreenPosition;
    Sequence _RevealSequence;
    Sequence _CloseSequence;
    Tween _DragLabelTween;

    #endregion

    private void Awake()
    {
        _MyCanvasGroup = GetComponent<CanvasGroup>();
    }

    public void InitializeUI(Planet landedPlanet)
    {
        if (!OrbitGenerator.Instance.IsUndiscoveredPlanet(landedPlanet)) return;

        _PlanetScreenPosition = Camera.main.WorldToScreenPoint(landedPlanet.transform.position);
        ResetElements();
       
        PlanetImage.sprite = landedPlanet.MyDetails.PlanetSprite;
        PlanetImage.transform.localScale = Vector2.one * 2;
        PlanetImage.rectTransform.anchoredPosition = new Vector2(PlanetImage.rectTransform.rect.width / 2f,
            PlanetImage.rectTransform.rect.height / 2f);

        _RevealSequence = DOTween.Sequence();

        _RevealSequence.Append(_WindowRect.DOScale(Vector2.one, 1));
        _RevealSequence.Join(_WindowRect.DOAnchorPos(Vector2.zero, 1));
        _RevealSequence.Append(PlanetImage.rectTransform.DOAnchorPos(Vector2.zero, 1).SetEase(Ease.InBack));
        _RevealSequence.Join(PlanetImage.rectTransform.DOScale(Vector2.one, 1).SetEase(Ease.InBack));
        _RevealSequence.Append(PlanetNameText.DOText(landedPlanet.MyDetails.PlanetName, 1));
        _RevealSequence.Join(PlanetDetailText.DOText(landedPlanet.MyDetails.PlanetDescription, 1));
        _RevealSequence.AppendCallback(() => AllowContinue());
        _MyCanvasGroup.alpha = 1;
    }

    void ResetElements()
    {
        _MyCanvasGroup.alpha = 0;
        TapLaunchLabel.alpha = 0;
        PlanetDetailText.text = string.Empty;
        PlanetNameText.text = string.Empty;
        _WindowRect.transform.position = _PlanetScreenPosition;
        _WindowRect.transform.localScale = Vector3.zero;
    }

    void AllowContinue()
    {
        _DragLabelTween = TapLaunchLabel.DOFade(1,1).SetLoops(-1, LoopType.Yoyo);
    }

    public void Close()
    {
        if (_MyCanvasGroup.alpha == 0) return;
        if (_CloseSequence != null && _CloseSequence.IsPlaying()) return;
        if (_RevealSequence != null) _RevealSequence.Kill();
        if (_DragLabelTween != null) _DragLabelTween.Kill();

        _CloseSequence = DOTween.Sequence();

        _CloseSequence.Append(_WindowRect.DOScale(0, 1));
        _CloseSequence.Join(_WindowRect.DOMove(_PlanetScreenPosition, 1));
        _CloseSequence.OnComplete(() => _MyCanvasGroup.alpha = 0);
    }
}
