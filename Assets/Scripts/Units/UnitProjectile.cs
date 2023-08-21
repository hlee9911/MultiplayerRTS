using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitProjectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody rb = null;
    [SerializeField] private int damageToDeal = 20;
    [SerializeField] private float projectileLifeTime = 5f;
    [SerializeField] private float launchForce = 10f;

    void Start()
    {
        // fire the projectile
        rb.velocity = transform.forward * launchForce;
    }

    public override void OnStartServer()
    {
        // everyone will launch off,
        // and after five seconds, destory self
        Invoke(nameof(DestroySelf), projectileLifeTime);
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        // if the collided object has network identity, 
        if (other.TryGetComponent<NetworkIdentity>(out NetworkIdentity networkIdentity))
        {
            // let's check if it's belong to us, meaning we hit our own unit
            // we don't want the freindly fire on
            if (networkIdentity.connectionToClient == connectionToClient) return;
        }

        if (other.TryGetComponent<HealthManager>(out HealthManager health))
        {
            health.DealDamage(damageToDeal);
        }

        DestroySelf();
    }

    [Server] 
    private void DestroySelf()
    {
        // destory object in the network
        NetworkServer.Destroy(gameObject);
    }
}
