using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(LineRenderer))]

public class GunScript : MonoBehaviour
{

    LineRenderer line;

    //Impact particles
    public GameObject impactEffect;

    //Gun Stats
    public float laserDuration = 0.05f;
    public float gunDamage = 20f;
    public float range = 100f;
    public float shootForce, upwardForce;
    public float timeBeforeShooting, spread, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowHold;

    //keep track of bullets
    int bulletsLeft, bulletsShot;

    //bools
    bool shooting, readyToShoot, reloading;
    public bool allowInvoke = true;

    //reference
    public ParticleSystem gunSmoke;

    public Camera fpsCam;
    public Transform attackPoint;

    //Graphics
    public TextMeshProUGUI ammunitionDisplay;

    void Start()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
        bulletsShot = 0;
        reloading = false;
        line = GetComponent<LineRenderer>();
        
    }

    // Update is called once per frame
    void Update()
    {
        Inputs();

        if (ammunitionDisplay != null)
            ammunitionDisplay.SetText(bulletsLeft + " / " + magazineSize);
    }

    private void Inputs()
    {
        if (allowHold)
        {
            shooting = Input.GetButton("Fire1");
        }
        else
        {
            shooting = Input.GetButtonDown("Fire1");
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();

        if (readyToShoot && shooting && !reloading && bulletsLeft <= 0) Reload();

        if (readyToShoot && !reloading && shooting && bulletsLeft > 0) Shoot();

    }
    
    void Shoot()
    {
        readyToShoot = false;

        //Info
        Vector3 rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
        line.SetPosition(0, attackPoint.position);
        RaycastHit hitInfo;
        if (Physics.Raycast(fpsCam.transform.position, fpsCam.transform.forward, out hitInfo, range))
        {
            Debug.Log(hitInfo.transform.name);

            //If hit enemy (Damage)
            Target enemy = hitInfo.transform.GetComponent<Target>();
            if (enemy != null)
            {
                enemy.LoseHealth(gunDamage);
            }

            //If hit any body (Knockback)
            if (hitInfo.rigidbody != null)
            {
                hitInfo.rigidbody.AddForce(-hitInfo.normal * shootForce);
            }

            Instantiate(impactEffect, hitInfo.point, Quaternion.LookRotation(hitInfo.normal));
        }
        else
        {
            line.SetPosition(1, rayOrigin + (fpsCam.transform.forward * range));
            StartCoroutine(ShootLaser());
        }
        gunSmoke.Play();

        bulletsShot++;
        bulletsLeft--;

        //Invoke resertShot function (if not already invoked)
        if (allowInvoke)
        {
            Invoke("ResetShot", timeBetweenShots);
            allowInvoke = false;
        }
    }

    void ResetShot()
    {
        //Ready to shoot then invoke again
        readyToShoot = true;
        allowInvoke = true;
    }

    IEnumerator ShootLaser()
    {
        line.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        line.enabled = false;
    }

    private void Reload()
    {
        reloading = true;
        Invoke("ReloadFinished", reloadTime);
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
