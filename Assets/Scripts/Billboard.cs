using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Billboard : MonoBehaviour
{
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
    }

    //Orient the camera after all movement is completed this frame to avoid jittering
    private void LateUpdate()
    {
        transform.LookAt(transform.position + camera.transform.rotation * Vector3.forward,
            camera.transform.rotation * Vector3.up);
    }
}