using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


public class CarrierMove : MonoBehaviour
{
    // Movement
    float thrust = 16f;
    float heading = 0f;
    private Rigidbody2D rb2D;
    public float rotationSpeed = 1f;

    // Audio
    public AudioSource audioSource;

    // Cache the child object for the thrust animation.
    private Renderer exhaustRenderer = null;
    private bool thrusting = false;

    // Flags to handle player input
    public bool inputThrust = false;
    private bool inputTurnRight = false;
    private bool inputTurnLeft = false;
    private bool inputBrake = false;

    void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        exhaustRenderer = transform.Find("Exhaust").gameObject.GetComponent<Renderer>();
        if (exhaustRenderer != null)
        {
            exhaustRenderer.enabled = false;
        }
        heading = transform.eulerAngles.z;
    }

    void FixedUpdate()
    {
        Vector3 forceDirection = new Vector3(0f, 0f, 0f);
        float maxSpeed = 5f;
        float decel = 0.99f;
        bool stopThrusting = true;

        if (inputThrust)
        {
            stopThrusting = false;
            if (!thrusting)
            {
                if (exhaustRenderer != null)
                {
                    exhaustRenderer.enabled = true;

                }
                if (audioSource != null)
                {
                    audioSource.Play();
                }
            }
            thrusting = true;
        }
        if (inputTurnLeft)
        {
            heading += rotationSpeed;
        }
        if (inputBrake)
        {
            rb2D.velocity *= decel * decel;
        }
        if (inputTurnRight)
        {
            heading -= rotationSpeed;
        }

        if (thrusting && stopThrusting)
        {
            if (exhaustRenderer != null)
            {
                exhaustRenderer.enabled = false;
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }
            thrusting = false;
        }

        transform.rotation = Quaternion.AngleAxis(heading, Vector3.forward);
        forceDirection += Quaternion.AngleAxis(heading, Vector3.forward) * Vector3.up;


        if (!thrusting)
        {
            rb2D.velocity *= decel;
        }
        else
        {
            // Add Force
            rb2D.AddForce(Vector3.Normalize(forceDirection) * thrust);
        }

        // Limit velocity
        rb2D.velocity = Vector2.ClampMagnitude(rb2D.velocity, maxSpeed);
    }

    // Helper routines for handling player input.
    public void ReadThrust(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputThrust = true;
        }
        else if (context.canceled)
        {
            inputThrust = false;
        }
    }

    public void ReadTurnRight(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputTurnRight = true;
        }
        else if (context.canceled)
        {
            inputTurnRight = false;
        }
    }
    public void ReadTurnLeft(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputTurnLeft = true;
        }
        else if (context.canceled)
        {
            inputTurnLeft = false;
        }
    }
    public void ReadBrake(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputBrake = true;
        }
        else if (context.canceled)
        {
            inputBrake = false;
        }
    }

}
