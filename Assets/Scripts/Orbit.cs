using System.Threading;
using UnityEngine;

// Script to move an object in an orbit around another object.
// Does not use physics, but the modifies position directly.
// Orbit distance is determined by starting position.
public class Orbit : MonoBehaviour
{
    // The transform to orbit around.
    public Transform OrbitCenter = null;
    // The "speed" to orbit the object.
    // Time for an orbit in seconds is (2 * pi / OrbitSpeed).
    public float OrbitSpeed = 1f;
    // true if script should move the object,
    // false if just calling routines
    public bool AutoOrbit = true;

    // Variables for tracking orbital position.
    private float timeStart = 0;
    public float OrbitDistance = 0f;
    private float OrbitAngle = 0f;

    // Start is called before the first frame update
    void Start()
    {
        if (AutoOrbit)
        {
            InitializeOrbit(null, transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (AutoOrbit)
        {
            transform.position = GetOrbitPosition();
        }
    }

    public void InitializeOrbit(Transform newOrbitCenter, Vector3 startLocation, float objectSpeed)
    {
        Transform myOrbitCenter = null;
        float myOrbitDistance = 0f;

        myOrbitCenter = OrbitCenter;
        if (newOrbitCenter != null)
        {
            myOrbitCenter = newOrbitCenter;
        }
        if (myOrbitCenter != null)
        {
            myOrbitDistance = Vector3.Magnitude(startLocation - myOrbitCenter.position);
            OrbitSpeed = objectSpeed / myOrbitDistance;
        }
        InitializeOrbit(newOrbitCenter, startLocation);
    }

    // Initialize orbit and starting phase
    public void InitializeOrbit(Transform newOrbitCenter, Vector3 startLocation)
    {
        timeStart = Time.time;
        if (newOrbitCenter != null)
        {
            OrbitCenter = newOrbitCenter;
        }
        if (OrbitCenter != null)
        {
            Vector3 startPosition;
            if (startLocation != null)
            {
                startPosition = startLocation;
            }
            else
            {
                startPosition = transform.position;
            }

            OrbitDistance = Vector3.Magnitude(startPosition - OrbitCenter.position);
            OrbitAngle = Mathf.Deg2Rad * Vector3.SignedAngle(Vector3.right,
                transform.position - OrbitCenter.position, Vector3.forward);
        }
    }

    // Calculate desired orbital location
    public Vector3 GetOrbitPosition()
    {
        if (OrbitCenter == null)
        {
            return transform.position;
        }
        float elapsedTime = Time.time - timeStart;
        Vector3 orbitPosition = OrbitCenter.position;
        orbitPosition.x += OrbitDistance * Mathf.Cos(OrbitSpeed * elapsedTime + OrbitAngle);
        orbitPosition.y += OrbitDistance * Mathf.Sin(OrbitSpeed * elapsedTime + OrbitAngle);
        orbitPosition.z = 0f;
        return orbitPosition;
    }

}
