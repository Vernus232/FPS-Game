using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Options")]
    public float BulletDamage;
    public float ShootingRange;
    public KeyCode shootKey = KeyCode.Mouse0;

    [SerializeField] private Camera fpsCam;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(shootKey))
            Shoot();
    }

    private void Shoot()
    {
        RaycastHit hit;
        
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hit, ShootingRange))
        {
            print(hit.transform.name);
        }
    }
}
