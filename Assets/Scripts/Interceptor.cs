using System;
using System.Collections.Generic;
using UnityEngine;

public class Interceptor : MonoBehaviour
{
    public GameObject Carrier = null;
    public float BuildCost = 1000f;

    // For targeting
    private GameObject target = null;
    public string ScanLayer = null;
    // List of enemy types in priority order
    private List<string> scanLayers = new List<string>();

    // State
    enum TrackingState { Hunting, Patrolling, Resting, Launching, Returning, Supporting };
    private TrackingState trackingState = TrackingState.Patrolling; // TBD Launching

    // Return to this position when on patrol
    public GameObject patrolLocation = null;
    private Vector3 basePosition;
    public bool shouldOrbit = false;

    private float huntingStart = 0f;
    public float huntingTimeout = 8f;

    // Components
    public InterceptorPropulsion propulsion = null;
    private InterceptorScanner scanner = null;
    private InterceptorWeapon weapon = null;

    public Squadron squadron = null;
    public int squadronPosition = 0;

    public int shipSTR = 0;
    public int shipINT = 0;
    public int shipDEX = 0;

    private bool wingman = false;

    public string shipName = "";

    // Start is called before the first frame update
    void Start()
    {
        shipName = gameObject.name;
        propulsion = GetComponent<InterceptorPropulsion>();
        scanner = GetComponent<InterceptorScanner>();
        weapon = GetComponent<InterceptorWeapon>();
        if (Carrier == null)
        {
            basePosition = transform.position;
            trackingState = TrackingState.Resting;
            propulsion.StopMoving();
            scanLayers.Add("Bombers");
            scanLayers.Add("Fighters");
            scanLayers.Add("Landers");
        }
        else
        {
            // Launched from carrier
            target = patrolLocation;
            trackingState = TrackingState.Launching;
            propulsion.TrackTarget(target.transform, Vector3.zero);
            //propulsion.state = InterceptorPropulsion.PropulsionState.Sport;
            propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
            String[] substrings = ScanLayer.Split(',');
            foreach (String s in substrings)
            {
                scanLayers.Add(s);
            }

            Vector3 deltaPosition = target.transform.position - transform.position;
            Quaternion desiredRotation = Quaternion.LookRotation(Vector3.forward, deltaPosition);
            Vector3 desiredAngles = desiredRotation.eulerAngles;
            propulsion.heading = desiredAngles.z;

            if (squadron != null && squadron.Formation() > 0)
            {
                shipName = squadron.squadronName + "_" + squadronPosition;
                if (squadron.Leader() == gameObject)
                {
                    Debug.Log(shipName + " leader checking in");
                }
                else
                {
                    Debug.Log(shipName + " checking in");
                    wingman = true;
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (trackingState == TrackingState.Resting)
        {
            propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
            propulsion.StopMoving();
            // Go to sleep
            propulsion.enabled = false;
            this.enabled = false;
            return;
        }

        if (trackingState == TrackingState.Returning)
        {
            if (propulsion.AtWaypoint)
            {
                //Debug.Log("Ship returned.");
                Carrier.GetComponent<ResourceTracker>().AddResources(BuildCost *
                    gameObject.GetComponent<HealthTracker>().health / gameObject.GetComponent<HealthTracker>().maxHealth);
                Die();
            }
        }

        if (trackingState == TrackingState.Launching)
        {
            //Debug.Log("Ship pos: " + transform.position + " Target: " + patrolLocation.transform.position);
            //Debug.Log("Distance: " + Vector3.Magnitude(transform.position - patrolLocation.transform.position));
            //Debug.Log("DistanceToTarget: " + propulsion.DistanceToTarget);
            if (propulsion.AtWaypoint)
            {
                Debug.Log(shipName + ": Reached launch waypoint, patrolling...");
                if (wingman)
                {
                    // Follow formation leader
                    Transform waypoint = squadron.Leader().transform;
                    // Line formation
                    float distance = 0.5f * (squadronPosition + 1) / 2;
                    float direction = (squadronPosition % 2) * 2 - 1;
                    Vector3 offset = new Vector3(direction * distance, -1 * distance, 0);
                    Debug.Log(shipName + " offset " + offset);
                    propulsion.TrackTarget(waypoint, offset);
                    propulsion.state = InterceptorPropulsion.PropulsionState.Follow;

                    trackingState = TrackingState.Supporting;
                }
                else
                {
                    trackingState = TrackingState.Patrolling;
                    if (shouldOrbit)
                    {
                        propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
                        propulsion.OrbitTarget(Carrier.transform, patrolLocation.transform.position);
                    }
                }
            }
            else
            {
                //Debug.Log("not at waypoint yet");
            }
        }

        if (trackingState == TrackingState.Patrolling)
        {
            if (true || propulsion.NearWaypoint)
            {
                GameObject newTarget = scanner.FindTarget(scanLayers);
                if (newTarget == null)
                {
                    propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
                    if (Carrier == null)
                    {
                        trackingState = TrackingState.Resting;
                        propulsion.StopMoving();
                        // TBD: This causes enemy interceptors to stop in mid-space. Looks weird.
                    }
                }
                else
                {
                    Debug.Log(shipName + " Found target " + newTarget.GetComponent<Interceptor>().shipName + " Range: " + Vector3.Magnitude(transform.position - newTarget.transform.position));
                    target = newTarget;
                    trackingState = TrackingState.Hunting;
                    huntingStart = Time.time;
                    propulsion.state = InterceptorPropulsion.PropulsionState.Sport;
                    InterceptTarget(target);
                }
            }
        }

        if (trackingState == TrackingState.Hunting)
        {
            if (target != null)
            {
                if (Time.time > huntingStart + huntingTimeout)
                {
                    target = null;
                }
                else if (target.layer == LayerMask.NameToLayer("Captured"))
                {
                    target = null;
                }
                else if (Vector3.Magnitude(target.transform.position - transform.position) < weapon.weaponRange)
                {
                    if (weapon.FireWeapon(target))
                    {
                        string targetName = target.name;

                        if (targetName.StartsWith("Cluster") ||
                            targetName.StartsWith("City") ||
                            targetName.StartsWith("Colony"))
                        {
                            Debug.Log(shipName + " captured " + targetName);
                            Carrier.GetComponent<ResourceTracker>().AddEnergy(1f);
                            Carrier.GetComponent<ResourceTracker>().AddResources(5000f);
                        }

                        // defeated target
                        target = null;
                    }
                }
            }
            if (target == null)
            {
                target = scanner.FindTarget(scanLayers);
                if (target == null)
                {
                    trackingState = TrackingState.Patrolling;
                    if (Carrier == null)
                    {
                        propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
                        propulsion.SetWaypoint(basePosition);
                    }
                    else
                    {
                        propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
                        if (shouldOrbit)
                        {
                            propulsion.OrbitTarget(Carrier.transform, patrolLocation.transform.position);
                        }
                        else
                        {
                            propulsion.TrackTarget(patrolLocation.transform, Vector3.zero);
                        }
                    }
                }
                else
                {
                    huntingStart = Time.time;
                    propulsion.state = InterceptorPropulsion.PropulsionState.Sport;
                    InterceptTarget(target);
                }
            }
        }

        if (trackingState == TrackingState.Supporting)
        {
            if (squadron.Leader() == null || squadron.Leader() == gameObject)
            {
                Debug.Log("Oh no, the leader is gone");
                wingman = false;
                trackingState = TrackingState.Patrolling;
            }
            else
            {
                //propulsion.state = squadron.Leader().GetComponent<Interceptor>().propulsion.state;

                if (squadron.Leader().GetComponent<Interceptor>().trackingState == TrackingState.Hunting)
                {
                    target = squadron.Leader().GetComponent<Interceptor>().target;
                    if (target != null)
                    {
                        if (Vector3.Magnitude(target.transform.position - transform.position) < weapon.weaponRange)
                        {
                            if (weapon != null && weapon.FireWeapon(target))
                            {
                                string targetName = target.name;

                                if (targetName.StartsWith("Cluster") ||
                                    targetName.StartsWith("City") ||
                                    targetName.StartsWith("Colony"))
                                {
                                    Debug.Log(name + " captured " + targetName);
                                    Carrier.GetComponent<ResourceTracker>().AddEnergy(1f);
                                    Carrier.GetComponent<ResourceTracker>().AddResources(5000f);
                                }

                                // defeated target
                                target = null; // TBD needed?
                            }
                        }
                    }
                }

            }
        }

    }

    private void InterceptTarget(GameObject target)
    {
        if (target.name.StartsWith("Cluster") ||
            target.name.StartsWith("Colony"))
        {
            // Strafe target
            Vector3 delta = target.transform.position - transform.position;
            if (Vector3.Magnitude(delta) > 5)
            {
                Debug.Log("Strafe: distance: " + Vector3.Magnitude(delta));
                Vector3 centerPoint = transform.position + 0.5f * delta;
                Transform orbitCenter = new GameObject().transform;
                orbitCenter.position = centerPoint;
                propulsion.OrbitTarget(orbitCenter, transform.position);
            }
            else
            {
                Debug.Log("Slow attack: distance: " + Vector3.Magnitude(delta));
                // Follow target slowly
                propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
                propulsion.TrackTarget(target.transform, Vector3.zero);

            }
        }
        else
        {
            // Follow target
            propulsion.TrackTarget(target.transform, Vector3.zero);
        }

    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check for null. Ships can get shot down pretty quickly.
        if (collision != null)
        {
            if (scanner.IsTarget(scanLayers, collision.gameObject))
            {
                //Debug.LogWarning(gameObject.name + ": Target approaching.");
                // Wake up
                this.enabled = true;
                propulsion.enabled = true;

                target = collision.gameObject;
                //Debug.Log(shipName + " targeting " + target.name);
                trackingState = TrackingState.Hunting;
                propulsion.state = InterceptorPropulsion.PropulsionState.Sport;
                propulsion.TrackTarget(target.transform, Vector3.zero);
            }
        }
    }

    public void Recall()
    {
        // Return ship to carrier.
        trackingState = TrackingState.Returning;
        target = Carrier;
        propulsion.state = InterceptorPropulsion.PropulsionState.Eco;
        propulsion.TrackTarget(target.transform, Vector3.zero);

    }

    public void Die()
    {
        if (squadron != null)
        {
            squadron.RemoveMember(gameObject);
        }
        Destroy(gameObject);
    }
}
