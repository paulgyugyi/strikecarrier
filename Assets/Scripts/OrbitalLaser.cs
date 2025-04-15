using System.Collections.Generic;
using UnityEngine;

public class OrbitalLaser : MonoBehaviour
{
    private GameObject target = null;
    // List of enemy types in priority order
    private List<string> scanLayers = new List<string>() { "Landers", "Fighters", "Bombers"};

    // State
    enum TrackingState { Hunting, Patrolling };
    private TrackingState trackingState = TrackingState.Patrolling;

    // Weapon settings
    private InterceptorWeapon weapon = null;
    private InterceptorScanner scanner = null;

    void Start()
    {
        weapon = GetComponent<InterceptorWeapon>();
        scanner = GetComponent<InterceptorScanner>();
    }

    void FixedUpdate()
    {
        if (target == null)
        {
            return;
        }
        Vector3 targetPosition = target.transform.position;
        Vector3 deltaPosition = targetPosition - transform.position;

        // Point sprite in direction of target
        transform.rotation = Quaternion.LookRotation(Vector3.forward, deltaPosition);

        if ((trackingState == TrackingState.Hunting) &&
            (Vector3.Magnitude(deltaPosition) < weapon.weaponRange))
        {
            if (weapon.FireWeapon(target))
            {
                // defeated target
                target = null;
            }
            if (target == null)
            {
                target = scanner.FindTarget(scanLayers);
                if (target == null)
                {
                    trackingState = TrackingState.Patrolling;
                }
            }
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            if (scanner.IsTarget(scanLayers, collision.gameObject))
            {
                target = collision.gameObject;
                Debug.Log(name + " targeting " + target.name);
                trackingState = TrackingState.Hunting;
            }
        }
    }
}
