using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public Transform Player;
    public float CameraCatchUpDuration = 1.0f;
    float _TimeElapsed;
    bool _IsTrackingVertical, _IsFullTracking;
    Vector2 _InitialCameraPos;
    Vector2 _Velocity = Vector2.zero;
    float _HorizontalValue = 0;
    float _VerticalValue = 0;

    private void Update()
    {
        UpdateCameraPosition();

        if (!_IsTrackingVertical)
        {
            Vector2 _playerScreenPos = Camera.main.WorldToScreenPoint(Player.position);
            if (_playerScreenPos.y >= Screen.height / 2.0f)
            {
                _IsTrackingVertical = true;
                _TimeElapsed = 0;
                _InitialCameraPos = transform.position;
            }
        }
    }

    void UpdateCameraPosition()
    {
        float _xPos = Mathf.SmoothDamp(transform.position.x, Player.position.x, ref _HorizontalValue, 0.05f);
        float _yPos = _IsTrackingVertical ? Mathf.SmoothDamp(transform.position.y, Player.position.y, ref _VerticalValue, 0.25f) : transform.position.y;

        transform.position = new Vector2(_xPos, _yPos);
    }
}
