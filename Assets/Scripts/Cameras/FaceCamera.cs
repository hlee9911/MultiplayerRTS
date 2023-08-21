using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform mainCameraTransform;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }

    // rotate camera after moving?
    void LateUpdate()
    {
        FaceTowardsCamera();
    }

    void FaceTowardsCamera()
    {
        // first arg, relative to position, forward vec
        // second arg, up
        transform.LookAt(
            transform.position + mainCameraTransform.rotation * Vector3.forward,
            mainCameraTransform.rotation * Vector3.up);
    }
}
