using System;
using UnityEngine;

public class SFXActor_Rock : SFXActor
{
    [SerializeField] private LayerMask _collisionLayers;
    private void OnCollisionEnter(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & _collisionLayers.value) != 0)
        {
            PlaySound(SFX.RockHit);
        }
    }
}
