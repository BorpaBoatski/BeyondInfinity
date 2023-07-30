using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketCollision : MonoBehaviour
{
    #region Properties

    const string c_PlanetTag = "Planet";
    RocketMovement _Rocket;

    #endregion

    private void Awake()
    {
        _Rocket = GetComponent<RocketMovement>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Planet")) return;

        _Rocket.Land(collision.attachedRigidbody.GetComponent<Planet>());
    }
}
