using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteeringUI : MonoBehaviour
{
    public Transform LeftSteerArrow;
    public Transform RightSteerArrow;    
    public GameObject SteeringHintHolder;
    public GameObject SteerHint;

    Tween _LeftSteerTween, _RightSteerTween;
    bool _IsHintShown;

    public void ToggleSteeringUI(bool state)
    {
        if (state)
        {
            LeftSteerArrow.gameObject.SetActive(true);
            RightSteerArrow.gameObject.SetActive(true);
            _LeftSteerTween = LeftSteerArrow.DOMoveX(LeftSteerArrow.position.x - 10.0f, 1).SetLoops(-1, LoopType.Yoyo);
            _RightSteerTween = RightSteerArrow.DOMoveX(RightSteerArrow.position.x + 10.0f, 1).SetLoops(-1, LoopType.Yoyo);

            if (_IsHintShown) return;
            SteeringHintBlinking();
        }
        else
        {
            if (_LeftSteerTween.IsPlaying())
            {
                _LeftSteerTween.Kill();
            }
            if (_RightSteerTween.IsPlaying())
            {
                _RightSteerTween.Kill();
            }

            LeftSteerArrow.gameObject.SetActive(false);
            RightSteerArrow.gameObject.SetActive(false);
        }
    }

    void SteeringHintBlinking()
    {
        _IsHintShown = true;
        SteerHint.gameObject.SetActive(true);

        foreach (Transform child in SteeringHintHolder.transform)
        {
            child.gameObject.SetActive(true);
            child.GetComponent<Image>().DOFade(0.1f, 0.5f).SetLoops(6, LoopType.Yoyo).OnComplete(() => 
            {
                SteerHint.gameObject.SetActive(false);
                child.gameObject.SetActive(false); 
            });
        }
    }
}
