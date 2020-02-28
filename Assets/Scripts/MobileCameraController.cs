using System.Collections;
using UnityEngine;
using UnityEngine.XR;

public class MobileCameraController : MonoBehaviour
{
    #region Settings/Variables

    [Header("Control Settings")]
    public bool useMobileSetup = false;

    [Header("Mouse/Keyboard Settings")]
    public float arrowMouseSpeed = 1.0f;

    [Header("Touch Settings")]
    public Vector2 touchSpeed = Vector2.one;

    [Header("Gyro Settings")]
    public bool useGyroOnAwake = false;

    // --- Variables ---

    //Can Use Touch
    private bool isTouchDevice;

    //Move Parameters
    float mouseX;
    float mouseY;
    private Quaternion localRotation;
    private Vector3 localEulerRotation = Vector3.zero; // rotation around the up/y axis & rotation around the right/x axis

    // --- Gyro ---
    private Gyroscope gyro;
    private Quaternion gyroRotation;
    private bool gyroInitalized = false;
    private Quaternion lastGyroRotation;
    private Vector3 gyroDestinationEuler = Vector3.zero;

    // --- Touch ---
    private Touch initTouch;

    #endregion

    // Use this for initialization
    private void Start()
    {
        //check if touch device
        isTouchDevice = (SystemInfo.deviceType == DeviceType.Handheld);

        //Get local rotation
        localRotation = transform.localRotation;
        localEulerRotation = transform.localEulerAngles;

        if(!useMobileSetup && isTouchDevice)
            useMobileSetup = true;

        if(SystemInfo.supportsGyroscope && useGyroOnAwake)
            EnableGyro();
    }

    // Update is called once per frame
    private void Update()
    {
        if(useMobileSetup)
        {
            //Rotate Camera with Accelerometer
            UpdateTouchInput(Time.deltaTime);

            if(gyroInitalized)
                GyroUpdate(Time.deltaTime);
        }
        else
        {
            //Rotate Camera with Keyboard Arrow and Mouse
            MoveWithArrowAndMouse(Time.deltaTime);
        }

        if(Input.GetKeyDown(KeyCode.Escape))
            CallExitCommand();
    }

    #region Desktop Movement

    //Move with Keyboard Arrow
    private void MoveWithArrowAndMouse(float dt)
    {
        //Keyboard Arrow
        MoveCamera(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), arrowMouseSpeed, arrowMouseSpeed);

        //Keyboard Mouse
        MoveCamera(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), arrowMouseSpeed, arrowMouseSpeed);
    }

    private void MoveCamera(float horizontal, float verticle, float moveSpeedX, float moveSpedY)
    {
        mouseX = horizontal;
        mouseY = -verticle;

        localEulerRotation.y += mouseX * moveSpeedX;
        localEulerRotation.x += mouseY * moveSpedY;
        localEulerRotation.x = Mathf.Clamp(localEulerRotation.x, -45f, 45f);

        localRotation = Quaternion.Euler(localEulerRotation.x, localEulerRotation.y, 0.0f);
        transform.localRotation = localRotation;
    }

    public void ResetOrientation()
    {
        localEulerRotation = Vector3.zero;
        transform.localRotation = localRotation = Quaternion.identity;
    }

    #endregion

    #region Gyro

    public void ToogleGyro()
    {
        if(!SystemInfo.supportsGyroscope)
            return;

        if(gyroInitalized)
            DisableGyro();
        else
            EnableGyro();
    }

    public void EnableGyro()
    {
        gyro = Input.gyro;
        gyro.enabled = true;
        gyroInitalized = true;

        Quaternion newGyroRotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);

        gyroDestinationEuler = localEulerRotation - newGyroRotation.eulerAngles;
        gyroDestinationEuler.x = ((int)(gyroDestinationEuler.x * 10)) / 10f;
        gyroDestinationEuler.y = ((int)(gyroDestinationEuler.y * 10)) / 10f;
        gyroDestinationEuler.z = ((int)(gyroDestinationEuler.z * 10)) / 10f;

        localRotation = Quaternion.Euler(gyroDestinationEuler);
        localEulerRotation = gyroDestinationEuler;

        lastGyroRotation = newGyroRotation;
    }

    public void DisableGyro()
    {
        GyroUpdate(Time.deltaTime);
        localRotation = transform.rotation;
        localEulerRotation = transform.localEulerAngles;

        gyro.enabled = false;
        gyroInitalized = false;
    }

    private void GyroUpdate(float dt)
    {
        Quaternion newGyroRotation = new Quaternion(0.5f, 0.5f, -0.5f, 0.5f) * Input.gyro.attitude * new Quaternion(0, 0, 1, 0);

        gyroDestinationEuler = localEulerRotation + newGyroRotation.eulerAngles;
        gyroDestinationEuler.x = ((int)(gyroDestinationEuler.x * 10)) / 10f;
        gyroDestinationEuler.y = ((int)(gyroDestinationEuler.y * 10)) / 10f;
        gyroDestinationEuler.z = ((int)(gyroDestinationEuler.z * 10)) / 10f;

        transform.localEulerAngles = gyroDestinationEuler;
        lastGyroRotation = newGyroRotation;
    }

    #endregion

    #region Touch

    private void UpdateTouchInput(float dt)
    {
        foreach(Touch touch in Input.touches)
        {
            if(touch.phase == TouchPhase.Began)
            {
                initTouch = touch;
            }
            else if(touch.phase == TouchPhase.Moved)
            {
                Vector2 deltaPosition = initTouch.position - touch.position;
                deltaPosition.Normalize();
                MoveCamera(deltaPosition.x, deltaPosition.y, touchSpeed.x, touchSpeed.y);
                initTouch = touch;
            }
            else if(touch.phase == TouchPhase.Ended)
            {
                initTouch = new Touch();
            }
        }
    }

    #endregion

    #region RenderMode

    public void ToogleXR()
    {
        if(XRSettings.enabled)
            DisableXR();
        else
            EnableCardboard();
    }

    public void EnableCardboard()
    {
        XRSettings.enabled = true;
        StartCoroutine(LoadDevice("Cardboard"));
    }

    public void DisableXR()
    {
        XRSettings.enabled = false;
    }

    private IEnumerator LoadDevice(string newDevice)
    {
        if(System.String.Compare(XRSettings.loadedDeviceName, newDevice, true) != 0)
        {
            XRSettings.LoadDeviceByName(newDevice);
            yield return null;
            XRSettings.enabled = true;
        }
    }

    #endregion

    #region Application Controls

    public void CallExitCommand()
    {
        if(XRSettings.enabled)
            DisableXR();
        else
            Application.Quit();
    }

    #endregion
}