using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
 
public class CameraFollow : MonoBehaviour
{
    // The object the camera should follow
    public GameObject followObject = null;
    // Convergence time for the camera. Larger is smoother.
    public float smoothTime = 10f;

    public float minZoom = 2.0f;
    public float maxZoom = 9.0f;

    // Updated by SmoothDamp, starts at zero.
    private Vector3 velocity = Vector3.zero;

    private bool inputZoomIn = false;
    private bool inputZoomOut = false;

    private float cameraZoom = 4.0f;
    private const float cameraStep = 0.1f;

    void FixedUpdate()
    {
        if (inputZoomIn)
        {
            cameraZoom -= cameraStep;
        }

        if (inputZoomOut)
        {
            cameraZoom += cameraStep;
        }
        cameraZoom = Mathf.Clamp(cameraZoom, minZoom, maxZoom);
        GetComponent<Camera>().orthographicSize = Mathf.Pow(2.0f, cameraZoom);
        // Smoothly follow the object
        Vector3 targetPosition = followObject.transform.TransformPoint(new Vector3(0, 0, -10.0f));
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

    }

    public void ReadZoomIn(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputZoomIn = true;
        }
        else if (context.canceled)
        {
            inputZoomIn = false;
        }

    }

    public void ReadZoomOut(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            inputZoomOut = true;
        }
        else if (context.canceled)
        {
            inputZoomOut = false;
        }

    }


}
