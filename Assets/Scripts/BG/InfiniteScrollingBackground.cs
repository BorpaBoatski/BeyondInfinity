using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteScrollingBackground : MonoBehaviour
{
    public Vector2 AutoScrollSpeed;
    public Vector2 ParallaxEffectMultiplier;
    Transform _CameraTransform;
    Vector3 _LastCameraPosition;
    float _TextureUnitSizeX, _TextureUnitSizeY;

    private void Start()
    {
        _CameraTransform = Camera.main.transform;
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        _TextureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        _TextureUnitSizeY = texture.height / sprite.pixelsPerUnit;
    }

    private void Update()
    {
        transform.position += new Vector3(AutoScrollSpeed.x, AutoScrollSpeed.y);

        Vector3 _deltaMovement = _CameraTransform.position - _LastCameraPosition;
        transform.position += new Vector3(_deltaMovement.x * ParallaxEffectMultiplier.x, _deltaMovement.y * ParallaxEffectMultiplier.y);
        _LastCameraPosition = _CameraTransform.position;

        if (Mathf.Abs(_CameraTransform.position.x - transform.position.x) >= _TextureUnitSizeX) // Horizontal Check
        {
            float offsetPositionX = (_CameraTransform.position.x - transform.position.x) % _TextureUnitSizeX;
            transform.position = new Vector2(_CameraTransform.position.x + offsetPositionX, transform.position.y);
        }

        if (Mathf.Abs(_CameraTransform.position.y - transform.position.y) >= _TextureUnitSizeY) // Vertical Check
        {
            float offsetPositionY = (_CameraTransform.position.y - transform.position.y) % _TextureUnitSizeY;
            transform.position = new Vector2(transform.position.x, _CameraTransform.position.y + offsetPositionY);
        }
    }
}
