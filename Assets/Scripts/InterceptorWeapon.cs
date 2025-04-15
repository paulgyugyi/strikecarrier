using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine;

public class InterceptorWeapon : MonoBehaviour
{
    // Weapon settings
    public float reloadTime = 1.0f;
    public float firingArc = 30f;
    private float lastFireTime = 0;
    public int minDamage = 1;
    public int maxDamage = 100;
    public float weaponRange = 1.5f;
    public GameObject LaserbeamPrefab;
    private LineRenderer laserBeam = null;
    private GameObject laserTarget = null;
    private Vector3 laserTargetPosition = Vector3.zero;
    private GameObject laserObject = null;

    // Audio
    public AudioSource audioSource;
    public AudioClip clip;
    public float volume = 0.5f;

    private void OnDestroy()
    {
        if (laserObject != null)
        {
            Destroy(laserObject);
        }    
    }

    void Update()
    {
        if ((laserObject != null) && laserObject.activeSelf)
        {
            // Update end of laser to track moving target
            // otherwise end at debris cloud at last known position.
            if (laserTarget != null)
            {
                laserTargetPosition = laserTarget.transform.position;
            }
            laserBeam.SetPosition(0, transform.position);
            laserBeam.SetPosition(1, laserTargetPosition);
            if (Time.time > lastFireTime + 0.1f)
            {
                laserObject.SetActive(false);
            }
        }
    }

    // return true if target defeated.
    public bool FireWeapon(GameObject target)
    {
        if (target == null)
        {
            return false;
        }
        // Check if target is in range
        //float targetRange = Vector3.Magnitude(transform.position - target.transform.position);
        //if (targetRange > weaponRange) {
        //    return false;
        //}
        {
            // Check if target is in firing arc
            float targetBearing = Vector3.Angle(transform.up, target.transform.position - transform.position);
            if (targetBearing > firingArc) {
                return false;
            }     
        } 
        if (Time.time > lastFireTime + reloadTime)
        {
            lastFireTime = Time.time;
            if ((audioSource != null) && (clip != null))
            {
                audioSource.PlayOneShot(clip, volume);
            }
            if (LaserbeamPrefab != null)
            {
                // Display laser beam to target.
                if (laserObject == null)
                {
                    laserObject = Instantiate(LaserbeamPrefab, Vector3.zero,
                        Quaternion.identity);
                }
                laserObject.SetActive(true);
                laserBeam = laserObject.GetComponent<LineRenderer>();
                laserTarget = target;
                laserTargetPosition = laserTarget.transform.position;
                laserBeam.SetPosition(0, transform.position);
                laserBeam.SetPosition(1, laserTargetPosition);
            }
            float damage = Random.Range(minDamage, maxDamage);
            if (target.GetComponent<HealthTracker>().TakeDamage(damage, gameObject))
            {
                return true;
            }
        }
        return false;
    }

}
