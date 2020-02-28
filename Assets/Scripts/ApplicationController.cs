using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using Utils;

public class ApplicationController : MonoBehaviour
{
    public MobileCameraController mobileCameraController;
    public LocationManager locationManager;

    public UnityEngine.UI.Image pointer;
    public UnityEngine.UI.Image gaze;

    private Timer gazeTimer = new Timer(3f);
    private bool gazeLookAt = true;

    private Transform currentTarget = null;

    private void Awake()
    {
        EnablePointer();

        if(Application.isMobilePlatform || !XRSettings.isDeviceActive)
            SetupForTouchAndGyro();
        else
            SetupForHMD();
    }

    private void Update()
    {
        for(int i = 0; i < 9; i++)
        {
            if(Input.GetKeyDown(KeyCode.Alpha1 + i))
                locationManager.StartLocationTween(i, null);
        }
    }

    private void FixedUpdate()
    {
        RaycastHit hit;
         
        if(Physics.Raycast(mobileCameraController.transform.position, mobileCameraController.transform.forward, out hit, Mathf.Infinity, LayerMask.GetMask("Marker")))
        {
            if(!gazeLookAt && hit.transform != locationManager.tweenTarget && hit.transform != currentTarget)
            {
                gazeTimer.Reset();
                currentTarget = hit.transform;
                gazeLookAt = true;
                locationManager.Stop();
                EnableGaze();
            }

            if(gazeTimer.Update(Time.fixedDeltaTime))
            {
                locationManager.StartLocationTween(hit.transform, LocationTweenComplete);
                gazeLookAt = false;
                EnablePointer();
            }
            else if(gazeLookAt)
            {
                gaze.fillAmount = gazeTimer.percentage;
            }

            Debug.DrawRay(mobileCameraController.transform.position, mobileCameraController.transform.forward * 4000f, Color.green);
        }
        else
        {
            gazeLookAt = false;

            if(!locationManager.processTween)
                currentTarget = null;

            EnablePointer();
            Debug.DrawRay(mobileCameraController.transform.position, mobileCameraController.transform.forward * 4000f, Color.red);
        }

        locationManager.Loop(Time.fixedDeltaTime);
    }

    private void LocationTweenComplete()
    {
        currentTarget = null;
    }

    private void EnablePointer()
    {
        gaze.fillAmount = 0f;
        pointer.enabled = true;
    }

    private void EnableGaze()
    {
        gaze.fillAmount = 0f;
        pointer.enabled = false;
    }

    #region Setup

    private void SetupForTouchAndGyro()
    {
        mobileCameraController.enabled = true;
        mobileCameraController.transform.localPosition = (Vector3.up * 1.8f);
    }

    private void SetupForHMD()
    {
        mobileCameraController.enabled = false;
        mobileCameraController.transform.localPosition = Vector3.zero;
    }

    #endregion
}
