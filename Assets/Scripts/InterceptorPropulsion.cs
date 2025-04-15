using UnityEngine;

public class InterceptorPropulsion : MonoBehaviour
{
    // Eco: Cruising speed, and slow down when approaching waypoint
    // Sport: Maintain max speed
    public enum PropulsionState { Eco, Sport };
    public PropulsionState state = PropulsionState.Eco;

    public GameObject regionLockCenter = null;
    public float regionLockRadius = 0f;

    private Rigidbody2D rb2D;
    public int level = 2;
    private float maxSpeed = 1f; // Change for ships: 1f
    private float maxThrust = 16f;
    private float deadzone = 0.6f;
    private float rotationSpeed = 1f;

    private Transform target = null;

    private Vector3 waypoint = Vector3.zero;
    private float distanceToWaypoint = 0.0f;

    public float heading = 0f;
    private bool shouldOrbit = false;
    private Orbit orbitComponent = null;
    private float cruisingSpeed;
    private Quaternion startingRotation;

    public float DistanceToWaypoint
    {
        get
        {
            return distanceToWaypoint;
        }
    }

    public bool AtWaypoint
    {
        get
        {
            return distanceToWaypoint < deadzone;
        }
    }
    public bool NearWaypoint
    {
        get
        {
            return distanceToWaypoint < 4 * deadzone;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        maxSpeed = level;
        rotationSpeed = 0.5f * level;
        if (orbitComponent == null)
        {
            orbitComponent = GetComponent<Orbit>();
        }
        deadzone = maxSpeed / 8f;
        cruisingSpeed = maxSpeed * 0.75f;
        startingRotation = transform.rotation;
        heading = startingRotation.eulerAngles.z;
    }

    public void OrbitTarget(Transform newTarget, Vector3 startPosition)
    {
        target = newTarget;
        shouldOrbit = true;
        if (target != null)
        {
            waypoint = startPosition;
        }
        if (orbitComponent == null)
        {
            orbitComponent = GetComponent<Orbit>();
        }
        float orbitingSpeed = maxSpeed;
        if (state == PropulsionState.Eco)
        {
            orbitingSpeed = cruisingSpeed;
        }
        orbitComponent.InitializeOrbit(target, startPosition, 0.95f * orbitingSpeed);
        distanceToWaypoint = Vector3.Magnitude(waypoint - transform.position);
    }

   public void TrackTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            waypoint = target.position;
        }
        shouldOrbit = false;
        distanceToWaypoint = Vector3.Magnitude(waypoint - transform.position);
    }

    public void SetWaypoint(Vector3 newPosition)
    {
        target = null;
        waypoint = newPosition;
        shouldOrbit = false;
        distanceToWaypoint = Vector3.Magnitude(waypoint - transform.position);
    }

    public void StopMoving()
    {
        target = null;
        waypoint = transform.position;
        distanceToWaypoint = 0f;
    }

    private void OnEnable()
    {
        if (rb2D != null)
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    private void OnDisable()
    {
        if (rb2D != null)
        {
            rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }

    private Vector3 UpdateWaypoint()
    {
        if (target != null)
        {
            if (shouldOrbit)
            {
                waypoint = orbitComponent.GetOrbitPosition();
            }
            else
            {
                waypoint = target.position;
            }
        }
        return waypoint;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(waypoint, 0.1f);
        Gizmos.DrawWireSphere(waypoint, deadzone);
        if (shouldOrbit)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(orbitComponent.OrbitCenter.transform.position, orbitComponent.OrbitDistance);
        }
    }

    void FixedUpdate()
    {
        UpdateWaypoint();
        // Predict position
        Vector3 predictedPosition = Vector3.zero;
        predictedPosition.x = transform.position.x + rb2D.velocity.x * Time.fixedDeltaTime;
        predictedPosition.y = transform.position.y + rb2D.velocity.y * Time.fixedDeltaTime;

        // Calculate heading
        Vector3 deltaPosition = waypoint - transform.position;
        distanceToWaypoint = Vector3.Magnitude(deltaPosition);
        Vector3 predictedDeltaPosition = waypoint - predictedPosition;

        Quaternion desiredRotation = Quaternion.LookRotation(Vector3.forward, predictedDeltaPosition);
        Vector3 desiredAngles = desiredRotation.eulerAngles;

        float newHeading = heading;
        if (state == PropulsionState.Eco)
        {
            newHeading = desiredAngles.z;
        }
        else
        {
            // limit rotation speed
            float deltaAngle = Mathf.DeltaAngle(heading, desiredAngles.z);
            if (Mathf.Abs(deltaAngle) > rotationSpeed)
            {
                // limit rotation
                if (deltaAngle > 0)
                {
                    newHeading = heading + rotationSpeed;
                }
                else
                {
                    newHeading = heading - rotationSpeed;
                }
            }
            else
            {
                newHeading = desiredAngles.z;
            }
        }

        // Brake vs. Thrust
        float desiredThrust = maxThrust;
        if (state == PropulsionState.Eco)
        {
            desiredThrust *= 0.1f;
        }
        if (false && Mathf.Abs(newHeading - heading) > 90f)
        {
            newHeading = (newHeading + 180f) % 360f;
            desiredThrust = -1 * desiredThrust;
        }
        heading = newHeading;

        // Apply thrust
        transform.rotation = Quaternion.AngleAxis(heading, Vector3.forward);

        Vector3 forceDirection = new Vector3(0f, 0f, 0f);
        forceDirection = Quaternion.AngleAxis(heading, Vector3.forward) * Vector3.up;
        rb2D.AddForce(Vector3.Normalize(forceDirection) * desiredThrust, ForceMode2D.Impulse);

        // Limit velocity
        if (state == PropulsionState.Eco)
        {
            rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, cruisingSpeed);
        }
        else
        {
            rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, maxSpeed);
        }

        // Set heading
        if (state == PropulsionState.Eco)
        {
            // If in Eco mode, heading does not reflect physics (ship is using vector thrusters, not just main)
            if (shouldOrbit == false && distanceToWaypoint < deadzone)
            {
                // if station-keeping, align with target
                if (target != null)
                {
                    transform.rotation = target.transform.rotation;
                }
                else
                {
                    transform.rotation = startingRotation;
                }
            }
            else
            {
                // For normal patrolling, point towards waypoint 
                transform.rotation = Quaternion.LookRotation(Vector3.forward, deltaPosition);
            }
        }
        heading = transform.rotation.eulerAngles.z;
 
        if (regionLockCenter != null)
        {
            Vector3 distanceToRegion = transform.position - regionLockCenter.transform.position;
            if (distanceToRegion.magnitude > regionLockRadius)
            {
                transform.position = regionLockCenter.transform.position + regionLockRadius * distanceToRegion.normalized;
            }
        }
    }
}
