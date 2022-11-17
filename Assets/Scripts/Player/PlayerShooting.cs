using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Options")]
    public bool isDynamic;
    public float BulletDamage;
    public float ShootingRange;
    public KeyCode shootKey = KeyCode.Mouse0;
    [SerializeField] private ParticleSystem shotParticleSystem;
    [SerializeField] private GameObject impactEffect;

    [SerializeField] private Camera fpsCam;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shootKey))
            Shoot();
    }

    private void Shoot()
    {
        shotParticleSystem.Play();
        
        RaycastHit hit;
        
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, ShootingRange))
        {
            HealthSystem target = hit.transform.GetComponent<HealthSystem>();
            if (target != null)
                target.TakeDamage(BulletDamage);

            Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
        }

    }
}
