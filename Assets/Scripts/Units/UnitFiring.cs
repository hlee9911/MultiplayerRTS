using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class UnitFiring : NetworkBehaviour
{
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private GameObject projectilePrefab = null;
    [SerializeField] private Transform projectileSpawnPoint = null;
    [SerializeField] private float fireRange = 5f;
    [SerializeField] private float fireRate = 2f;
    [SerializeField] private float rotationSpeed = 40f;

    private float lastFireTime;

    [ServerCallback]
    void Update()
    {
        FireProjectile();
    }

    void FireProjectile()
    {
        Targetable target = targeter.Target;

        // if we don't have a target don't call this method
        if (!target) return;

        if (!CanFireAtTarget()) return;

        Quaternion targetRotation =
            Quaternion.LookRotation(target.transform.position
            - transform.position);

        transform.rotation = Quaternion.RotateTowards(transform.rotation,
                                                      targetRotation,
                                                      rotationSpeed * Time.deltaTime);

        if (Time.time > (1 / fireRate) + lastFireTime)
        {
            // angle for firing at a small or bigger target
            Quaternion projectileRotation = Quaternion.LookRotation(target.AimAtPoint.position
                                                                    - projectileSpawnPoint.position);

            GameObject projectileInstance = Instantiate(projectilePrefab,
                                                        projectileSpawnPoint.position,
                                                        projectileRotation);

            NetworkServer.Spawn(projectileInstance, connectionToClient);

            // we can now fire
            lastFireTime = Time.time;
        }
    }

    // check the firing range if it can fire
    [Server]
    private bool CanFireAtTarget()
    {
        return (targeter.Target.transform.position - transform.position).sqrMagnitude 
            <= fireRange * fireRange;
    }
}
